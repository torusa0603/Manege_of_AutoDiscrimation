using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using System.Windows.Forms;

namespace ParamBase
{
	abstract public class CParaBase
	{
		#region ローカル変数
		// アプリ名(ルートになる)
		protected static string m_strProductName = "Application.ProductName";
		// コメント(フルパスファイル名)
		protected static string m_strComment = "";
		// クラス名(ルートの子になる。ユーザークラスを指定する事)
		protected string m_strClassName = nameof( CParaBase );
		// XMLフォルダ名(ユーザークラスで指定する事)
		protected string m_strFolderName = "prm";
		// XMLファイル名(ユーザークラスで指定する事)
		protected string m_strFileName = nameof( CParaBase ) + ".xml";
		#endregion

		#region メンバ関数
		/// <summary>
		/// アプリ名の設定
		/// </summary>
		public void setProductName( string nstrProductName )
		{
			m_strProductName	= nstrProductName;
		}

		/// <summary>
		/// コメントの設定
		/// </summary>
		public void setComment( string nstrComment )
		{
			m_strComment	= nstrComment;
		}

		/// <summary>
		/// クラス名の設定
		/// </summary>
		public void setClassName( string nstrClassName )
		{
			m_strClassName	= nstrClassName;
		}

		/// <summary>
		/// フォルダ名の設定
		/// </summary>
		public void setFolderName( string nstrFolderName )
		{
			m_strFolderName	= nstrFolderName;
		}

		/// <summary>
		/// ファイル名の設定
		/// </summary>
		public void setFileName( string nstrFileName )
		{
			m_strFileName	= nstrFileName;
		}
		#endregion


		#region XElement取得用
		/// <summary>
		/// int/float/double/bool/string型 XML XElement取得用
		/// </summary>
		protected XElement getXElement< T >( string nstrName, T nData, string nstrValue = "value" )
		{
			return new XElement( nstrName, new XAttribute( nstrValue, nData ) );
		}


		/// <summary>
		/// List型 XML XElement取得用
		/// </summary>
		protected XElement getXElement< T >( string nstrName, List< T > nlstData, string nstrValue = "list" )
		{
			return new XElement( nstrName, new XAttribute( nstrValue, String.Join( ",", nlstData ) ) );
		}


		/// <summary>
		/// Rectangle型 XML XElement取得用
		/// </summary>
		protected XElement getXElement( string nstrName, System.Drawing.Rectangle nBounds )
		{
			return new XElement( nstrName, 
									new XAttribute( nameof( nBounds.X ), nBounds.X ),
									new XAttribute( nameof( nBounds.Y ), nBounds.Y ),
									new XAttribute( nameof( nBounds.Width ), nBounds.Width ),
									new XAttribute( nameof( nBounds.Height ), nBounds.Height ) );
		}
		#endregion


		#region Data編集用
		/// <summary>
		/// int/float/double/bool/string型 XML Data設定用
		/// </summary>
		protected void setXmlData< T >( XElement nXml, string nstrName, T ntDefault, string nstrValue = "value" )
		{
			XElement xelm		= nXml.Element( nstrName );
			XAttribute xatt		= xelm?.Attribute( nstrValue );
			if( null != xatt )
			{ 
				xatt.Value		= ntDefault.ToString();
			}
			else
			{
				nXml.Add( getXElement( nstrName, ntDefault, nstrValue ) );
			}
		}


		/// <summary>
		/// List型 XML Data設定用
		/// </summary>
		protected void setXmlData< T >( XElement nXml, string nstrName, List< T > nList, string nstrValue = "list" )
		{
			string 		str_temp	= "";
			for( int i_loop = 0; i_loop < nList.Count; i_loop++ )
			{
				if( "" != str_temp ||
					0 < i_loop && typeof( T ) == typeof( string ) )
				{
					str_temp		+= ",";
				}
				str_temp			+= nList[ i_loop ].ToString();
			}
			setXmlData( nXml, nstrName, str_temp, nstrValue );
		}


		/// <summary>
		/// Rectangleg型 XML Data設定用
		/// </summary>
		protected void setXmlData( XElement nXml, string nstrName, System.Drawing.Rectangle nBounds )
		{
			XElement xelm		= nXml.Element( nstrName );
			if( null != xelm )
			{ 
				xelm.Attribute( nameof( nBounds.X ) ).Value			= nBounds.X.ToString();
				xelm.Attribute( nameof( nBounds.Y ) ).Value			= nBounds.Y.ToString();
				xelm.Attribute( nameof( nBounds.Width ) ).Value		= nBounds.Width.ToString();
				xelm.Attribute( nameof( nBounds.Height ) ).Value	= nBounds.Height.ToString();
			}
		}
		#endregion



