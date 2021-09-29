using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Manege_of_AutoDiscrimation
{
	class CParaFormMain : ParamBase.CParaBase
	{
		#region クラス内定数パラメータ
		// XMLファイル名
		private const string s_strFILENAME = "ParaFormMain.xml";
		#endregion


		#region アプリに必ずひとつの実体
		private static CParaFormMain m_Item = new CParaFormMain();

		/// <summary>
		/// サブクラス実態の取得
		/// </summary>
		public static CParaFormMain getInstance()
		{
			return m_Item;
		}
		#endregion


		#region プロパティ（クラス変数）
		// 画面サイズ
		public System.Drawing.Rectangle Bounds { get; set; } = new System.Drawing.Rectangle( int.MinValue, int.MinValue, int.MinValue, int.MinValue );
		public bool EnableLogError { get; set; } = true;
		public bool EnableLogExecute { get; set; } = true;
		public bool EnableLogCamera { get; set; } = true;

		// Debug
		public bool DebugCameraFlag { get; set; } = false;

		// 画像処理コンボボックスの選択
		public string ComboBoxClass_SelectedItem { get; set; } = "Virtual";

		public string PythonPictureFolder { get; set; }= "";

		#endregion


		#region ローカル変数
		#endregion


		#region コンストラクタ・デストラクタ・実態管理
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CParaFormMain()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CParaFormMain()
		{
			// 本パラメータクラスの初期化
			initialize_for_constructor();
		}


		/// <summary>
		/// コンストラクタ用初期化
		/// </summary>
		private void initialize_for_constructor()
		{
			// Baseクラスパラメータの初期化
			m_strClassName = nameof( CParaFormMain );
			m_strFileName = s_strFILENAME;
		}
		#endregion


		#region XMLパラメータファイル管理
		/// <summary>
		/// XMLファイル読み込み
		/// </summary>
		override public void readXmlFile( XElement nXroot )
		{
			Bounds				= getXmlData(nXroot, nameof(Bounds));
			EnableLogError		= getXmlData<bool>(nXroot, nameof(EnableLogError), EnableLogError.ToString());
			EnableLogExecute	= getXmlData<bool>(nXroot, nameof(EnableLogExecute), EnableLogExecute.ToString());
			EnableLogCamera		= getXmlData<bool>(nXroot, nameof(EnableLogCamera), EnableLogCamera.ToString());

			DebugCameraFlag = getXmlData<bool>(nXroot, nameof(DebugCameraFlag), DebugCameraFlag.ToString());
			ComboBoxClass_SelectedItem = getXmlData<string>(nXroot, nameof(ComboBoxClass_SelectedItem), ComboBoxClass_SelectedItem.ToString());
			PythonPictureFolder = getXmlData<string>(nXroot, nameof(PythonPictureFolder), PythonPictureFolder.ToString());
		}


		/// <summary>
		/// クラスのXElement取得用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの新規書き込みを行う為
		/// </summary>
		override public XElement getNewXElement()
		{
			XElement	xml = new XElement( m_strClassName );
			xml.Add(getXElement(nameof(Bounds), Bounds));
			xml.Add(getXElement(nameof(EnableLogError), EnableLogError));
			xml.Add(getXElement(nameof(EnableLogExecute), EnableLogExecute));
			xml.Add(getXElement(nameof(EnableLogCamera), EnableLogCamera));
			return xml;
		}


		/// <summary>
		/// クラスのXElement変更用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの変更書き込みを行う為
		/// </summary>
		override public void replaceXElement( XElement nElem )
		{
			setXmlData(nElem, nameof(Bounds), Bounds);
			setXmlData(nElem, nameof(EnableLogError), EnableLogError);
			setXmlData(nElem, nameof(EnableLogExecute), EnableLogExecute);
			setXmlData(nElem, nameof(EnableLogCamera), EnableLogCamera);

			setXmlData(nElem, nameof(ComboBoxClass_SelectedItem), ComboBoxClass_SelectedItem);
			setXmlData(nElem, nameof(PythonPictureFolder), PythonPictureFolder);
			
		}
		#endregion
	}
}
