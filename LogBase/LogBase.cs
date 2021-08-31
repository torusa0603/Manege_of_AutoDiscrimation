using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace LogBase
{
	/// <summary>
	/// ログライブラリクラス
	/// </summary>
	public class CLogBase
	{
		#region クラス内定数パラメータ
		// 拡張子
		private const string s_strEXTENSION = ".log";
		#endregion

		#region 本クラス実体保持用Dictionary
		static Dictionary< string, CLogBase > m_dicLog = new Dictionary< string, CLogBase >();
		#endregion

		#region パラメータ変数
		// ログを残す設定
		public bool Enable { get; set; }
		// 即時書き込み
		public bool ImmediateWrite { get; set; }
		// ログ文字列に日時を含むか(true=含まない)
		public bool NoDateTime{ get; set; }
		// 書き込みを実施する文字サイズ[byte]（即時書き込みでない場合）
		public uint StringSize{ get; set; }
		// 最大ファイルサイズ[byte]
		public uint FileSize{ get; set; }
		// 最大ファイル数
		public uint FileCount{ get; set; }
		// アプリケーションフォルダ名
		public string ExecFolderName{ get; set; }
		// フォルダ名
		public string FolderName{ get; set; }
		// ファイル名に使用する文字列
		public string FileName{ get; set; }
		// 拡張子文字列（.を含めて設定する事）
		public string Extension{ get; set; }
		#endregion

		#region ローカル変数
		private string m_strFolderName = "";				// フォルダ名（ ExecPath + FolderName + "\\" ）
		private string m_strFileName = "";					// ファイル名
		private string m_strLog = "";						// ログ文字列
		private string m_strDate = "";						// ログ文字列 日付
		private string m_strHour = "";						// ログ文字列 時分
		private uint m_uiWriteSize = 0;						// 書き込みサイズ(ファイル削除判定用)
		// 文字列エンコーダ
		private Encoding m_Encoding = Encoding.GetEncoding( "Shift_JIS" );
		// 書き込みミューテックス
		private Mutex m_mtxWrite = new Mutex();
		#endregion


		#region コンストラクタ・デストラクタ・実体管理
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CLogBase()
		{
			if( false == ImmediateWrite && 0 < m_strLog.Length )
			{
				// 最後に書き込みして終了
				writeLog( m_strLog );
			}
		}

		/// <summary>
		/// コンストラクタ(単体で使用する場合)
		/// </summary>
		public CLogBase()
		{
			// 本パラメータクラスの初期化
			initialize_for_constructor();
		}

		/// <summary>
		/// コンストラクタ( Dictionaryを使用する場合)
		/// </summary>
		public CLogBase( string nstrKey )
		{
			// 本パラメータクラスの初期化
			initialize_for_constructor();

			if( false == m_dicLog.ContainsKey( nstrKey ) )
			{
				m_dicLog.Add( nstrKey, this );
			}
		}


		/// <summary>
		/// コンストラクタ用初期化
		/// </summary>
		private void initialize_for_constructor()
		{
			// 本パラメータクラスの初期化
			Enable = true;					// ログを残す設定
			ImmediateWrite = true;			// 即時書き込み
			NoDateTime = false;				// ログ文字列に日時を含むか(true=含まない)
			StringSize = 1024;				// 書き込みを実施する文字サイズ1Kbyte
			//FileSize = 65536;               // ファイル最大文字サイズ64Kbyte
			FileSize = 1000000;               // ファイル最大文字サイズ1Mbyte
			FileCount = 50;					// 最大ファイル数 50個
			ExecFolderName = "";			// アプリケーションフォルダ名
			FolderName = "log";				// フォルダ名
			FileName = "log";				// ファイル名
			Extension = s_strEXTENSION;		// 拡張子文字列（.を含めて設定する事）
		}


		/// <summary>
		/// Dictionaryからクラス実体の取得
		/// </summary>
		public static CLogBase getInstance( string nstrKey )
		{
			if( true == m_dicLog.ContainsKey( nstrKey ) )
			{
				return m_dicLog[ nstrKey ];
			}

			return null;
		}
		#endregion


		#region メンバ関数
		/// <summary>
		/// ログ出力
		/// </summary>
		/// <param name="strLog">ログ文字列</param>
		public void outputLog( string nstrLog )
		{
			// ログを残す設定になっているか
			if( false == Enable )
			{
				return;
			}
			// 文字書き込み数カウント
			m_uiWriteSize += ( uint )m_Encoding.GetByteCount( nstrLog );
			// 即時書き込みの場合
			if( true == ImmediateWrite )
			{
				writeLog( getLogString( nstrLog ) );
				return;
			}
			// 複数行一括書き込みの場合
			else
			{
				if( 0 == m_strLog.Length )
				{
					m_strLog = getLogString( nstrLog );
					// ファイル名が設定されていない場合
					if( 0 == m_strFileName.Length )
					{
						setFileName();
					}
					// 最初のログ文字列であればリターンでよかろう
					return;
				}

				// 複数行ログ文字列生成
				m_strLog += "\r\n";
				m_strLog += getLogString( nstrLog );
				// 文字サイズ確認
				uint ui_size = ( uint )m_Encoding.GetByteCount( m_strLog );
				if( ui_size > StringSize )
				{
					// 書き込み
					writeLog( m_strLog );
					// クリア
					m_strLog = "";
				}
			}
		}


		/// <summary>
		/// csv出力
		/// </summary>
		/// <param name="nlstData">csv出力するデータ</param>
		public void outputCsv( List< double[] > nlstData )
		{
			// ファイル名の設定
			setFileName();

			// 書き込み実行
			using ( var writer = new StreamWriter( m_strFileName, true, m_Encoding ) )
			{
				for( int i_loop = 0; i_loop < nlstData[ 0 ].Length; i_loop++ )
				{
					string	str_log		= "";

					for( int i_list = 0; i_list < nlstData.Count(); i_list++ )
					{
						if( 0 != str_log.Count() )
						{
							str_log		+= ",";
						}
						str_log			+= nlstData[ i_list ][ i_loop ].ToString();
					}
					writer.WriteLine( str_log );
					m_uiWriteSize += ( uint )str_log.Count();
				}
			}

			// 次回は新しいファイル名
			m_strFileName = "";
			// 削除実行
			execRemove();
		}


		/// <summary>
		/// ファイルリスト取得
		/// </summary>
		/// <returns>ファイルリスト</returns>
		public List< string > getFileList()
		{
			// 設定されていない場合は、setFileName()を実行する事で設定されるはず
			if( "" == m_strFolderName )
			{
				setFileName();
			}
			return	new List< string >( System.IO.Directory.GetFiles( m_strFolderName, "*" + Extension ) );
		}
		#endregion


		#region ローカル関数
		/// <summary>
		/// 1行分のログ文字列生成
		/// </summary>
		/// <param name="strLog">ログ文字列</param>
		private string getLogString( string nstrLog )
		{
			// ログ文字列に日時を含むか(true=含まない)
			if( true == NoDateTime )
			{
				return	nstrLog;
			}

			string str_now_date = System.DateTime.Now.ToString( "yyyyMMdd" );
			string str_now_hour = System.DateTime.Now.ToString( "HHmm" );
			string str_now_seco = System.DateTime.Now.ToString( "ss.fff" );

			//if( m_strDate == str_now_date )
			//{
			//	// 同じ場合tabに圧縮
			//	str_now_date = "\t\t";
			//}
			//else
			//{
			//	m_strDate = str_now_date;
			//}
			m_strDate = str_now_date;

			//if ( m_strHour == str_now_hour )
			//{
			//	// 同じ場合tabに圧縮
			//	str_now_hour = "\t";
			//}
			//else
			//{
			//	m_strHour = str_now_hour;
			//}
			m_strHour = str_now_hour;
			
			return str_now_hour + str_now_seco + " " + nstrLog;
		}


		/// <summary>
		/// ログファイル書き込み
		/// </summary>
		/// <param name="nstrLog">ログ文字列</param>
		private void writeLog( string nstrLog )
		{
			m_mtxWrite.WaitOne();
			try
			{
				// ファイル名が設定されていない場合
				if( 0 == m_strFileName.Length )
				{
					setFileName();
				}

				// 書き込み実行
				using ( var writer = new StreamWriter( m_strFileName, true, m_Encoding ) )
				{
					writer.WriteLine( nstrLog );
				}

				// ファイルサイズ確認
				System.IO.FileInfo fileinfo = new System.IO.FileInfo( m_strFileName );
				uint uiFileSize = ( uint )fileinfo.Length;
				if( uiFileSize > FileSize )
				{
					// 次回は新しいファイル名
					m_strFileName = "";
					m_strDate = "";
					m_strHour = "";
				}

				// 削除実行
				execRemove();
			}
			finally
			{
				m_mtxWrite.ReleaseMutex();
			}
		}


		/// <summary>
		/// ファイル名の設定
		/// </summary>
		private void setFileName()
		{
			if( 0 != ExecFolderName.Length )
			{
				m_strFolderName = ExecFolderName + "\\" + FolderName + "\\";
			}
			else
			{
				m_strFolderName = FolderName + "\\";
			}
			if( false == System.IO.File.Exists( m_strFolderName ) )
			{
				System.IO.Directory.CreateDirectory( m_strFolderName );
			}

			string	str_temp_fname	= m_strFolderName + FileName + Extension;

			// ファイル有無確認
			if( true == System.IO.File.Exists( str_temp_fname ) )
			{
				string	str_backup	= m_strFolderName + System.DateTime.Now.ToString( "yyyyMMdd_HHmmss.fff" ) + "_" + FileName + Extension;

				// ファイル名のファイルが有る場合（念の為）
				if( true == System.IO.File.Exists( str_backup ) )
				{
					str_backup		= m_strFolderName + System.DateTime.Now.ToString( "yyyyMMdd_HHmmss.fff" ) + "_" + FileName + Extension;
				}
				// ファイル名変更
				System.IO.File.Move( str_temp_fname, str_backup );
			}
			m_strFileName 			= str_temp_fname;
		}


		/// <summary>
		/// ログファイル削除
		/// </summary>
		private void execRemove()
		{
			// 書き込みを実行したサイズがStringSize以上であるか
			if( m_uiWriteSize < StringSize )
			{
				return;
			}
			m_uiWriteSize = 0;

			// リストアップ開始
			List< string >	lstFileName = getFileList();	// ログファイル名リスト

			// ファイル数確認して一気に削除
			while( null != lstFileName && lstFileName.Count() > FileCount )
			{
				string str_del_file_name = lstFileName[ 0 ];
				System.IO.File.Delete( str_del_file_name );
				lstFileName.RemoveAt( 0 );
			}
		}
		#endregion
	}
}
