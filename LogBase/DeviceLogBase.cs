using System;

namespace LogBase
{
	/// <summary>
	/// デバイス(DeviceやCounter)の抽象クラス(ログ専用)
	/// </summary>
	/// <rematks>
	/// 抽象クラスと言いながら抽象関数は持たない。
	/// 本抽象クラスを各デバイス(DeviceやCounter)の抽象クラスが継承する。
	///
	/// 本クラスを継承すると3つのログファイルが生成される。
	///  1.エラーログ		エラー発生時ログとして記録（最上位ログ）
	///  2.実行ログ			実行した関数名等をログとして記録
	///  3.デバイスログ		デバイスとの通信ログとして記録
	/// エラーログにログ文字列を追加すると、実行ログ、デバイスログにもログ文字列が追加される（事も可能）。
	/// 実行ログにログ文字列を追加すると、デバイスログにもログ文字列が追加される（事も可能）。	
	/// </rematks>
	abstract public class CDeviceControlLog
	{
		#region クラス内定義
		protected const string		m_strDEVICE_NAME	= "Device";			// デバイス名
		#endregion


		#region ローカル変数
		protected string			m_strDeviceName		= m_strDEVICE_NAME;	// デバイス名
		#endregion


		#region プロパティ
		protected string			LogErrorName	{ get; set; }
		protected string			LogExecuteName	{ get; set; }
		protected string			LogDeviceName	{ get; set; }

		protected string			LabelName		{ get; set; }
		#endregion


		#region ログクラス
		protected CLogBase			m_cLogError			= null;				// エラーログ
		protected CLogBase			m_cLogExecute		= null;				// 実行ログ
		protected CLogBase			m_cLogDevice		= null;				// デバイスログ

		/// <summary>
		/// エラーログクラスの実体設定
		/// </summary>
		/// <param name="nstrName">実体を設定する為の文字列</param>
		public void setLogErrorInstance( string nstrName )
		{
			LogErrorName		= nstrName;
			m_cLogError			= CLogBase.getInstance( nstrName );
		}


		/// <summary>
		/// 実行ログクラスの実体設定
		/// </summary>
		/// <param name="nstrName">実体を設定する為の文字列</param>
		public void setLogExecuteInstance( string nstrName )
		{
			LogExecuteName		= nstrName;
			m_cLogExecute		= CLogBase.getInstance( nstrName );
		}


		/// <summary>
		/// デバイスログクラスの実体設定
		/// </summary>
		/// <param name="nstrName">実体を設定する為の文字列</param>
		public void setLogDeviceInstance( string nstrName )
		{
			LogDeviceName		= nstrName;
			m_cLogDevice		= CLogBase.getInstance( nstrName );
		}


		/// <summary>
		/// エラーログ文字列設定
		/// </summary>
		/// <param name="nstrName">ログ文字列</param>
		/// <param name="nbOnly">エラーログのみ記録する</param>
		protected void setLogError( string nstrText, bool nbOnly = false )
		{
			if( null != m_cLogError )
			{
				string str_log = "[" + m_strDeviceName + "]";
				m_cLogError.outputLog( str_log + nstrText );
			}
			if( false == nbOnly )
			{
				setLogExecute( nstrText, true );
				setLogDevice( nstrText );
			}
		}


		/// <summary>
		/// 実行ログ文字列設定
		/// </summary>
		/// <param name="nstrName">ログ文字列</param>
		/// <param name="nbOnly">実行ログのみ記録する</param>
		protected void setLogExecute( string nstrText, bool nbOnly = false )
		{
			if( null != m_cLogExecute )
			{
				string str_log = "[" + m_strDeviceName + "]";
				m_cLogExecute.outputLog( str_log + nstrText );
			}
			if( false == nbOnly )
			{
				setLogDevice( nstrText );
			}
		}


		/// <summary>
		/// デバイスログ文字列設定
		/// </summary>
		/// <param name="nstrName">ログ文字列</param>
		protected void setLogDevice( string nstrText )
		{
			if( null != m_cLogDevice )
			{
				m_cLogDevice.outputLog( nstrText );
			}
		}
		#endregion


		#region ラベルクラス
		protected CTypeWriterLabel	m_cTypeWriterLabel	= null;				// ラベルクラス

		/// <summary>
		/// エラーログクラスの実体設定
		/// </summary>
		/// <param name="nstrName">実体を設定する為の文字列</param>
		public void setLabelInstance( string nstrName )
		{
			LabelName			= nstrName;
			m_cTypeWriterLabel	= CTypeWriterLabel.getInstance( nstrName );
		}


		/// <summary>
		/// ラベル文字列設定
		/// </summary>
		/// <param name="nstrName">ログ文字列</param>
		protected void setLabel( string nstrText )
		{
			if( null != m_cTypeWriterLabel )
			{
				m_cTypeWriterLabel.set( nstrText );
			}
		}
		#endregion


		#region コンストラクタ / デストラクタ
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CDeviceControlLog()
		{
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CDeviceControlLog()
		{
		}
		#endregion
	}
}