		#region Data取得用
		/// <summary>
		/// int/float/double/bool/string型 XML Data取得用
		/// </summary>
		protected T getXmlData< T >( XElement nXml, string nstrName, string nstrDefault, string nstrValue = "value" )
		{
			T		ret			= default( T );
			string	str_value	= nstrDefault;

			XElement	xelm	= nXml?.Element( nstrName );
			XAttribute	xatt	= xelm?.Attribute( nstrValue );
			if( null != xatt )
			{ 
				str_value = xatt.Value;
			}

			// 変換
			Type type = typeof( T );
			if( null == str_value )
			{
				ret = default( T );
			}
			else if( type == typeof( int ) )
			{
				ret = ( T )( object )int.Parse( str_value );
			}
			else if( type == typeof( uint ) )
			{
				ret = ( T )( object )uint.Parse( str_value );
			}
			else if( type == typeof( short ) )
			{
				ret = ( T )( object )short.Parse( str_value );
			}
			else if( type == typeof( ushort ) )
			{
				ret = ( T )( object )ushort.Parse( str_value );
			}
			else if( type == typeof( bool ) )
			{
				ret = ( T )( object )bool.Parse( str_value );
			}
			else if( type == typeof( float ) )
			{
				ret = ( T )( object )float.Parse( str_value );
			}
			else if( type == typeof( double ) )
			{
				ret = ( T )( object )double.Parse( str_value );
			}
			else if( type == typeof( string ) )
			{
				ret = ( T )( object )( str_value );
			}
			else
			{
				ret = default( T );
			}
			return	ret;
		}


