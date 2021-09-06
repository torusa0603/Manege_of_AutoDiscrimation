using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Matrox.MatroxImagingLibrary;

namespace MilControl
{
    /// <summary>
	/// MILを使用した制御のベースクラス
	/// </summary>
	public class CBase : LogBase.CDeviceControlLog
	{
		#region クラス内定義
		new private const string	m_strDEVICE_NAME	= "MIL.Base";	// デバイス名
		#endregion


		#region ローカル変数
		private				bool	m_bIamOpener		= false;		// オープンしたのが自分自身
		static protected	bool	m_bOpend			= false;		// オープン済み？

		static protected	MIL_ID	m_MilApplication	= MIL.M_NULL;	// [必須]Application identifier.
		static protected	MIL_ID	m_MilSystem			= MIL.M_NULL;	// [必須]System identifier.
		static protected	MIL_ID	m_MilDisplay		= MIL.M_NULL;	// [必須]Display identifier.
		static protected	MIL_ID	m_MilDigitizer		= MIL.M_NULL;	// [必須]Digitizer identifier.

		static protected	MIL_ID	m_MilShowImage		= MIL.M_NULL;	// ユーザー画面へ表示する為のID
		static protected	IntPtr	m_hDisplayHandle	= IntPtr.Zero;	// ユーザー画面表示用ウインドウハンドル(FormかPanelのハンドル)
		#endregion


		#region プロパティ
		public	bool	Display { get; set; }			= false;		// 画像を表示するか？(MIL又はユーザプログラムへ)
		#endregion


		#region パラメータクラス
		protected CParaMilControl	m_cParaMilControl	= CParaMilControl.getInstance();
		#endregion


