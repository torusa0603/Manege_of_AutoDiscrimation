using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Matrox.MatroxImagingLibrary;

namespace MilControl
{
	/// <summary>
	/// MILを使用したカメラ画像取得実行クラス
	/// </summary>
	/// <remarks>
	/// MILHelp.chm内のMdigProcess.csを参考に作成
	/// </remarks>
	public class CCamera : CBase
	{
		#region クラス内定義
		new private const string		m_strDEVICE_NAME			= "MIL.Camera";		// デバイス名
		private const int				m_iGRABBUFFER_SIZE_MAX		= 20;				// 画像取得バッファ最大数
		private const int				m_iLISTIMAGE_SIZE_MAX		= 20;				// m_MilListImageGrab最大数
		#endregion


		#region Process()用情報クラス
		public class HookDataStruct
		{
			public	MIL_ID				MilDigitizer;
			public	MIL_ID				MilImageDisp;
			public	List< MIL_ID >		MilListImageGrab;
			public	double				FrameRate;
			public	Mutex				mtxListImageGrab;
			public	Mutex				mtxFrameRate;
		};
		#endregion


		#region ローカル変数
		private		MIL_ID[]			m_MilGrabBuffer				= new MIL_ID[ m_iGRABBUFFER_SIZE_MAX ];
		private		MIL_ID				m_MilMonoBuffer				= MIL.M_NULL;					// モノクロ受け渡し用
		private		int					m_MilCountGrabBuffer		= 0;
		private		List< MIL_ID >		m_MilListImageGrab			= new List< MIL_ID >();
		private		Mutex				m_mtxListImageGrab			= new Mutex();					// ListImageGrab操作用
		private		Mutex				m_mtxFrameRate				= new Mutex();					// FrameRate操作用
		private		HookDataStruct		m_tagHookData				= new HookDataStruct();			// Process()用情報クラス
		private		bool				m_bThrough					= false;
		// MdigProcess()用変数
		private		GCHandle 					m_hUserData;
		private		MIL_DIG_HOOK_FUNCTION_PTR	m_funcPtr;
		#endregion


		#region プロパティ
		public		string	DeviceVendorName { get; private set; }	= "";		// Device名(Ex.Basler)
		#endregion