		/// <summary>
		/// List型 XML Data取得用
		/// </summary>
		protected List< T > getXmlListData< T >( XElement nXml, string nstrName, string nstrDefault, string nstrValue = "list" )
		{
			List< T >	lstRes	= new List< T >();
			string 		strTemp	= getXmlData< string >( nXml, nstrName, nstrDefault, nstrValue );

			// 文字列分解
			string[]	aryStr	= strTemp?.Split( ',' );
			// 変換
			Type type = typeof( T );
			for( int i_loop = 0; i_loop < aryStr?.Count(); i_loop++ )
			{
				if( type == typeof( int ) )
				{
					lstRes.Add( ( T )( object )int.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( uint ) )
				{
					lstRes.Add( ( T )( object )uint.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( short ) )
				{
					lstRes.Add( ( T )( object )short.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( ushort ) )
				{
					lstRes.Add( ( T )( object )ushort.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( bool ) )
				{
					lstRes.Add( ( T )( object )bool.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( long ) )
				{
					lstRes.Add( ( T )( object )long.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( ulong ) )
				{
					lstRes.Add( ( T )( object )ulong.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( bool ) )
				{
					lstRes.Add( ( T )( object )bool.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( float ) )
				{
					lstRes.Add( ( T )( object )float.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( double ) )
				{
					lstRes.Add( ( T )( object )double.Parse( aryStr[ i_loop ] ) );
				}
				else if( type == typeof( string ) )
				{
					lstRes.Add( ( T )( object )aryStr[ i_loop ] );
				}
				else
				{
					lstRes.Add( default( T ) );
				}
			}

			return	lstRes;
		}


		/// <summary>
		/// Rectangleg型 XML Data取得用
		/// </summary>
		protected System.Drawing.Rectangle getXmlData( XElement nXml, string nstrName )
		{
			System.Drawing.Rectangle	Bounds 			= new System.Drawing.Rectangle( int.MinValue, int.MinValue, int.MinValue, int.MinValue );

			XElement	xelm	= nXml?.Element( nstrName );

			if( null != xelm )
			{
				Bounds.X		= int.Parse( xelm.Attribute( nameof( Bounds.X ) ).Value );
				Bounds.Y		= int.Parse( xelm.Attribute( nameof( Bounds.Y ) ).Value );
				Bounds.Width	= int.Parse( xelm.Attribute( nameof( Bounds.Width ) ).Value );
				Bounds.Height	= int.Parse( xelm.Attribute( nameof( Bounds.Height ) ).Value );
			}

			return	Bounds;
		}
		#endregion


		#region XML ファイル読み込み用
		/// <summary>
		/// XML ファイル読み込み用(ユーザークラス用実体)
		/// </summary>
		/// <example>
		///  ユーザークラスでXMLファイル読み込み、ユーザークラスのプロパティを設定する関数を準備する事
		/// </example>
		abstract public void readXmlFile( XElement nXroot );


		/// <summary>
		/// XML ファイル読み込み用
		/// 本関数からユーザークラスのreadXmlFile()を呼び出す
		/// </summary>
		/// <param name="nStrErrLog">エラーログ文字列。失敗した際、ユーザー側で表示するなり、ログファイルに書き込むなりする</param>
		/// <returns>true：成功 / false：失敗(ただし初期データ動作させることは出来る)</returns>
		public bool readXmlFile( out string nStrErrLog )
		{
			bool	b_ret	= true;
			string	str_log	= "";
			try
			{
				string			str_filename	= System.IO.Path.GetDirectoryName(m_strComment) + "\\" + m_strFolderName + "\\" + m_strFileName;
				XElement		xroot			= null;
				if( true == File.Exists( str_filename ) )
				{
					// xmlファイルを読み込む
					XDocument	xdoc			= XDocument.Load( str_filename );
					XElement	xapp			= xdoc?.Element( m_strProductName );
					xroot						= xapp?.Element( m_strClassName );
				}
				// ユーザークラス読み込み処理
				readXmlFile( xroot );
			}
			catch( System.Exception ex )
			{
				str_log		= "設定ファイル" + m_strFileName + "が壊れています! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				b_ret	= false;
			}
			nStrErrLog	= str_log;
			return	b_ret;
		}
		#endregion


		#region XML ファイル書き込み用
		/// <summary>
		/// プロパティの変更が有ったか、現在のxmlファイルを読み返して確認する。
		/// ファイルが無ければ、「プロパティの変更が有った」と同意とする。
		/// </summary>
		virtual public bool isModified()
		{
			return		true;
		}

		/// <summary>
		/// クラスのXElement取得用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの新規書き込みを行う為
		/// </summary>
		/// <example>
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの新規書き込みを行う例
		///		XElement	xml = new XElement( Application.ProductName );			← ルートがプロダクト名になる
		///		xml.Add( cParaApp1.getXElement() );									← 本クラスを継承したデータクラス1よりElementを取得追加
		///		xml.Add( cParaApp2.getXElement() );									← 本クラスを継承したデータクラス1よりElementを取得追加
		///		XDocument	doc = new XDocument( cParaApp1.getXComment(), xml );	← コメントを追加したDocument生成
		///		doc.Save( Application.ProductName + ".xml" );						← プロダクト名.xmlのファイル書き込み
		/// </example>
		abstract public XElement getNewXElement();

		/// <summary>
		/// クラスのXElement取得用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの変更書き込みを行う為
		/// </summary>
		abstract public void replaceXElement( XElement nElem );

		/// <summary>
		/// クラスのXComment取得用
		/// </summary>
		public XComment getXComment()
		{
			return new XComment( m_strComment );
		}


		/// <summary>
		/// XMLファイル出力(クラス単位でxmlファイルを作成するとき使用)
		/// </summary>
		/// <param name="nStrErrLog">エラーログ文字列。失敗した際、ユーザー側で表示するなり、ログファイルに書き込むなりする</param>
		/// <returns>true：成功 / false：失敗(ただし初期データ動作させることは出来る)</returns>
		public bool writeXmlFile( out string nStrErrLog )
		{
			bool	b_ret	= true;
			string	str_log	= "";
			try
			{
				if( true == isModified() )
				{
					// フォルダ有無確認
					if( false == Directory.Exists( m_strFolderName ) )
					{
						Directory.CreateDirectory( m_strFolderName );
					}

					string			str_filename	= m_strFolderName + "\\" + m_strFileName;
					XDocument		doc				= null;

					// 新規作成の場合、コメントも含め全て作成
					if( false == File.Exists( str_filename ) )
					{
						XElement	xml = new XElement( m_strProductName );
						xml.Add( getNewXElement() );
						doc = new XDocument( getXComment(), xml );
					}
					// ファイルが存在する場合、xmlファイルを読み込んで値を変更する
					else
					{
						doc = XDocument.Load( str_filename );
						XElement app = doc?.Element( m_strProductName );
						XElement root = app?.Element( m_strClassName );
						if( null != root )
						{
							replaceXElement( root );
						}
						else
						{
							str_log		= "設定ファイル" + m_strFileName + "が壊れています! ";
							b_ret	= false;
						}
					}
					if( true == b_ret )
					{
						doc?.Save( str_filename );
					}
				}
			}
			catch( System.Exception ex )
			{
				str_log		= "設定ファイル" + m_strFileName + "が壊れています! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				b_ret	= false;
			}
			nStrErrLog	= str_log;
			return	b_ret;
		}
		#endregion
	}
}
