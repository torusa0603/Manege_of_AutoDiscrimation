using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CameraControl
{
	/// <summary>
	/// カメラ制御の抽象クラス
	/// </summary>
	abstract public class CCameraControlBase : LogBase.CDeviceControlLog
	{
		#region 継承先で必須の抽象メソッド
		abstract public bool close();
		abstract public bool open( IntPtr nhDispHandle );
		abstract public bool save_image( string nstrFilename );

		abstract public bool set_trigger_mode_off();
		abstract public bool set_trigger_mode_software();
		abstract public bool set_trigger_mode_hardware( string nstrTrigger );
		abstract public bool execute_software_trigger();
		abstract public bool get_image_size( out Size nSize );
		abstract unsafe public bool get_bitmap_data( long nlArySize, byte []npbyteData );
		#endregion


		#region クラス内定義
		new private const string	m_strDEVICE_NAME	= "Camera";		// デバイス名
		#endregion


		#region ローカル変数
		protected bool				m_bOpend			= false;		// オープン済み？
		protected string			m_strFolderName		= "";			// フォルダ名
        protected CImageMatrox cImageMatrox = new CImageMatrox();
        #endregion


        #region プロパティ
        #endregion


        #region コンストラクタ / デストラクタ
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CCameraControlBase()
		{
			m_strDeviceName			= m_strDEVICE_NAME;
		}
		#endregion


		#region メンバ関数
		/// <summary>
		/// オープン確認用
		/// </summary>
		public bool is_open()
		{
			return		m_bOpend;
		}
		#endregion


		#region 画像保存用フォルダ&ファイル名関連
		/// <summary>
		/// フォルダ名設定
		/// </summary>
		/// <remarks>
		///  画像保存先フォルダは、ユーザークラスが登録しライブラリ側で管理、運用する。
		/// </remarks>
		public void set_folder_name( string nstrName )
		{
			m_strFolderName		= nstrName;
			if( false == System.IO.File.Exists( m_strFolderName ) )
			{
				// フォルダ生成
				System.IO.Directory.CreateDirectory( m_strFolderName );
			}
		}


		/// <summary>
		/// フォルダ名取得
		/// </summary>
		/// <returns>フォルダ名</returns>
		public string get_folder_name()
		{
			return	m_strFolderName;
		}


		/// <summary>
		/// カメラ画像ファイル名取得
		/// </summary>
		/// <returns>カメラ画像ファイル名</returns>
		/// <remarks>
		///  画像ファイル名はフォルダ名と合成して本クラスで作成する。
		/// </remarks>
		public string get_file_name()
		{
			string		str_ret;
			str_ret		= m_strFolderName + "\\" + System.DateTime.Now.ToString( "yyyyMMdd_HHmmss.fff" ) + ".bmp";
			return		str_ret;
		}


		/// <summary>
		/// 画像ファイル削除
		/// </summary>
		/// <param name="nstrNowFileName">現在表示中のファイル名</param>
		/// <param name="niFileCount">フォルダ内に残すファイル数</param>
		/// <returns>画像ファイルでは例外が発生することが多々あるようだ</returns>
		/// <remarks>
		///  ユーザークラスは適宜関数を呼び出し、フォルダ内のファイル数を減らす。
		/// </remarks>
		public bool exec_file_remove( string nstrNowFileName = null, int niFileCount = 20 )
		{
			bool	b_ret	= true;
			try
			{
				// リストアップ開始
				string		str_folder_nane		= get_folder_name() + "\\";
				List< string >	lstFileName		= new List< string >( System.IO.Directory.GetFiles( str_folder_nane, "*.bmp" ) );	// ファイル名リスト
				// 現在表示中のファイルは無視する
				if( null != nstrNowFileName )
				{
					lstFileName.Remove( nstrNowFileName );
				}
				// ソートすれば古い順に並ぶはず(念の為)
				lstFileName.Sort();
				// ファイル数確認して一気に削除
				int intPicNo = 0;
				while( null != lstFileName && lstFileName.Count() > niFileCount )
				{
                    try
                    {
						if (intPicNo >= niFileCount)
							return b_ret;

						string str_del_file_name = lstFileName[intPicNo];
						System.IO.File.Delete(str_del_file_name);
						lstFileName.RemoveAt(0);
					}
					catch (System.Exception ex1)
                    {
						System.Diagnostics.Debug.WriteLine(ex1.Message);
						intPicNo++;
					}
				}
			}
			catch( System.Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( ex.Message );
				b_ret	= false;
			}
			return	b_ret;
		}
		#endregion
	}
}