		#region プログラム開始時終了時
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CCamera()
		{
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CCamera()
		{
			// デバイス名
			m_strDeviceName		= m_strDEVICE_NAME;
		}
		#endregion


		#region オープン/クローズ関数
		/// <summary>
		/// クローズ
		/// </summary>
		/// <returns>true:成功</returns>
		public new bool close()
		{
			bool			b_ret	= false;
			string 			str_log;

			try
			{
				while( true )
				{
					// オープンしているか?
					if( false == is_open() )
					{
						break;
					}
					// 画像取得バッファの解放
					while( 0 < m_MilCountGrabBuffer )
					{
						m_MilCountGrabBuffer--;
						MIL.MbufClear( m_MilGrabBuffer[ m_MilCountGrabBuffer ], 0 );
						MIL.MbufFree( m_MilGrabBuffer[ m_MilCountGrabBuffer ] );
						m_MilGrabBuffer[ m_MilCountGrabBuffer ]		= MIL.M_NULL;
					}
					// エラー確認
					if( true == is_mil_error() )	break;
					// モノクロ受け渡し用解放
					if( MIL.M_NULL != m_MilMonoBuffer )
					{
						MIL.MbufClear( m_MilMonoBuffer, 0 );
						MIL.MbufFree( m_MilMonoBuffer );
						m_MilMonoBuffer		= MIL.M_NULL;
					}
					// エラー確認
					if( true == is_mil_error() )	break;
					// baseのクローズ実行
					if( false == base.close() )
					{
						break;
					}
					// 完了
					b_ret		= true;
					break;
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed close! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}
			return	b_ret;
		}


		/// <summary>
		/// オープン
		/// </summary>
		/// <returns>true:成功</returns>
		public bool open()
		{
			bool			b_ret	= false;
			string 			str_log;

			try
			{
				while( true )
				{
					// baseのオープン実行
					if( false == base.open( true ) )
					{
						break;
					}
					// 画像取得バッファの確保 & クリア
					MIL_INT		i_width				= m_cParaMilControl.ImageSizeWidth;
					MIL_INT		i_height			= m_cParaMilControl.ImageSizeHeight;
					for( int i_loop = 0; i_loop < m_iGRABBUFFER_SIZE_MAX; i_loop++ )
					{
						MIL.MbufAllocColor( m_MilSystem, 3, i_width, i_height,
											8 + MIL.M_UNSIGNED,
											MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC,
											ref m_MilGrabBuffer[ i_loop ] );
						m_MilCountGrabBuffer	= i_loop + 1;
						if( MIL.M_NULL != m_MilGrabBuffer[ i_loop ] )
						{
							MIL.MbufClear( m_MilGrabBuffer[ i_loop ], 0xFF );
						}
						else
						{
							break;
						}
					}
					// 確保できているか確認
					if( 0 == m_MilCountGrabBuffer )
					{
						str_log			= "Failed GrabBuffer memory allocation!";
						setLogError( str_log );
						break;
					}
					// エラー確認
					if( true == is_mil_error() )	break;
					// モノクロ受け渡し用確保
					MIL.MbufAlloc2d( m_MilSystem, i_width, i_height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_MilMonoBuffer );
					MIL.MbufClear( m_MilMonoBuffer, 0 );
					// エラー確認
					if( true == is_mil_error() )	break;

					// FrameRateの設定(備忘録、呼び出し方は正しいが、実行で失敗する。Helpによるとgigeでは使えない様子)
					/*{ 
						MIL.MdigControl( m_MilDigitizer, MIL.M_SELECTED_FRAME_RATE , MIL.M_DEFAULT  );
						//MIL.MdigControl( m_MilDigitizer, MIL.M_SELECTED_FRAME_RATE , 30  );
					}*/

					// デバイス名取得
					StringBuilder	sb_device		= new StringBuilder();
					MIL.MdigInquireFeature( m_MilDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, sb_device );
					DeviceVendorName				= sb_device.ToString();
					// エラー確認
					if( true == is_mil_error() )	break;

					//Process()用情報クラス初期化
					m_tagHookData.MilDigitizer		= m_MilDigitizer;
					m_tagHookData.MilImageDisp		= MIL.M_NULL;
					m_tagHookData.MilListImageGrab	= m_MilListImageGrab;
					m_tagHookData.mtxListImageGrab	= m_mtxListImageGrab;
					m_tagHookData.mtxFrameRate		= m_mtxFrameRate;
					// 完了
					b_ret			= true;
					break;
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed open! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}
			return	b_ret;
		}
		#endregion


		#region スルー実行 / 終了
		/// <summary>
		/// カメラスルー実行
		/// </summary>
		/// <returns>true:成功</returns>
		public bool do_through()
		{
			bool			b_ret	= false;
			string 			str_log;

			try
			{
				if( true == is_open() && false == m_bThrough )
				{
					// 取得Process開始
					m_hUserData		= GCHandle.Alloc( m_tagHookData );
					m_funcPtr		= new MIL_DIG_HOOK_FUNCTION_PTR( ProcessingFunction );
					MIL.MdigProcess( m_MilDigitizer, m_MilGrabBuffer, m_MilCountGrabBuffer,
										MIL.M_START, MIL.M_DEFAULT, 
										m_funcPtr, GCHandle.ToIntPtr( m_hUserData ) );
					setLogDevice( "Do through." );
					m_bThrough		= true;
					b_ret			= true;
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed through! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
			}
			return	b_ret;
		}


		/// <summary>
		/// カメラスルー終了
		/// </summary>
		/// <returns>true:成功</returns>
		public bool do_frezze()
		{
			bool			b_ret	= false;
			string 			str_log;

			try
			{
				if( true == is_open() && true == m_bThrough )
				{
					// 取得Process終了
					MIL.MdigProcess( m_MilDigitizer, m_MilGrabBuffer, m_MilCountGrabBuffer,
										MIL.M_STOP + MIL.M_WAIT, MIL.M_DEFAULT, 
										m_funcPtr, GCHandle.ToIntPtr( m_hUserData ) );
					setLogDevice( "Do frezze." );
					m_bThrough		= false;
					b_ret			= true;
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed frezze! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
			}
			return	b_ret;
		}


		/// <summary>
		/// プロセス処理
		/// </summary>
		/// <returns>true:成功</returns>
		static MIL_INT ProcessingFunction( MIL_INT niHookType, MIL_ID nHookId, IntPtr npiHookData )
		{
			MIL_ID		mil_modified_image	= MIL.M_NULL;

			if( !IntPtr.Zero.Equals( npiHookData ) )
			{
				// Process()用情報クラス実体化
				GCHandle		hUserData		= GCHandle.FromIntPtr( npiHookData );
				HookDataStruct	tagHookData 	= hUserData.Target as HookDataStruct;

				// 変更されたバッファIDを取得する
				MIL.MdigGetHookInfo( nHookId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image );

				// コピー表示
				if( MIL.M_NULL != tagHookData.MilImageDisp )
				{
					MIL.MbufCopy( mil_modified_image, tagHookData.MilImageDisp );
				}

				// リストへ追加
				if( null != tagHookData.MilListImageGrab && null != tagHookData.mtxListImageGrab )
				{
					tagHookData.mtxListImageGrab.WaitOne();
					tagHookData.MilListImageGrab.Add( mil_modified_image );
					while( CCamera.m_iLISTIMAGE_SIZE_MAX < tagHookData.MilListImageGrab.Count )
					{
						tagHookData.MilListImageGrab.RemoveAt( 0 );
					}
					tagHookData.mtxListImageGrab.ReleaseMutex();
				}

				// frame rate表示(デバッグ用)
				double		d_rate		= 0;
				MIL.MdigInquire( tagHookData.MilDigitizer, MIL.M_PROCESS_FRAME_RATE, ref d_rate );
				if( null != tagHookData.mtxFrameRate )
				{
					tagHookData.mtxFrameRate.WaitOne();
					tagHookData.FrameRate	= d_rate;
					tagHookData.mtxFrameRate.ReleaseMutex();
				}
				/*{
					MIL_INT		i_count		= 0;
					MIL.MdigInquire( tagHookData.MilDigitizer, MIL.M_PROCESS_FRAME_COUNT, ref i_count );
					string	str_log			= "[CAMERA]" + i_count.ToString() + "frames grabbed at! ";
					str_log					+= d_rate.ToString( "F2" ) + "[frames/sec] "; 
					str_log					+= ( 1000.0 / d_rate ).ToString( "F2" ) + "[ms/frame]"; 
					System.Diagnostics.Debug.WriteLine( str_log );
				}*/

				// 設定frame rateの取得(デバッグ用)
				/*{
					MIL.MdigInquire( tagHookData.MilDigitizer, MIL.M_SELECTED_FRAME_RATE , ref d_rate );
				}*/
			}

			return 0;
		}
		#endregion


		#region メンバ関数
		/// <summary>
		/// 白黒画像データ取得(bit straem)
		/// </summary>
		/// <param name="nlArySize">nbyDataサイズ(画像幅×高さ[pixce]になるはず)</param>
		/// <param name="nbyData">画像データ</param>
		/// <param name="nbLast">true:一番古いデータを取得</param>
		/// <returns>true:成功</returns>
		public bool get_mono_bitmap_data( long nlArySize, ref byte[] nbyData, bool nbLast )
		{
			bool			b_ret			= false;
			string 			str_log;
			MIL_ID			mil_image		= MIL.M_NULL;

			try
			{
				while( true == is_open() )
				{
					// ユーザが準備した画像バッファサイズが少ない
					MIL_INT		i_width		= m_cParaMilControl.ImageSizeWidth;
					MIL_INT		i_height	= m_cParaMilControl.ImageSizeHeight;
					if( nlArySize < i_width * i_height )
					{
						break;
					}
					// ロック
					m_mtxListImageGrab.WaitOne();
					// 最新 or 最古のIDを取得
					while( 0 < m_MilListImageGrab.Count )
					{
						mil_image			= m_MilListImageGrab[ 0 ];
						if( true == nbLast )
						{
							break;
						}
						m_MilListImageGrab.RemoveAt( 0 );
					}
					// リリース
					m_mtxListImageGrab.ReleaseMutex();
					// 画像が無ければ
					if( MIL.M_NULL == mil_image )
					{
						break;
					}
					// モノクロ画像バッファクリア
					MIL.MbufClear( m_MilMonoBuffer, 0 );
					// モノクロ画像バッファコピー
					MIL.MbufCopy( mil_image, m_MilMonoBuffer );
					// モノクロ画像バッファ ビットマップデータ取得
					MIL.MbufGet2d( m_MilMonoBuffer, 0, 0, i_width, i_height, nbyData );
					// エラー確認
					if( true == is_mil_error() )	break;
					b_ret					= true;
					break;
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed get mono bitmap data! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
			}

			return	b_ret;
		}


		/// <summary>
		/// ProcessingFunction()でのフレームレート結果の取得
		/// </summary>
		public double get_process_frame_rate()
		{
			double	d_ret	= 0;
			m_mtxFrameRate.WaitOne();
			d_ret		= m_tagHookData.FrameRate;
			m_mtxFrameRate.ReleaseMutex();
			return	d_ret;
		}
		#endregion
	}
}