		#region プログラム開始時終了時
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CBase()
		{
			close();
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CBase()
		{
			// デバイス名
			m_strDeviceName		= m_strDEVICE_NAME;
		}
		#endregion


		#region オープン/クローズ関数
		/// <summary>
		/// オープン確認用
		/// </summary>
		public bool is_open()
		{
			return		m_bOpend;
		}


		/// <summary>
		/// クローズ
		/// </summary>
		/// <returns>true:成功</returns>
		public bool close()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// ユーザー画面へ表示する為のID
				if( m_MilShowImage != MIL.M_NULL )
				{
					MIL.MbufClear( m_MilShowImage, 0 );
					MIL.MbufFree( m_MilShowImage );
					m_MilShowImage		= MIL.M_NULL;
				}
				// デフォルトのMILオブジェクト(必須ID)を解放
				if( MIL.M_NULL != m_MilApplication && true == m_bIamOpener )
				{
					// デフォルトのMILオブジェクトを解放
					MIL.MappFreeDefault( m_MilApplication, m_MilSystem, m_MilDisplay, m_MilDigitizer, MIL.M_NULL );
					m_MilApplication	= MIL.M_NULL;
					m_MilSystem			= MIL.M_NULL;
					m_MilDisplay		= MIL.M_NULL;
					m_MilDigitizer		= MIL.M_NULL;
					m_bIamOpener		= false;
					m_bOpend			= false;

					// エラーフックの解除
					// clearHookError();

					// ログ残し
					str_log			= "Close.";
					setLogExecute( str_log );
				}
				// パラメータファイル保存
				if( false == m_cParaMilControl.writeXmlFile( out str_log ) )
				{
					setLogError( str_log );
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
		/// <param name="nbDigitizer">Digitizer(カメラ)を使用するか</param>
		/// <returns>true:成功</returns>
		public bool open( bool nbDigitizer )
		{
			bool			b_ret				= true;
			string 			str_log;

			try
			{
				// デフォルトのMILオブジェクト(必須ID)を割り当て
				while( MIL.M_NULL == m_MilApplication )
				{
					// ログ残し
					str_log						= "Open.";
					setLogExecute( str_log );
					// パラメータ読み込み
					if( false == m_cParaMilControl.readXmlFile( out str_log ) )
					{
						setLogError( str_log );
						b_ret					= false;
						break;
					}
					// エラーフックの設定
					// setHookError();
					// デフォルトの設定
					if( false == nbDigitizer )
					{
						MIL.MappAllocDefault( MIL.M_DEFAULT, ref m_MilApplication, ref m_MilSystem, ref m_MilDisplay, MIL.M_NULL, MIL.M_NULL );
					}
					else
					{
						MIL.MappAllocDefault( MIL.M_DEFAULT, ref m_MilApplication, ref m_MilSystem, ref m_MilDisplay, ref m_MilDigitizer, MIL.M_NULL );
					}
					if( true == is_mil_error() )
					{
						// MILオブジェクトを解放
						MIL_ID	MilApplication	= m_MilApplication;
						MIL_ID	MilSystem		= m_MilSystem;
						MIL_ID	MilDisplay		= m_MilDisplay;
						MIL_ID	MilDigitizer	= m_MilDigitizer;
						MIL_ID	MilImageBufId	= MIL.M_NULL;
						MIL.MappFreeDefault( MilApplication, MilSystem, MilDisplay, MilDigitizer, MilImageBufId );
						m_MilApplication		= MIL.M_NULL;
						m_MilSystem				= MIL.M_NULL;
						m_MilDisplay			= MIL.M_NULL;
						m_MilDigitizer			= MIL.M_NULL;
						// ログ残し
						str_log					= "Can not open.";
						setLogError( str_log );
						b_ret					= false;
						break;
					}
					m_bIamOpener				= true;
					m_bOpend					= true;
					// 画像サイズ確認して設定
					if( true == nbDigitizer )
					{ 
						MIL_INT	i_width			= MIL.MdigInquire( m_MilDigitizer, MIL.M_SOURCE_SIZE_X, MIL.M_NULL );
						MIL_INT	i_height		= MIL.MdigInquire( m_MilDigitizer, MIL.M_SOURCE_SIZE_Y, MIL.M_NULL );
						if( m_cParaMilControl.ImageSizeWidth > i_width ||
							m_cParaMilControl.ImageSizeHeight > i_height )
						{
							str_log				= "Unmatch image size. Camara width=" + i_width.ToString() + "Camera height=" + i_height.ToString();
							setLogError( str_log );
							b_ret				= false;
							break;
						}
						i_width					= m_cParaMilControl.ImageSizeWidth;
						i_height				= m_cParaMilControl.ImageSizeHeight;
						MIL.MdigControl( m_MilDigitizer, MIL.M_SOURCE_SIZE_X, i_width );
						MIL.MdigControl( m_MilDigitizer, MIL.M_SOURCE_SIZE_Y, i_height );
					}
					// 終了
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


		#region エラーフック関連(未デバッグ)
		/// <summary>
		/// エラーフックに関する設定クリア
		/// </summary>
		private void clearHookError()
		{
			// 本クラスのポインター
			GCHandle hUserData		= GCHandle.Alloc( this );
			// フック関数のポインタ
			MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr		= new MIL_APP_HOOK_FUNCTION_PTR( hookErrorHandler );
			// 設定
			MIL.MappHookFunction( MIL.M_ERROR_CURRENT + MIL.M_UNHOOK, ProcessingFunctionPtr, GCHandle.ToIntPtr( hUserData ) );
		}


		/// <summary>
		/// エラーフックに関する設定(未完成、未デバッグ)
		/// </summary>
		/// <remarks>
		/// エラーフック関数によりエラー発生を取得したい時に使用するが、未デバッグ。
		/// 以下の設定をしなければMIL側でエラーダイアログを出してくれる。
		///  MIL.MappControl( MIL.M_ERROR, MIL.M_PRINT_DISABLE )
		/// MIL側で出してくれるエラーダイアログと、エラーフック関数で得られる情報は全く同じである為、
		/// エラーフック関数で取得するまでもない。(AlignerPrototype0)
		/// C#で実装するには情報が少なくて困難である事も、上記判断に至った理由。
		/// 参考にしたホームページ https://intra97.tistory.com/127
		/// </returns>
		private void setHookError()
		{
			// MILのエラーメッセージの制限設定
			MIL.MappControl( MIL.M_ERROR, MIL.M_PRINT_DISABLE );

			// 本クラスのポインター
			GCHandle hUserData		= GCHandle.Alloc( this );
			// フック関数のポインタ
			MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr		= new MIL_APP_HOOK_FUNCTION_PTR( hookErrorHandler );
			// 設定
			MIL.MappHookFunction( MIL.M_ERROR_CURRENT, ProcessingFunctionPtr, GCHandle.ToIntPtr( hUserData ) );

			// 参考のため残す
			// 設定(本クラスのポインタを渡さない場合)
			// MIL.MappHookFunction( MIL.M_ERROR_CURRENT, ProcessingFunctionPtr, IntPtr.Zero );
			// 設定(最初のエラーが発生した時にフック関数場合、MIL.M_ERROR_GLOBALを使用)
			// MIL.MappHookFunction( MIL.M_ERROR_GLOBAL, ProcessingFunctionPtr, IntPtr.Zero );
		}


		/// <summary>
		/// エラーフック関数(未完成、未デバッグ)
		/// </summary>
		/// <remarks>
		/// 本クラスのポインタ取得はMILHelp.chmを参照。
		/// 取得した文字列をどう扱うかは未実装。
		/// StringBuilderに関する所はMILHelp.chmのMappGetError()のMseqProcess.csを参考にすると良し。
		/// </returns>
		protected MIL_INT hookErrorHandler( MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr )
		{
			MIL_ID 			ModifiedBufferId	= MIL.M_NULL;
			StringBuilder	sb_function			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
			StringBuilder	sb_current			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
			StringBuilder	sb_errmsg1			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
			StringBuilder	sb_errmsg2			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
			StringBuilder	sb_errmsg3			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
			MIL_INT			i_subcode			= 0;

			// this is how to check if the user data is null, the IntPtr class
			// contains a member, Zero, which exists solely for this purpose
			if( !IntPtr.Zero.Equals( npUserDataPtr ) )
			{
				// get the handle to the DigHookUserData object back from the IntPtr
				GCHandle hUserData = GCHandle.FromIntPtr(npUserDataPtr);
				// get a reference to the DigHookUserData object
				CBase UserData = hUserData.Target as CBase;
				// Retrieve the MIL_ID of the grabbed buffer.
				MIL.MdigGetHookInfo( nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref ModifiedBufferId );

				/* 本コメントアウトを外すと「古い形式」とワーニングが出る。必要な場合、is_mil_error()を使用すれば様
				// エラー発生関数
				MIL.MappGetHookInfo( nEventId, MIL.M_MESSAGE + MIL.M_CURRENT_FCT, sb_function );
				// エラー内容
				MIL.MappGetHookInfo( nEventId, MIL.M_MESSAGE + MIL.M_CURRENT, sb_current );
				// エラー内容詳細の文字列数
				MIL.MappGetHookInfo( nEventId, MIL.M_CURRENT_SUB_NB, ref i_subcode );

				// エラー内容の詳細文字列を取得する
				if( i_subcode > 2 )
				{
					MIL.MappGetHookInfo( nEventId, MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE, sb_errmsg3 );
				}
				if( i_subcode > 1 )
				{
					MIL.MappGetHookInfo( nEventId, MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE, sb_errmsg2 );
				}
				if( i_subcode > 0 )
				{
					MIL.MappGetHookInfo( nEventId, MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE, sb_errmsg1 );
				}
				*/
			}

			return	MIL.M_NULL;
		}
		#endregion


		#region ローカル関数
		/// <summary>
		/// MILエラーの有無を判断し、有る場合はログに残す
		/// </summary>
		/// <returns>true:エラー有り</returns>
		/// <remarks>MILHelp.chmのMappGetError()のMseqProcess.csのコピー</returns>
		protected bool is_mil_error()
		{
			bool				b_ret				= false;
			StringBuilder		MilErrorMsg			= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );

			MIL_INT		MilErrorCode	= MIL.MappGetError( m_MilApplication, MIL.M_CURRENT + MIL.M_MESSAGE, MilErrorMsg );
			if( MIL.M_NULL_ERROR != MilErrorCode )
			{
				b_ret								= true;
				MIL_INT[]		  MilErrorSubCode	= new MIL_INT[ 3 ];
				StringBuilder[]	  MilErrorSubMsg	= new StringBuilder[ 3 ];

				// Initialize MilErrorSubMsg array.
				for( int i_loop = 0; i_loop < 3; i_loop++ )
				{
					MilErrorSubMsg[ i_loop ]		= new StringBuilder( MIL.M_ERROR_MESSAGE_SIZE );
				}
				/* Collects Mil error messages and sub-messages */
				MIL_INT				subCount		= 3;
				MIL.MappGetError( m_MilApplication, MIL.M_CURRENT_SUB_NB, ref subCount );
				MilErrorSubCode[ 0 ]				= MIL.MappGetError( m_MilApplication,
														MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE,
														MilErrorSubMsg[ 0 ] );
				MilErrorSubCode[ 1 ]				= MIL.MappGetError( m_MilApplication,
														MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE,
														MilErrorSubMsg[ 1 ] );
				MilErrorSubCode[ 2 ]				= MIL.MappGetError( m_MilApplication,
														MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE,
														MilErrorSubMsg[ 2 ]);

				setLogDevice( MilErrorMsg.ToString() );
				setLogError( "[MIL.Error message]" + MilErrorMsg.ToString(), true );
				setLabel( MilErrorMsg.ToString() );
				for( int i_loop = 0; i_loop < subCount; i_loop++ )
				{
					if( 0 != MilErrorSubCode[ i_loop ] )
					{ 
						setLogDevice( MilErrorSubMsg[ i_loop ].ToString() );
						setLogError( "[MIL.Error message]" + MilErrorSubMsg[ i_loop ].ToString(), true );
						setLabel( MilErrorSubMsg[ i_loop ].ToString() );
					}
				}
			}

			return		b_ret;
		}


		/// <summary>
		/// 画面表示用バッファの設定
		/// </summary>
		/// <param name="nhDisplayHandle">Window Handel</param>
		/// <remarks>FormかPanelのハンドルのみ有効、PictureBoxのハンドルでは表示しない</returns>
		protected void set_show_display( MIL_ID nMilId )
		{
			// 表示しない場合
			if( false == Display )
			{
				return;
			}

			// MIL画面へ表示
			if( IntPtr.Zero == m_hDisplayHandle )
			{
				MIL.MdispSelect( m_MilDisplay, nMilId );
				return;
			}

			// ユーザアプリへ表示
			{
				// 画像サイズ取得
				MIL_INT		i_object_width		= MIL.MbufInquire( nMilId, MIL.M_SIZE_X, MIL.M_NULL );
				MIL_INT		i_object_height		= MIL.MbufInquire( nMilId, MIL.M_SIZE_Y, MIL.M_NULL );
				if( MIL.M_NULL != m_MilShowImage )
				{
					// 現在の画像サイズ
					MIL_INT		i_width			= MIL.MbufInquire( m_MilShowImage, MIL.M_SIZE_X, MIL.M_NULL );;
					MIL_INT		i_height		= MIL.MbufInquire( m_MilShowImage, MIL.M_SIZE_Y, MIL.M_NULL );;
					if( i_object_width != i_width || i_object_height != i_height )
					{
						MIL.MbufClear( m_MilShowImage, 0 );
						MIL.MbufFree( m_MilShowImage );
						m_MilShowImage			= MIL.M_NULL;
					}
				}

				// 表示用画像バッファ生成
				if( MIL.M_NULL == m_MilShowImage )
				{
					MIL.MbufAllocColor( m_MilSystem, 3, i_object_width, i_object_height,
										8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24,
										ref m_MilShowImage );
					MIL.MbufClear( m_MilShowImage, 0 );
				}
				// 表示実施
				MIL.MbufCopy( nMilId, m_MilShowImage );
				MIL.MdispSelectWindow( m_MilDisplay, m_MilShowImage, m_hDisplayHandle );
			}
		}
		#endregion


		#region アクセッサ関数
		/// <summary>
		/// 画像幅サイズ取得
		/// </summary>
		/// <returns>画像幅[pixcel]</returns>
		public int get_image_width()
		{
			return	m_cParaMilControl.ImageSizeWidth;
		}


		/// <summary>
		/// 画像高さサイズ取得
		/// </summary>
		/// <returns>画像幅[pixcel]</returns>
		public int get_image_height()
		{
			return	m_cParaMilControl.ImageSizeHeight;
		}
		#endregion


		#region メンバ関数
		/// <summary>
		/// ユーザー画面表示用ウインドウハンドルの設定
		/// </summary>
		/// <param name="nhDisplayHandle">Window Handel</param>
		/// <remarks>FormかPanelのハンドルのみ有効、PictureBoxのハンドルでは表示しない</returns>
		public void set_display_handle( IntPtr nhDisplayHandle )
		{
			if( null != nhDisplayHandle )
			{
				MIL.MdispControl( m_MilDisplay, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE );	// センターに表示
				MIL.MdispControl( m_MilDisplay, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE );	// ストレッチして表示
			}
			m_hDisplayHandle	= nhDisplayHandle;
		}
		#endregion
	}
}
