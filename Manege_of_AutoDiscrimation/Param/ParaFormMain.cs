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
		public bool EnableLogPio { get; set; } = true;

		// Debug
		public bool DebugCameraFlag { get; set; } = false;
		public bool DebugDIOFlag { get; set; } = false;

		// Graphic処理
		public string GraphicTargetNumber { get; set; } = "10";
		public System.Drawing.Rectangle GraphicTarget1 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"530,70,200,200";
		public System.Drawing.Rectangle GraphicTarget2 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"270,220,200,200";
		public System.Drawing.Rectangle GraphicTarget3 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"170,490,200,200";
		public System.Drawing.Rectangle GraphicTarget4 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"270,750,200,200";
		public System.Drawing.Rectangle GraphicTarget5 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"530,910,200,200";
		public System.Drawing.Rectangle GraphicTarget6 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"800,910,200,200";
		public System.Drawing.Rectangle GraphicTarget7 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"1060,750,200,200";
		public System.Drawing.Rectangle GraphicTarget8 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"1160,490,200,200";
		public System.Drawing.Rectangle GraphicTarget9 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"1060,220,200,200";
		public System.Drawing.Rectangle GraphicTarget10 { get; set; } = new System.Drawing.Rectangle(int.MinValue, int.MinValue, int.MinValue, int.MinValue); //"800,70,200,200";
		public string GraphicTargetSavePath { get; set; } = System.Windows.Forms.Application.StartupPath;
		public string GraphicRangeHue { get; set; } = "5";
		public string GraphicRangeSaturation { get; set; } = "0.001";
		public string GraphicRangeLightness { get; set; } = "0.001";

		// 画像処理コンボボックスの選択
		public string ComboBoxClass_SelectedItem { get; set; } = "Virtual";

		// PIO画面データ
		public string ComboBoxPIO_SelectedItem { get; set; } = "Virtual";
		public string TextBoxOutPort1_Value { get; set; } = "12";
		public string TextBoxOutPort2_Value { get; set; } = "17";
		public string TextBoxInPort1_Value { get; set; } = "12";
		public string TextBoxInPort2_Value { get; set; } = "17";

		// PIO処理
		public string CupControlOpenCloseTimer { get; set; } = "1000";
		public string CupControlNextTimer { get; set; } = "2000";

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
			EnableLogPio		= getXmlData<bool>(nXroot, nameof(EnableLogPio), EnableLogPio.ToString());

			DebugCameraFlag = getXmlData<bool>(nXroot, nameof(DebugCameraFlag), DebugCameraFlag.ToString());
			DebugDIOFlag = getXmlData<bool>(nXroot, nameof(DebugDIOFlag), DebugDIOFlag.ToString());

			GraphicTargetNumber = getXmlData<string>(nXroot, nameof(GraphicTargetNumber), GraphicTargetNumber);
			GraphicTarget1 = getXmlData(nXroot, nameof(GraphicTarget1));
			GraphicTarget2 = getXmlData(nXroot, nameof(GraphicTarget2));
			GraphicTarget3 = getXmlData(nXroot, nameof(GraphicTarget3));
			GraphicTarget4 = getXmlData(nXroot, nameof(GraphicTarget4));
			GraphicTarget5 = getXmlData(nXroot, nameof(GraphicTarget5));
			GraphicTarget6 = getXmlData(nXroot, nameof(GraphicTarget6));
			GraphicTarget7 = getXmlData(nXroot, nameof(GraphicTarget7));
			GraphicTarget8 = getXmlData(nXroot, nameof(GraphicTarget8));
			GraphicTarget9 = getXmlData(nXroot, nameof(GraphicTarget9));
			GraphicTarget10 = getXmlData(nXroot, nameof(GraphicTarget10));
			GraphicTargetSavePath = getXmlData<string>(nXroot, nameof(GraphicTargetSavePath), GraphicTargetSavePath);
			GraphicRangeHue = getXmlData<string>(nXroot, nameof(GraphicRangeHue), GraphicRangeHue);
			GraphicRangeSaturation = getXmlData<string>(nXroot, nameof(GraphicRangeSaturation), GraphicRangeSaturation);
			GraphicRangeLightness = getXmlData<string>(nXroot, nameof(GraphicRangeLightness), GraphicRangeLightness);

			ComboBoxClass_SelectedItem = getXmlData<string>(nXroot, nameof(ComboBoxClass_SelectedItem), ComboBoxClass_SelectedItem);

			ComboBoxPIO_SelectedItem = getXmlData<string>(nXroot, nameof(ComboBoxPIO_SelectedItem), ComboBoxPIO_SelectedItem);
			TextBoxOutPort1_Value    = getXmlData<string>(nXroot, nameof(TextBoxOutPort1_Value), TextBoxOutPort1_Value);
			TextBoxOutPort2_Value    = getXmlData<string>(nXroot, nameof(TextBoxOutPort2_Value), TextBoxOutPort2_Value);
			TextBoxInPort1_Value     = getXmlData<string>(nXroot, nameof(TextBoxInPort1_Value), TextBoxInPort1_Value);
			TextBoxInPort2_Value     = getXmlData<string>(nXroot, nameof(TextBoxInPort2_Value), TextBoxInPort2_Value);

			CupControlOpenCloseTimer = getXmlData<string>(nXroot, nameof(CupControlOpenCloseTimer), CupControlOpenCloseTimer);
			CupControlNextTimer = getXmlData<string>(nXroot, nameof(CupControlNextTimer), CupControlNextTimer);

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
			xml.Add(getXElement(nameof(EnableLogPio), EnableLogPio));

			xml.Add(getXElement(nameof(GraphicTarget1), GraphicTarget1));
			xml.Add(getXElement(nameof(GraphicTarget2), GraphicTarget2));
			xml.Add(getXElement(nameof(GraphicTarget3), GraphicTarget3));
			xml.Add(getXElement(nameof(GraphicTarget4), GraphicTarget4));
			xml.Add(getXElement(nameof(GraphicTarget5), GraphicTarget5));
			xml.Add(getXElement(nameof(GraphicTarget6), GraphicTarget6));
			xml.Add(getXElement(nameof(GraphicTarget7), GraphicTarget7));
			xml.Add(getXElement(nameof(GraphicTarget8), GraphicTarget8));
			xml.Add(getXElement(nameof(GraphicTarget9), GraphicTarget9));
			xml.Add(getXElement(nameof(GraphicTarget10), GraphicTarget10));

			xml.Add(getXElement(nameof(ComboBoxClass_SelectedItem), ComboBoxClass_SelectedItem));

			xml.Add(getXElement(nameof(ComboBoxPIO_SelectedItem), ComboBoxPIO_SelectedItem));
			xml.Add(getXElement(nameof(TextBoxOutPort1_Value), TextBoxOutPort1_Value));
			xml.Add(getXElement(nameof(TextBoxOutPort2_Value), TextBoxOutPort2_Value));
			xml.Add(getXElement(nameof(TextBoxInPort1_Value), TextBoxInPort1_Value));
			xml.Add(getXElement(nameof(TextBoxInPort2_Value), TextBoxInPort2_Value));

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
			setXmlData(nElem, nameof(EnableLogPio), EnableLogPio);

			setXmlData(nElem,nameof(GraphicTarget1), GraphicTarget1);
			setXmlData(nElem,nameof(GraphicTarget2), GraphicTarget2);
			setXmlData(nElem,nameof(GraphicTarget3), GraphicTarget3);
			setXmlData(nElem,nameof(GraphicTarget4), GraphicTarget4);
			setXmlData(nElem,nameof(GraphicTarget5), GraphicTarget5);
			setXmlData(nElem,nameof(GraphicTarget6), GraphicTarget6);
			setXmlData(nElem,nameof(GraphicTarget7), GraphicTarget7);
			setXmlData(nElem,nameof(GraphicTarget8), GraphicTarget8);
			setXmlData(nElem,nameof(GraphicTarget9), GraphicTarget9);
			setXmlData(nElem,nameof(GraphicTarget10), GraphicTarget10);

			setXmlData(nElem, nameof(ComboBoxClass_SelectedItem), ComboBoxClass_SelectedItem);

			setXmlData(nElem, nameof(ComboBoxPIO_SelectedItem), ComboBoxPIO_SelectedItem);
			setXmlData(nElem, nameof(TextBoxOutPort1_Value), TextBoxOutPort1_Value);
			setXmlData(nElem, nameof(TextBoxOutPort2_Value), TextBoxOutPort2_Value);
			setXmlData(nElem, nameof(TextBoxInPort1_Value), TextBoxInPort1_Value);
			setXmlData(nElem, nameof(TextBoxInPort2_Value), TextBoxInPort2_Value);
		}
		#endregion
	}
}
