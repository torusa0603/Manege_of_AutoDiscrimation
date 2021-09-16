using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CameraControl
{
	/// <summary>
	/// Matrox制御クラス
	/// </summary>
	public class CMatrox : CCameraControlBase
	{
		#region クラス内定義
		new private const string			m_strDEVICE_NAME		= "Matrox";		// デバイス名
		#endregion


		#region ローカル変数
		#endregion


		#region ローカル変数 サンプル動作用
		private CancellationTokenSource		m_CancellationImage		= null;			// 撮影スレッド中止用
		private CancellationTokenSource		m_CancellationProcess	= null;			// 処理スレッド中止用

		#endregion


		#region プロパティ
		#endregion


		#region コンストラクタ / デストラクタ
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CMatrox()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CMatrox()
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
				// Open済みでないばあいもtrueで返しちゃう
				if( true == is_open() )
				{
					str_log			= "Close.";
					setLogExecute( str_log );
					setLogDevice( str_log );
					cImageMatrox.ImageMatroxDllMethod.sifCloseImageProcess();
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
		public override bool open( IntPtr nhDispHandle )
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
					int i_ret		= 0;
					while( true )
					{
                        
                        i_ret	= cImageMatrox.ImageMatroxDllMethod.sifInitializeImageProcess( nhDispHandle ,"");
						if( 0 != i_ret )
						{
							break;
						}
						i_ret	= cImageMatrox.ImageMatroxDllMethod.sifThrough();
						if( 0 != i_ret )
						{
							break;
						}
						m_bOpend = true;
						break;
					}
					if( 0 != i_ret )
					{
						str_log		= "Failed open( " + i_ret.ToString() + " ).";
						setLogExecute( str_log );
						setLogDevice( str_log );
						setLogError( str_log );
						b_ret		= false;
					}
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
			tag_rect		rect;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					str_log			= "Save. Filename = " + nstrFilename;
					setLogDevice( str_log );
					rect.left = rect.top = rect.right = rect.bottom = 0;
					//cImageMatrox.ImageMatroxDllMethod.sifSaveImage( rect, true, nstrFilename, true );
					cImageMatrox.ImageMatroxDllMethod.sifSaveImage(rect, true, nstrFilename, false);
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
				str_log				= "Failed save! " + ex.Message;
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
					if( 0 != cImageMatrox.ImageMatroxDllMethod.sifSetTriggerModeOff() )
					{
						str_log			= "Failed trigger off.";
						setLogDevice( str_log );
						b_ret		= false;
					}
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
					str_log			= "Set trigger mode software.";
					setLogDevice( str_log );
					cImageMatrox.ImageMatroxDllMethod.sifSetTriggerModeSoftware();
					if( 0 != cImageMatrox.ImageMatroxDllMethod.sifSetTriggerModeSoftware() )
					{
						str_log			= "Failed set trigger mode software.";
						setLogDevice( str_log );
						b_ret		= false;
					}
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed set trigger mode software! " + ex.Message;
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
					str_log			= "Set trigger mode hardware( "+ nstrTrigger + " ).";
					setLogDevice( str_log );
					if( 0 != cImageMatrox.ImageMatroxDllMethod.sifSetTriggerModeHardware( nstrTrigger ) )
					{
						str_log			= "Failed set trigger mode hardware.";
						setLogDevice( str_log );
						b_ret		= false;
					}
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
					if( 0 != cImageMatrox.ImageMatroxDllMethod.sifExecuteSoftwareTrigger() )
					{
						str_log			= "Failed execute software trigger.";
						setLogDevice( str_log );
						b_ret		= false;
					}
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

			nSize					= new Size( 0, 0 );
			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					nSize			= cImageMatrox.ImageMatroxDllMethod.sifGetImageSize();
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
                    i_ret = cImageMatrox.ImageMatroxDllMethod.sifGetMonoBitmapData(nlArySize, npbyteData);
     //               unsafe
					//{ 
					//	fixed( byte px = npbyteData )
					//	{
					//		i_ret	= cImageMatrox.ImageMatroxDllMethod.sifGetMonoBitmapData( nlArySize, npbyteData);
					//	}
					//}
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


		#region サンプル動作 1
		/// <summary>
		/// サンプル動作1 スレッドを使い画像データの連続処理開始
		/// </summary>
		/// <returns>true:成功</returns>
		public bool start_sample1()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					// ログ
					str_log			= "Start stample1.";
					setLogDevice( str_log );
					// ファイル削除
					exec_file_remove();
					// 開始
					start_sample1_image();
					start_sample1_process();
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed sample1! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret				= false;
			}

			return	b_ret;
		}


		/// <summary>
		/// サンプル動作 1 連続処理終了(キャンセル)
		/// </summary>
		/// <remarks>
		/// </remarks>
		public void cancel_sample1()
		{
			m_CancellationImage?.Cancel();			// Cancel実行
			m_CancellationProcess?.Cancel();
		}


		/// <summary>
		/// サンプル動作 1 ソフトトリガ連続画像取得処理開始
		/// </summary>
		private async void start_sample1_image()
		{
			// 終了(キャンセル)処理用
			m_CancellationImage	= new CancellationTokenSource();
			var token = m_CancellationImage.Token;

			// 測定処理
			await execute_sample1_image( token );

			// 終了(キャンセル)処理終了
			m_CancellationImage		= null;
		}

		/// <summary>
		/// サンプル動作 1 ソフトトリガ連続画像取得処理開始task
		/// </summary>
		/// <param name="cancelToken">処理キャンセル用Token</param>
		private async Task< bool > execute_sample1_image( CancellationToken cancelToken )
		{
			return	await Task.Run( () =>
			{
				bool		b_ret				= true;

				// ソフトトリガ設定
				set_trigger_mode_software();

				// 時間測定（確認用）
				var sw_camera = new System.Diagnostics.Stopwatch();
				var sw_trigger = new System.Diagnostics.Stopwatch();
				sw_camera.Restart();
				// キャンセルされるまでループ
				while( false == cancelToken.IsCancellationRequested )
				{
					// 撮影処理
					sw_camera.Restart();
					sw_trigger.Restart();
					execute_software_trigger();
					sw_trigger.Stop();

					// fps=30設定
					while( 33 > sw_camera.ElapsedMilliseconds )
					{
						Thread.Sleep( 0 );
					}

					// デバッグ表示
					string	str_log		= "time trigger = " + $"{ sw_trigger.ElapsedMilliseconds }msec" + " : time_camara = " + $"{ sw_camera.ElapsedMilliseconds }msec";
					System.Diagnostics.Debug.WriteLine( str_log );

					// ストップウォッチリスタート
					sw_camera.Restart();
				}
				// トリガオフ設定
				set_trigger_mode_off();
				return		b_ret;
			} );
		}


		/// <summary>
		/// サンプル動作 1 連続処理開始
		/// </summary>
		private async void start_sample1_process()
		{
			// 終了(キャンセル)処理用
			m_CancellationProcess	= new CancellationTokenSource();
			var token = m_CancellationProcess.Token;

			// 測定処理
			await execute_sample1_process( token );

			// 終了(キャンセル)処理終了
			m_CancellationProcess	= null;
		}


		/// <summary>
		/// サンプル動作 1 連続処理実行task
		/// </summary>
		/// <param name="cancelToken">処理キャンセル用Token</param>
		private async Task< bool > execute_sample1_process( CancellationToken cancelToken )
		{
			return	await Task.Run( () =>
			{
				bool		b_ret				= true;
				Size		size;

				// 配列準備
				get_image_size( out size );
				int			i_total_pixel_num	= size.Width * size.Height;
				byte[]		bt_pixel_value_buff	= new byte[ i_total_pixel_num ];
				// ビットマップ生成
				Bitmap		bmp		= new Bitmap( size.Width, size.Height, PixelFormat.Format8bppIndexed );
				// カラーパレットを設定
				ColorPalette		pal			= bmp.Palette;
				for( int i_loop = 0; i_loop < 256; i_loop++ )
				{
					pal.Entries[ i_loop ]		= Color.FromArgb( i_loop, i_loop, i_loop );
				}
				bmp.Palette						= pal;

				// 時間測定（確認用）
				var sw_camera = new System.Diagnostics.Stopwatch();
				var sw_total = new System.Diagnostics.Stopwatch();
				sw_total.Restart();
				// キャンセルされるまでループ
				while( false == cancelToken.IsCancellationRequested )
				{
					sw_camera.Restart();
					// ビットマップバイトデータの取得
					if( true == get_bitmap_data( i_total_pixel_num, bt_pixel_value_buff ) )
					{
						// BitmapDataに用意したbyte配列を一気に書き込む
						BitmapData bmpdata			= bmp.LockBits( new Rectangle( 0, 0, size.Width, size.Height ),
																	ImageLockMode.WriteOnly,
																	PixelFormat.Format8bppIndexed
																	);
						Marshal.Copy( bt_pixel_value_buff, 0, bmpdata.Scan0, bt_pixel_value_buff.Length );
						bmp.UnlockBits( bmpdata );
						sw_camera.Stop();
						// ビットマップファイル保存
						string str_file_name	= get_file_name();
						bmp.Save( str_file_name, System.Drawing.Imaging.ImageFormat.Bmp );

						sw_total.Stop();
						// デバッグ表示
						string	str_log		= "time camara = " + $"{ sw_camera.ElapsedMilliseconds }msec" + " : time_total = " + $"{ sw_total.ElapsedMilliseconds }msec";
						System.Diagnostics.Debug.WriteLine( str_log );

						// ストップウォッチリスタート
						sw_total.Restart();
					}
					else
					{
						Thread.Sleep( 0 );
					}
				}
				return		b_ret;
			} );
		}
		#endregion


		#region サンプル動作 2
		/// <summary>
		/// サンプル動作2 ImageMatrox.dll(c++)で画像データの連続処理開始
		/// </summary>
		/// <returns>true:成功</returns>
		public bool start_sample2()
		{
			bool			b_ret	= true;
			string 			str_log;

			try
			{
				// Open済み確認
				if( true == is_open() )
				{
					// ログ
					str_log			= "Start stample2.";
					setLogDevice( str_log );
					// ファイル削除
					exec_file_remove();
					// 開始
					if( 0 != cImageMatrox.ImageMatroxDllMethod.sifSampleInspection( m_strFolderName ) )
					{
						b_ret	= false;
					}
				}
			}
			catch( System.Exception ex )
			{
				str_log				= "Failed sample2! " + ex.Message;
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
