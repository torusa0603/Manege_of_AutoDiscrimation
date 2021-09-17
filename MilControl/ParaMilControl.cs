using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace MilControl
{
	public class CParaMilControl : ParamBase.CParaBase
	{
		#region クラス内定数パラメータ
		// XMLファイル名
		private const string s_strFILENAME = "ParaMilControl.xml";
		#endregion


		#region アプリに必ずひとつの実体
		private static CParaMilControl m_Item = new CParaMilControl();

		/// <summary>
		/// サブクラス実態の取得
		/// </summary>
		public static CParaMilControl getInstance()
		{
			return m_Item;
		}
		#endregion


		#region プロパティ（クラス変数）
		// 画像サイズ
		public int ImageSizeWidth { get; set; } = 1000;
		public int ImageSizeHeight { get; set; } = 800;

		// for GMF( Geometric Model Finder module )
		// 1)サーチ速度
		public int GmfSpeed { get; set; } = 0;
		// 2)平滑度（ノイズ低減度）
		public double GmfSmoothness { get; set; } = -1;
		// 3)許容スコア
		public double GmfAcceptance { get; set; } = -1;
		// 4)スコア確定レベル
		public double GmfCertainty { get; set; } = -1;
		// 5)サーチ精度
		public int GmfAccuraHeight { get; set; } = 0;
		// 6)角度範囲サーチ方式計算実施
		public int GmfSearchAngleRange { get; set; } = 0;
		// 7)負側サーチ角度
		public double GmfAngleNegative { get; set; } = 60.0;
		// 8)正側サーチ角度
		public double GmfAnglePositive { get; set; } = 60.0;
		// 9)均一スケール計算実施
		public int GmfSearchScaleRange { get; set; } = 0;
		// 10)極性
		public int GmfPolarity { get; set; } = 0;
		// 11)サーチ個数
		public int GmfSearchNumber { get; set; } = 1;

		// for FRTC( Find Rotate Table Center )
		// 1)回転テーブル中心を求める際の2値化閾値
		public int FrtcBinarizeThreshold { get; set; } = 140;
		// 2)回転テーブル中心を求める際の最低特徴サイズ
		public int FrtcMinimumFeatureSize { get; set; } = 100000;
		#endregion


		#region ローカル変数
		#endregion


		#region コンストラクタ・デストラクタ・実態管理
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CParaMilControl()
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CParaMilControl()
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
			m_strClassName			= nameof( CParaMilControl );
			m_strFileName			= s_strFILENAME;
		}
		#endregion


		#region XMLパラメータファイル管理
		/// <summary>
		/// XMLファイル読み込み
		/// </summary>
		override public void readXmlFile( XElement nXroot )
		{
			ImageSizeWidth			= getXmlData< int >( nXroot, nameof( ImageSizeWidth ), ImageSizeWidth.ToString() );
			ImageSizeHeight			= getXmlData< int >( nXroot, nameof( ImageSizeHeight ), ImageSizeHeight.ToString() );

			GmfSpeed				= getXmlData< int >( nXroot, nameof( GmfSpeed ), GmfSpeed.ToString() );
			GmfSmoothness			= getXmlData< double >( nXroot, nameof( GmfSmoothness ), GmfSmoothness.ToString() );
			GmfAcceptance			= getXmlData< double >( nXroot, nameof( GmfAcceptance ), GmfAcceptance.ToString() );
			GmfCertainty			= getXmlData< double >( nXroot, nameof( GmfCertainty ), GmfCertainty.ToString() );
			GmfAccuraHeight				= getXmlData< int >( nXroot, nameof( GmfAccuraHeight ), GmfAccuraHeight.ToString() );
			GmfSearchAngleRange		= getXmlData< int >( nXroot, nameof( GmfSearchAngleRange ), GmfSearchAngleRange.ToString() );
			GmfAngleNegative		= getXmlData< double >( nXroot, nameof( GmfAngleNegative ), GmfAngleNegative.ToString() );
			GmfAnglePositive		= getXmlData< double >( nXroot, nameof( GmfAnglePositive ), GmfAnglePositive.ToString() );
			GmfSearchScaleRange		= getXmlData< int >( nXroot, nameof( GmfSearchScaleRange ), GmfSearchScaleRange.ToString() );
			GmfPolarity				= getXmlData< int >( nXroot, nameof( GmfPolarity ), GmfPolarity.ToString() );
			GmfSearchNumber			= getXmlData< int >( nXroot, nameof( GmfSearchNumber ), GmfSearchNumber.ToString() );

			FrtcBinarizeThreshold	= getXmlData< int >( nXroot, nameof( FrtcBinarizeThreshold ), FrtcBinarizeThreshold.ToString() );
			FrtcMinimumFeatureSize	= getXmlData< int >( nXroot, nameof( FrtcMinimumFeatureSize ), FrtcMinimumFeatureSize.ToString() );
		}


		/// <summary>
		/// プロパティの比較
		/// </summary>
		/// <param name="ncSrc">比較クラス</param>
		/// <returns>true:一致</returns>
		/// <remarks>
		/// SequenceEqual()はList<>用。
		/// </remarks>
		private bool compare_properties( CParaMilControl ncSrc )
		{
			bool	b_ret	= false;

			while( true )
			{
				if( this.ImageSizeWidth != ncSrc.ImageSizeWidth )				break;
				if( this.ImageSizeHeight != ncSrc.ImageSizeHeight )				break;

				if( this.GmfSpeed != ncSrc.GmfSpeed )							break;
				if( this.GmfSmoothness != ncSrc.GmfSmoothness )					break;
				if( this.GmfAcceptance != ncSrc.GmfAcceptance )					break;
				if( this.GmfCertainty != ncSrc.GmfCertainty )					break;
				if( this.GmfAccuraHeight != ncSrc.GmfAccuraHeight )						break;
				if( this.GmfSearchAngleRange != ncSrc.GmfSearchAngleRange )		break;
				if( this.GmfAngleNegative != ncSrc.GmfAngleNegative )			break;
				if( this.GmfAnglePositive != ncSrc.GmfAnglePositive )			break;
				if( this.GmfSearchScaleRange != ncSrc.GmfSearchScaleRange )		break;
				if( this.GmfPolarity != ncSrc.GmfPolarity )						break;
				if( this.GmfSearchNumber != ncSrc.GmfSearchNumber )				break;

				if( this.FrtcBinarizeThreshold != ncSrc.FrtcBinarizeThreshold )		break;
				if( this.FrtcMinimumFeatureSize != ncSrc.FrtcMinimumFeatureSize )	break;

				b_ret	= true;
				break;
			}
			return	b_ret;
		}


		/// <summary>
		/// プロパティの変更が有ったか確認
		/// </summary>
		/// <returns>true:変更有（xmlファイル保存せよ）</returns>
		/// <remarks>
		/// パラメータファイルをXMLファイルに書き込む際、変更が有ったかを確認する。
		/// プロパティのsetでModifyフラグをセットしても良いが、プロパティの記述が縦長になり、
		/// 可読性が悪化するので、こちらで判定する。
		/// </remarks>
		override public bool isModified()
		{
			// ファイル有無確認
			string			str_filename	= m_strFolderName + "\\" + m_strFileName;
			if( false == System.IO.File.Exists( str_filename ) )
			{
				return	true;
			}

			// 本クラスをローカルで生成
			CParaMilControl		ccParaMilControl		= new CParaMilControl();
			string			str_log;
			ccParaMilControl.readXmlFile( out str_log );
			// 現在残っているXMLファイルと比較して判定する
			return	!compare_properties( ccParaMilControl );
		}


		/// <summary>
		/// クラスのXElement取得用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの新規書き込みを行う為
		/// </summary>
		override public XElement getNewXElement()
		{
			XElement	xml = new XElement( m_strClassName );

			xml.Add( new XComment( " Image size of width[pixcel] " ) );
			xml.Add( getXElement( nameof( ImageSizeWidth ), ImageSizeWidth ) );
			xml.Add( new XComment( " Image size of height[pixcel] " ) );
			xml.Add( getXElement( nameof( ImageSizeHeight ), ImageSizeHeight ) );

			// 1)サーチ速度
			xml.Add( new XComment( " Speed for Geometric Model Finder. 0:Very High(Default) / 1:High / 2:Middle 3:Low " ) );
			xml.Add( getXElement( nameof( GmfSpeed ), GmfSpeed ) );
			// 2)平滑度（ノイズ低減度）
			xml.Add( new XComment( " Smoothness for Geometric Model Finder. 0.0 to 100[%](Default value:50[%] / Default:out of range) " ) );
			xml.Add( getXElement( nameof( GmfSmoothness ), GmfSmoothness ) );
			// 3)許容スコア
			xml.Add( new XComment( " Acceptance level for Geometric Model Finder. 0.0 to 100[%](Default value:60[%] / Default:out of range) " ) );
			xml.Add( getXElement( nameof( GmfAcceptance ), GmfAcceptance ) );
			// 4)スコア確定レベル
			xml.Add( new XComment( " Certainty level for Geometric Model Finder. 0.0 to 100[%](Default value:90[%] / Default:out of range) " ) );
			xml.Add( getXElement( nameof( GmfCertainty ), GmfCertainty ) );
			// 5)サーチ精度
			xml.Add( new XComment( "  Memo start. " ) );
			xml.Add( new XComment( "  *** Speed vs AccuraHeight valid settings *** " ) );
			xml.Add( new XComment( "      Speed | VHi |  Hi | Mid | Low  " ) );
			xml.Add( new XComment( "   AccuraHeight +- - -+- - -+- - -+- - - " ) );
			xml.Add( new XComment( "       High |  o  |  o  |  x  |  o   " ) );
			xml.Add( new XComment( "     Middle |  x  |  x  |  x  |  x   " ) );
			xml.Add( new XComment( "        Low |  o  |  o  |  x  |  o   " ) );
			xml.Add( new XComment( "    Middle is can not select!!! The reason is unknown. " ) );
			xml.Add( new XComment( "  Memo end." ) );
			xml.Add( new XComment( " AccuraHeight for Geometric Model Finder. 0:High(Default) / 1:Middle / 2:Low " ) );
			xml.Add( getXElement( nameof( GmfAccuraHeight ), GmfAccuraHeight ) );
			// 6)角度範囲サーチ方式計算実施
			xml.Add( new XComment( " Enable of search angle range mode for Geometric Model Finder. 0:Enable(Default) / 1:Disable " ) );
			xml.Add( getXElement( nameof( GmfSearchAngleRange ), GmfSearchAngleRange ) );
			// 7)負側サーチ角度
			xml.Add( new XComment( " Angle of negative for Geometric Model Finder. 0.0 to 180.0(Default) " ) );
			xml.Add( getXElement( nameof( GmfAngleNegative ), GmfAngleNegative ) );
			// 8)正側サーチ角度
			xml.Add( new XComment( " Angle of positive for Geometric Model Finder. 0.0 to 180.0(Default) " ) );
			xml.Add( getXElement( nameof( GmfAnglePositive ), GmfAnglePositive ) );
			// 9)均一スケール計算実施
			xml.Add( new XComment( " Enable of search scale range mode for Geometric Model Finder. 0:Disable(Default) / 1:Enable " ) );
			xml.Add( getXElement( nameof( GmfSearchScaleRange ), GmfSearchScaleRange ) );
			// 10)極性
			xml.Add( new XComment( " Polarity for Geometric Model Finder. 0:Same(Default) / 1:Any / 2:Reverse / 3:Same or Reverse " ) );
			xml.Add( getXElement( nameof( GmfPolarity ), GmfPolarity ) );
			// 11)サーチ個数
			xml.Add( new XComment( " Number of search for Geometric Model Finder. 0:All / 1:Default / n:User setting " ) );
			xml.Add( getXElement( nameof( GmfSearchNumber ), GmfSearchNumber ) );

			// 1)回転テーブル中心を求める際の2値化閾値
			xml.Add( new XComment( " Binarize threshold for Find Rotate Table Center. 0 to 255 140(Defualt)" ) );
			xml.Add( getXElement( nameof( FrtcBinarizeThreshold ), FrtcBinarizeThreshold ) );
			// 2)回転テーブル中心を求める際の2値化閾値
			xml.Add( new XComment( " Minimumsize of Feature for Find Rotate Table Center. 100000(Defualt)" ) );
			xml.Add( getXElement( nameof( FrtcMinimumFeatureSize ), FrtcMinimumFeatureSize ) );

			return	xml;
		}


		/// <summary>
		/// クラスのXElement変更用
		///  ユーザークラスで複数のデータクラスから本関数を呼び出し1本のXMLファイルの変更書き込みを行う為
		/// </summary>
		override public void replaceXElement( XElement nElem )
		{
			setXmlData( nElem, nameof( ImageSizeWidth ), ImageSizeWidth );
			setXmlData( nElem, nameof( ImageSizeHeight ), ImageSizeHeight );

			setXmlData( nElem, nameof( GmfSpeed ), GmfSpeed );
			setXmlData( nElem, nameof( GmfSmoothness ), GmfSmoothness );
			setXmlData( nElem, nameof( GmfAcceptance ), GmfAcceptance );
			setXmlData( nElem, nameof( GmfCertainty ), GmfCertainty );
			setXmlData( nElem, nameof( GmfAccuraHeight ), GmfAccuraHeight );
			setXmlData( nElem, nameof( GmfSearchAngleRange ), GmfSearchAngleRange );
			setXmlData( nElem, nameof( GmfAngleNegative ), GmfAngleNegative );
			setXmlData( nElem, nameof( GmfAnglePositive ), GmfAnglePositive );
			setXmlData( nElem, nameof( GmfSearchScaleRange ), GmfSearchScaleRange );
			setXmlData( nElem, nameof( GmfPolarity ), GmfPolarity );
			setXmlData( nElem, nameof( GmfSearchNumber ), GmfSearchNumber );

			setXmlData( nElem, nameof( FrtcBinarizeThreshold ), FrtcBinarizeThreshold );
			setXmlData( nElem, nameof( FrtcMinimumFeatureSize ), FrtcMinimumFeatureSize );
		}
		#endregion
	}
}
