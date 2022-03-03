using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace LogBase
{
	/// <summary>
	/// タイプライターの様な表示をするLabel
	/// </summary>
	/// <remarks>
	/// 画面上にログを表示する際に使用する。
	/// 使用方法としては画面上にLabelを張って、本クラスへポインタを渡す。
	/// 各ユニット(Device)制御の際、ここぞという文字列を表示する際、本クラスのset()を呼び出す
	/// </remarks>
	public class CTypeWriterLabel
	{
		#region クラス内定数パラメータ
		private const int				m_iLINECOUNT			= 1;							// 一気に表示するライン数
		#endregion

		#region アプリに必ずひとつの実体
		private static CTypeWriterLabel	m_Item					= null;

		/// <summary>
		/// サブクラス実態の取得
		/// </summary>
		public static CTypeWriterLabel getInstance()
		{
			return		m_Item;
		}
		#endregion


		#region 本クラス実体保持用Dictionary
		static Dictionary< string, CTypeWriterLabel > m_dicLog	= new Dictionary< string, CTypeWriterLabel >();
		#endregion


		#region ローカル変数
		private		Label					m_Label				= null;

		private		List< string >			m_lstRegist			= new List< string >();		// ユーザーに登録された文字列リスト
		private		List< string >			m_lstDisplay		= new List< string >();		// 表示中の文字列リスト

		private		Mutex					m_mtxRegist			= new Mutex();				// m_lstRegist操作用

		private CancellationTokenSource		m_Cancellation		= null;						// Task動作確認用
		private Thread						m_Thread			= null;						// スレッド
		#endregion


		#region プロパティ
		public bool		UseTask { private get; set; }			= false;					// タスクを使うか？
		public bool		Enabled { private get; set; }			= true;						// 機能有効か？
		public int		Interval { private get; set; }			= 10;						// 表示インターバル
		public double	Fluctuation { private get; set; }		= 90;						// 表示インターバル揺らぎ割合[%]
		public double	DisplayHeight { private get; set; }		= 98;						// 表示高さ（親に対する）割合[%]
		#endregion


		#region プログラム開始時初期化
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CTypeWriterLabel()
		{
			cancel();
		}


		/// <summary>
		/// コンストラクタ(一つのアプリで一つのLabelのみの場合)
		/// </summary>
		/// <param name="nLabel">Label</param>
		/// <remarks>LabelにはParent設定をしておく事。Parent設定が無いと透けない</remarks>
		public CTypeWriterLabel( Label nLabel )
		{
			m_Item				= this;

			// 本クラスの初期化
			initialize_for_constructor( nLabel );
		}


		/// <summary>
		/// コンストラクタ( Dictionaryを使用する場合)
		/// </summary>
		/// <param name="nLabel">Label</param>
		/// <remarks>LabelにはParent設定をしておく事。Parent設定が無いと透けない</remarks>
		public CTypeWriterLabel( string nstrKey, Label nLabel )
		{
			if( false == m_dicLog.ContainsKey( nstrKey ) )
			{
				m_dicLog.Add( nstrKey, this );
			}

			// 本クラスの初期化
			initialize_for_constructor( nLabel );
		}


		/// <summary>
		/// コンストラクタ用初期化
		/// </summary>
		/// <param name="nLabel">Label</param>
		private void initialize_for_constructor( Label nLabel )
		{
			// 本クラスの初期化
			m_Label					= nLabel;
			if( null != nLabel )
			{
				m_Label.BackColor	= System.Drawing.Color.Transparent;
				m_Label.Location	= new System.Drawing.Point( 0, 4 );
			}
		}


		/// <summary>
		/// Dictionaryからクラス実体の取得
		/// </summary>
		public static CTypeWriterLabel getInstance( string nstrKey )
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
		/// 表示文字列登録
		/// </summary>
		/// <param name="nStrText">表示文字列</param>
		public void set( string nStrText )
		{
			// 表示実行するか
			if( null == m_Label || false == Enabled )
			{
				return;
			}

			// 登録
			m_mtxRegist.WaitOne();
			m_lstRegist.Add( nStrText );
			m_mtxRegist.ReleaseMutex();

			// タスク実行開始
			if( null == m_Cancellation )
			{
				// タスクを使用する場合
				if( true == UseTask )
				{
					start_task();
				}
				else
				{
					start_thread();
				}
			}
		}


		/// <summary>
		/// 表示実行中確認
		/// </summary>
		/// <returns>true:表示途上</returns>
		public bool is_running()
		{
			bool	b_ret	= false;
			if( null != m_Cancellation )
			{
				b_ret		= true;
			}
			return	b_ret;
		}


		/// <summary>
		/// 表示実行完了待ち
		/// </summary>
		public async Task< bool > wait_running()
		{
			return	await Task.Run( () =>
			{
				while( null != m_Cancellation )
				{
					Thread.Sleep( 100 );
				}
				return		true;
			} );
		}
		#endregion


		#region ローカル関数
		/// <summary>
		/// ラベル文字列設定
		/// </summary>
		/// <param name="nLabel">ラベル名</param>
		/// <param name="nstrText">文字列</param>
		private void set_label_text( System.Windows.Forms.Label nLabel, string nstrText )
		{
			try
			{
				if( nLabel.IsDisposed )
				{
					return;
				}
				if( nLabel.InvokeRequired )
				{
					nLabel.Invoke( ( MethodInvoker )delegate { set_label_text( nLabel, nstrText ); } );
					return;
				}

				nLabel.Text		= nstrText;
			}
			catch( System.Exception ex )
			{
				System.Diagnostics.Debug.WriteLine( ex.Message );
			}
		}


		/// <summary>
		/// タスクキャンセル
		/// </summary>
		/// <remarks>
		/// キャンセルはThreadでも使える。
		/// </remarks>
		public void cancel()
		{
			m_Cancellation?.Cancel();		// Cancel実行
		}


		/// <summary>
		/// ラベル表示
		/// </summary>
		/// <param name="cancelToken">処理キャンセル用Token</param>
		private bool execute_sub( CancellationToken cancelToken )
		{
			bool		b_ret				= true;
			int			i_count				= 0;
			string		str_regist			= "";
			string		str_label			= "";
			char[]		sz_regist			= null;
			var			sw_total			= new System.Diagnostics.Stopwatch();
			int			i_rand_interval		= ( int )( Interval * Fluctuation / 100 );		// 揺らぎ設定
			Random		random				= new Random();

			// 現在表示中のラベル文字列設定
			for( int i_loop = 0; i_loop < m_lstDisplay.Count; i_loop++ )
			{
				str_label					+= m_lstDisplay[ i_loop ];
				str_label					+= "\n\r";
			}

			// キャンセルされるまでループ
			while( false == cancelToken.IsCancellationRequested )
			{
				// 表示されている高さ確認(Parentの一定割合の高さになったら1行削除)
				while( m_Label.Height > ( int )( m_Label.Parent.Height * DisplayHeight / 100 ) )
				{
					// 一行削除
					m_lstDisplay.RemoveAt( 0 );
					str_label				= "";
					for( int i_loop = 0; i_loop < m_lstDisplay.Count; i_loop++ )
					{
						str_label			+= m_lstDisplay[ i_loop ];
						str_label			+= "\n\r";
					}
					// ラベル文字列表示
					set_label_text( m_Label, str_label );
				}

				// 登録された文字列取得
				if( 0 == str_regist.Length )
				{
					m_mtxRegist.WaitOne();
					if( 0 < m_lstRegist.Count )
					{
						str_regist			= m_lstRegist[ 0 ];
						m_lstRegist.RemoveAt( 0 );
					}
					m_mtxRegist.ReleaseMutex();
					// 登録されている文字列無しにより終了
					if( 0 == str_regist.Length )
					{
						break;
					}
					// string -> char 
					sz_regist	 			= str_regist.ToCharArray();
					m_lstDisplay.Add( str_regist );
				}

				// 既に登録された文字列が指定数以上ある場合はサクッと表示
				if( m_iLINECOUNT < m_lstRegist.Count || 0 == i_count )
				{
					// 一行追加
					str_label				+= str_regist;
					// ラベル文字列表示
					set_label_text( m_Label, str_label );
					// 周期設定
					sw_total.Restart();
					int		i_interval		= Interval + random.Next( -1 * i_rand_interval, i_rand_interval );
					while( i_interval > sw_total.ElapsedMilliseconds &&
						 false == cancelToken.IsCancellationRequested )
					{
						Thread.Sleep( i_interval - ( int )sw_total.ElapsedMilliseconds );
					}
				}
				// 一文字ずつ表示
				else
				{
					// 表示
					for( int i_loop = 0; i_loop < str_regist.Length; i_loop++ )
					{
						// キャンセルされている場合
						if( true == cancelToken.IsCancellationRequested )
						{
							break;
						}
						// 一文字追加
						str_label			+= sz_regist[ i_loop ];
						// ラベル文字列表示
						set_label_text( m_Label, str_label );
						// 1文字ずつ表示中に複数行追加された場合は一気に表示
						if( m_iLINECOUNT < m_lstRegist.Count )
						{
							continue;
						}
						// 周期設定
						sw_total.Restart();
						int		i_interval	= Interval + random.Next( -1 * i_rand_interval, i_rand_interval );
						while( i_interval > sw_total.ElapsedMilliseconds &&
							 false == cancelToken.IsCancellationRequested )
						{
							Thread.Sleep( i_interval - ( int )sw_total.ElapsedMilliseconds );
						}
					}
				}
				// キャンセルされている場合
				if( true == cancelToken.IsCancellationRequested )
				{
					break;
				}
				// 一文字追加
				str_label					+= "\n\r";
				// ラベル文字列表示
				set_label_text( m_Label, str_label );
				// クリア
				str_regist					= "";
				// 実行回数
				i_count++;
			}
			return		b_ret;
		}
		#endregion


		#region タスク
		/// <summary>
		/// タスク実行開始
		/// </summary>
		/// <remarks>
		/// C#らしくタスクを使用したいが、優先順位の設定が出来ないのでThreadも準備する
		/// </remarks>
		private async void start_task()
		{
			// 終了(キャンセル)処理用
			m_Cancellation		= new CancellationTokenSource();
			var		token		= m_Cancellation.Token;
			// タイプライター表示処理開始
			await execute( token );
			// 終了(キャンセル)処理終了
			m_Cancellation		= null;
		}


		/// <summary>
		/// タスク実行
		/// </summary>
		/// <param name="cancelToken">処理キャンセル用Token</param>
		private async Task< bool > execute( CancellationToken cancelToken )
		{
			return	await Task.Run( () =>
			{
				return		execute_sub( cancelToken );
			} );
		}
		#endregion


		#region スレッド
		/// <summary>
		/// スレッド実行開始
		/// </summary>
		/// <remarks>
		/// C#らしくタスクを使用したいが、優先順位の設定が出来ないのでThreadも準備する
		/// </remarks>
		private void start_thread()
		{
			// 終了(キャンセル)処理用
			m_Cancellation		= new CancellationTokenSource();

			// 本スレッド開始
			m_Thread			= new Thread( new ThreadStart( OnThread ) );
			m_Thread.Priority	= ThreadPriority.Lowest;
			m_Thread.Start();
		}


		/// <summary>
		/// スレッド実体
		/// </summary>
		private void OnThread()
		{
			var		token		= m_Cancellation.Token;
			execute_sub( token );
			m_Cancellation		= null;
		}
		#endregion
	}
}
