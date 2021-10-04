using System;
using System.Windows.Forms;
using System.Drawing;

namespace CameraControl
{
	/// <summary>
	/// Matrox制御クラス(仮想)
	/// </summary>
	public class CCameraControlVirtual : CCameraControlBase
	{
		#region クラス内定義
		new private const string		m_strDEVICE_NAME	= "VirtualCameraController";			// デバイス名
		#endregion

		#region ローカル変数
		#endregion


		#region プロパティ
		#endregion


		#region コンストラクタ / デストラクタ
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CCameraControlVirtual()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CCameraControlVirtual()
		{
			// デバイス名
			m_strDeviceName		= m_strDEVICE_NAME;
		}
		#endregion


		#region 抽象メソッドの実体
		/// <summary>
		/// クローズ処理
		/// </summary>
		/// <returns>true:成功</returns>
		public override bool close()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済みでない場合もtrueで返しちゃう
				if( true == is_open() )
				{
					str_log			= "Close.";
					setLogExecute( str_log );
					setLogDevice( str_log );
					m_bOpend		= false;
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
		/// オープン処理
		/// </summary>
		/// <param name="nhDispHandle">Window handle(Ex.Pandl.Handle)</param>
		/// <returns>true:成功</returns>
		/// <remarks>
		///  nhDispHandleで渡すWindow handleはPictureBoxは使えない。
		///  FormやPandlであれば使える。またストレッチ表示等は出来ない。
		///  ( IntPtr )nullを渡すとMILの画面が勝手に立ち上がる。
		/// </remarks>
		public override bool open( IntPtr nhDispHandle , bool nbThroughSimple)
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済みだった場合はtrueで返しちゃう
				if( false == is_open() )
				{
					str_log			= "Open.";
					setLogExecute( str_log );
					setLogDevice( str_log );
					m_bOpend		= true;
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


		/// <summary>
		/// 画像保存
		/// </summary>
		/// <param name="nstrFilename">ファイル名</param>
		/// <returns>true:成功</returns>
		public override bool save_image( string nstrFilename )
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Save. Filename = " + nstrFilename ;
					setLogDevice( str_log );
				}
				else
				{
					str_log			= "Failed not open.";
					setLogDevice( str_log );
					setLogError( str_log );
					b_ret			= false;
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




		/// <summary>
		/// TriggerModeOff設定
		/// </summary>
		/// <returns>true:成功</returns>
		public override bool set_trigger_mode_off()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Trigger off.";
					setLogDevice( str_log );
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed trigger off! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// TriggerModeSoftware設定
		/// </summary>
		/// <returns>true:成功</returns>
		public override bool set_trigger_mode_software()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Trigger mode software.";
					setLogDevice( str_log );
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed trigger mode software! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// TriggerModeHardware設定
		/// </summary>
		/// <param name="nstrTrigger">トリガ名(Ex.acA1300-30gmの場合Line1)</param>
		/// <returns>true:成功</returns>
		public override bool set_trigger_mode_hardware( string nstrTrigger )
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Trigger mode hardware( "+ nstrTrigger + " ).";
					setLogDevice( str_log );
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed trigger mode hardware! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// SoftwareTriggerの実行
		/// </summary>
		/// <returns>true:成功</returns>
		public override bool execute_software_trigger()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Execute software trigger.";
					setLogDevice( str_log );
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed execute software trigger! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// 画像サイズの取得
		/// </summary>
		/// <param name="nSize">画像サイズ(戻り値用)</param>
		/// <returns>true:成功</returns>
		public override bool get_image_size( out Size nSize )
		{
			bool			b_ret	= true;
			string 			str_log;

			nSize					= new Size( 1000, 800 );
			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Get image size( " + nSize.Width.ToString() + ", " + nSize.Height.ToString() + " ).";
					setLogDevice( str_log );
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed get image size! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// 画像データの取得
		/// </summary>
		/// <returns>true:成功</returns>
		unsafe public override bool get_bitmap_data( long nlArySize, byte []npbyteData )
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Get bitmap data.";
					setLogDevice( str_log );
					int		i_ret	= 0;
					for( int i_loop = 0; i_loop < nlArySize; i_loop ++ )
					{
						npbyteData[ i_loop ]	= 0x00;
					}
					if( 0 != i_ret )
					{
						str_log			= "Failed get bitmap data.";
						setLogDevice( str_log );
						b_ret		= false;
					}
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed get bitmap data! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}
		#endregion
	}
}
