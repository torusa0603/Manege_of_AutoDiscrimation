using System;

namespace LogBase
{
	/// <summary>
	/// デバイス(DeviceやCounter)の抽象クラス(ログ専用)
	/// </summary>
	/// <rematks>
	/// 抽象クラスと言いながら抽象関数は持たない
	/// 本抽象クラスを各デバイス(DeviceやCounter)の抽象クラスが継承する
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
		#endregion


		#region ログクラス
		protected CLogBase			m_cLogError		= null;					// エラーログ
		protected CLogBase			m_cLogExecute	= null;					// 実行ログ
		protected CLogBase			m_cLogDevice	= null;					// デバイスログ

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
		protected void setLogError( string nstrText )
		{
			if( null != m_cLogError )
			{
				string str_log = "[" + m_strDeviceName + "]";
				m_cLogError.outputLog( str_log + nstrText );
			}
		}


		/// <summary>
		/// 実行ログ文字列設定
		/// </summary>
		/// <param name="nstrName">ログ文字列</param>
		protected void setLogExecute( string nstrText )
		{
			if( null != m_cLogExecute )
			{
				string str_log = "[" + m_strDeviceName + "]";
				m_cLogExecute.outputLog( str_log + nstrText );
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


		#region コンストラクタ / デストラクタ
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CDeviceControlLog()
		{
		}
		#endregion
	}
}
