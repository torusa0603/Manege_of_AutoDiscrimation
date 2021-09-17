using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Matrox.MatroxImagingLibrary;

namespace MilControl
{
	/// <summary>
	/// MILを使用したパターンマッチング実行クラス
	///  Gmf -> Geometric Model Finder module 
	/// </summary>
	/// <remarks>
	/// MILHelp.chm内のMmod.csを参考に作成
	/// </remarks>
	public class CGmf : CBase
	{
		#region クラス内定義
		new private const string			m_strDEVICE_NAME		= "MIL.GMF";		// デバイス名
		#endregion


		#region ローカル変数
		#endregion


		#region プログラム開始時終了時
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CGmf()
		{
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CGmf()
		{
			// デバイス名
			m_strDeviceName		= m_strDEVICE_NAME;
		}
		#endregion


		#region オープン/クローズ関数
		/// <summary>
		/// クローズ
		/// </summary>
		public new void close()
		{
			base.close();
		}


		/// <summary>
		/// オープン
		/// </summary>
		public void open()
		{
			base.open( false );
		}
		#endregion


		#region 実行
		/// <summary>
		/// パターンマッチング実行関数
		/// </summary>
		/// <param name="nStrTargetFileName">検索対象画像（グレイ化済み）のフルパスファイル名</param>
		/// <param name="nStrObjectFileName">検索オブジェクト画像（グレイ化済み）のフルパスファイル名</param>
		/// <param name="ntagResult">結果配列</param>
		/// <returns>true:成功</returns>
		/// <remarks>
		/// MIL.MmodControl()の戻りがbooleanで無い割に、失敗するとメッセージボックスが出る。
		/// しかも、メッセージボックスが何の設定で出たか良く判らないので、手掛かりとしてログを残しつつ、エラーチェックを行う。
		/// </returns>
		public bool execute( string nStrImageFileName, string nStrObjectFileName, out tag_GMF_Result[] ntagResult )
		{
			bool			b_ret				= false;
			string 			str_log, str_para;
			MIL_INT			i_object_width, i_object_height;				// 画像サイズ
			MIL_ID			mil_Object			= MIL.M_NULL;				// 検索オブジェクト画像ID
			MIL_ID			mil_Image			= MIL.M_NULL;				// 検索対象画像ID
			MIL_ID			mil_GraphicList 	= MIL.M_NULL;				// Graphic list indentifier.
			MIL_ID			mil_SearchContext	= MIL.M_NULL;				// Search context.
			MIL_ID			mil_Result			= MIL.M_NULL;				// Result identifier.
			MIL_INT			i_num_results		= 0;						// Number of results found.
			double			d_time				= 0.0;						// Bench variable.
			const double	d_MODELDRAWCOLOR	= MIL.M_COLOR_RED;			// 描画色

			// 初期化
			ntagResult		= null;
			// 開始
			try
			{
				while( true )
				{
					// ログ残し
					str_log			= "Start of geometric model finder.";
					setLogDevice( str_log );
					setLabel( str_log );
					// 検索オブジェクト画像ロード&表示
					MIL.MbufRestore( nStrObjectFileName, m_MilSystem, ref mil_Object );
					// 表示の設定
					set_show_display( mil_Object );
					// グラフィックリストを割り当て
					MIL.MgraAllocList( m_MilSystem, MIL.M_DEFAULT, ref mil_GraphicList );
					// MILディスプレイ設定を制御
					MIL.MdispControl( m_MilDisplay, MIL.M_ASSOCIATED_GRAPHIC_LIST_ID, mil_GraphicList );
					// GMFコンテキストを割り当て
					MIL.MmodAlloc( m_MilSystem, MIL.M_GEOMETRIC, MIL.M_DEFAULT, ref mil_SearchContext );
					// モデルファインダー結果バッファを割り当て
					MIL.MmodAllocResult( m_MilSystem, MIL.M_DEFAULT, ref mil_Result );
					// GFMコンテキストに対して、モデルを追加
					i_object_width		= MIL.MbufInquire( mil_Object, MIL.M_SIZE_X, MIL.M_NULL );
					i_object_height		= MIL.MbufInquire( mil_Object, MIL.M_SIZE_Y, MIL.M_NULL );
					MIL.MmodDefine( mil_SearchContext, MIL.M_IMAGE, mil_Object, 0, 0, i_object_width, i_object_height );

					// 1)サーチ速度
					{
						MIL_INT			i_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						switch( m_cParaMilControl.GmfSpeed )
						{
							case 0	:	i_para		= MIL.M_VERY_HIGH;	str_para	= "MIL.M_VERY_HIGH";	break;
							case 1	:	i_para		= MIL.M_HIGH;		str_para	= "MIL.M_HIGH";			break;
							case 2	:	i_para		= MIL.M_MIDDLE;		str_para	= "MIL.M_MIDDLE";		break;
							case 3	:	i_para		= MIL.M_LOW;		str_para	= "MIL.M_LOW";			break;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_CONTEXT, MIL.M_SPEED, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_SPEED, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 2)平滑度（ノイズ低減度） default=50
					{
						double			d_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						if( 0 <= m_cParaMilControl.GmfSmoothness && m_cParaMilControl.GmfSmoothness <= 100 )
						{
							d_para					= m_cParaMilControl.GmfSmoothness;
							str_para				= m_cParaMilControl.GmfSmoothness.ToString();
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_CONTEXT, MIL.M_SMOOTHNESS, d_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_SMOOTHNESS, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 3)許容スコア default=60
					{
						double			d_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						if( 0 <= m_cParaMilControl.GmfAcceptance && m_cParaMilControl.GmfAcceptance <= 100 )
						{
							d_para					= m_cParaMilControl.GmfAcceptance;
							str_para				= m_cParaMilControl.GmfAcceptance.ToString();
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_ACCEPTANCE, d_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_ACCEPTANCE, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 4)スコア確定レベル default=90
					{
						double			d_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						if( 0 <= m_cParaMilControl.GmfCertainty && m_cParaMilControl.GmfCertainty <= 100 )
						{
							d_para					= m_cParaMilControl.GmfCertainty;
							str_para				= m_cParaMilControl.GmfCertainty.ToString();
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_CERTAINTY, d_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_CERTAINTY, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 5)サーチ精度
					{
						MIL_INT			i_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						switch( m_cParaMilControl.GmfAccuraHeight )
						{
							case 0	:	i_para		= MIL.M_HIGH;		str_para	= "MIL.M_HIGH";			break;
							case 1	:	i_para		= MIL.M_MIDDLE;		str_para	= "MIL.M_MIDDLE";		break;
							case 2	:	i_para		= MIL.M_LOW;		str_para	= "MIL.M_LOW";			break;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_CONTEXT, MIL.M_ACCURACY, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_ACCURAHeight, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 6)角度範囲サーチ方式計算実施
					{
						MIL_INT			i_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						switch( m_cParaMilControl.GmfSearchAngleRange )
						{
							case 0	:	i_para		= MIL.M_ENABLE ;	str_para	= "MIL.M_ENABLE ";		break;
							case 1	:	i_para		= MIL.M_DISABLE ;	str_para	= "MIL.M_DISABLE ";		break;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_ANGLE_RANGE, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_SEARCH_ANGLE_RANGE, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 7)負側サーチ角度
					{
						double	d_para	= m_cParaMilControl.GmfAngleNegative;
						if( 0.0 > d_para || d_para > 180.0 )
						{
							d_para			= 180.0;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_ANGLE_DELTA_NEG, d_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_ANGLE_DELTA_NEG, " + d_para.ToString() + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 8)正側サーチ角度
					{
						double	d_para	= m_cParaMilControl.GmfAnglePositive;
						if( 0.0 > d_para || d_para > 180.0 )
						{
							d_para			= 180.0;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_ANGLE_DELTA_POS, d_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_ANGLE_DELTA_POS, " + d_para.ToString() + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 9)均一スケール計算実施
					{
						MIL_INT			i_para		= MIL.M_DEFAULT;	str_para	= "MIL.M_DEFAULT";
						switch( m_cParaMilControl.GmfSearchScaleRange )
						{
							case 0	:	i_para		= MIL.M_DISABLE ;	str_para	= "MIL.M_DISABLE ";		break;
							case 1	:	i_para		= MIL.M_ENABLE ;	str_para	= "MIL.M_ENABLE ";		break;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_CONTEXT, MIL.M_SEARCH_SCALE_RANGE, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_SEARCH_SCALE_RANGE, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 10)極性
					{
						MIL_INT			i_para	= MIL.M_DEFAULT;			str_para	= "MIL.M_DEFAULT";
						switch( m_cParaMilControl.GmfPolarity )
						{
							case 0	:	i_para	= MIL.M_SAME;				str_para	= "MIL.M_SAME";				break;
							case 1	:	i_para	= MIL.M_ANY;				str_para	= "MIL.M_ANY";				break;
							case 2	:	i_para	= MIL.M_REVERSE;			str_para	= "MIL.M_REVERSE";			break;
							case 3	:	i_para	= MIL.M_SAME_OR_REVERSE;	str_para	= "MIL.M_SAME_OR_REVERSE";	break;
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_POLARITY, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_POLARITY, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}
					// 11)サーチ個数 default=1
					{
						MIL_INT			i_para	= MIL.M_DEFAULT;			str_para	= "MIL.M_DEFAULT";
						if( 0 == m_cParaMilControl.GmfSearchNumber )
						{
							i_para	= MIL.M_ALL;							str_para	= "MIL.M_ALL";
						}
						else
						{
							i_para	= m_cParaMilControl.GmfSearchNumber;	str_para	= i_para.ToString();
						}
						MIL.MmodControl( mil_SearchContext, MIL.M_DEFAULT, MIL.M_NUMBER, i_para );
						str_log			= "MIL.MmodControl( ,,MIL.M_NUMBER, " + str_para + " )";
						setLogDevice( str_log );
						setLabel( str_log );
						if( true == is_mil_error() )	break;
					}

					// GFMコンテキストを前処理
					MIL.MmodPreprocess( mil_SearchContext, MIL.M_DEFAULT );
					// 赤ペン指定
					MIL.MgraColor( MIL.M_DEFAULT, d_MODELDRAWCOLOR );
					///*
					// 赤四角で囲む(デバッグ用に残す)
					MIL.MmodDraw( MIL.M_DEFAULT, mil_SearchContext, mil_GraphicList,
									MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, 0, MIL.M_ORIGINAL );
					// 描画クリア
					MIL.MgraClear( MIL.M_DEFAULT, mil_GraphicList );
					//*/

					// デバッグメッセージ表示
					System.Diagnostics.Debug.WriteLine( "[GMF]-------------------------------------" );
					System.Diagnostics.Debug.WriteLine( "[GMF]end of initial setting." );
					System.Diagnostics.Debug.WriteLine( "[GMF]object file name = " + nStrObjectFileName );
					System.Diagnostics.Debug.WriteLine( "[GMF]image size width = " + i_object_width.ToString() + ", height = "+ i_object_height.ToString() );
					System.Diagnostics.Debug.WriteLine( "[GMF]a model context was defined with the model in the displayed image." );
					System.Diagnostics.Debug.WriteLine( "[GMF]start search." );

					// 検索対象画像画像ロード&表示
					MIL.MbufRestore( nStrImageFileName, m_MilSystem, ref mil_Image );
					// 表示の設定
					set_show_display( mil_Image );
					// ベンチマーク測定の為のダミー測定（実アプリでは不要です、念の為コードは残すけど）
					//MIL.MmodFind( mil_SearchContext, mil_Image, mil_Result );
					// ベンチマーク測定用タイマー開始
					MIL.MappTimer( MIL.M_DEFAULT, MIL.M_TIMER_RESET + MIL.M_SYNCHRONOUS, MIL.M_NULL );
					// サーチ開始
					MIL.MmodFind( mil_SearchContext, mil_Image, mil_Result );
					// ベンチマーク測定用タイマー終了
					MIL.MappTimer( MIL.M_DEFAULT, MIL.M_TIMER_READ + MIL.M_SYNCHRONOUS, ref d_time );
					// 測定個数の取得
					MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref i_num_results );
					// デバッグメッセージ表示
					System.Diagnostics.Debug.WriteLine( "[GMF]end search." );
					// 測定結果の取得開始
					if( 0 < i_num_results )
					{
						// 配列確保
						MIL_INT[]	i_model		= new MIL_INT[ i_num_results ];				// Model index.
						double[]	d_score		= new double[ i_num_results ];				// Model correlation score.
						double[]	d_x_pos		= new double[ i_num_results ];				// Model X position.
						double[]	d_y_pos		= new double[ i_num_results ];				// Model Y position. 
						double[]	d_angle		= new double[ i_num_results ];				// Model occurrence angle.
						double[]	d_scale		= new double[ i_num_results ];				// Model occurrence scale.
						// 測定結果の取得実行
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_INDEX + MIL.M_TYPE_MIL_INT, i_model );
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_POSITION_X, d_x_pos );
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_POSITION_Y, d_y_pos );
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_ANGLE, d_angle );
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_SCALE, d_scale );
						MIL.MmodGetResult( mil_Result, MIL.M_DEFAULT, MIL.M_SCORE, d_score );
						// 戻り値設定
						ntagResult				= new tag_GMF_Result[ i_num_results ];
						for( int i_loop = 0; i_loop < i_num_results; i_loop++ )
						{
							ntagResult[ i_loop ].model	= ( int )i_model[ i_loop ];
							ntagResult[ i_loop ].x		= d_x_pos[ i_loop ];
							ntagResult[ i_loop ].y		= d_y_pos[ i_loop ];
							ntagResult[ i_loop ].score	= d_score[ i_loop ] / 100.0;		// 0.00～1.00に正規化
							ntagResult[ i_loop ].angle	= d_angle[ i_loop ];
							ntagResult[ i_loop ].scale	= d_scale[ i_loop ];
						}
						// デバッグメッセージ表示
						str_log							= "time width = " + ( d_time * 1000.0 ).ToString( "F1" ) + "[msec]";
						setLogDevice( str_log );
						setLabel( str_log );
						for( int i_loop = 0; i_loop < i_num_results; i_loop++ )
						{
							string		str_log2		= "result[" + i_loop.ToString( "D2" ) + "].";
							str_log						= str_log2 + "model = " + ntagResult[ i_loop ].model.ToString();
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );

							str_log						= str_log2 + "x = " + ntagResult[ i_loop ].x.ToString( "F2" );
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );

							str_log						= str_log2 + "y = " + ntagResult[ i_loop ].y.ToString( "F2" );
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );

							str_log						= str_log2 + "score = " + ntagResult[ i_loop ].score.ToString( "F2" );
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );

							str_log						= str_log2 + "angle = " + ntagResult[ i_loop ].angle.ToString( "F2" );
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );

							str_log						= str_log2 + "scale = " + ntagResult[ i_loop ].scale.ToString( "F2" );
							setLogDevice( str_log );
							setLabel( str_log );
							System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );
						}
						// 見つかった場所を赤四角で囲む
						for( int i_loop = 0; i_loop < i_num_results; i_loop++ )
						{
							MIL.MgraColor( MIL.M_DEFAULT, d_MODELDRAWCOLOR );
							MIL.MmodDraw( MIL.M_DEFAULT, mil_Result, mil_GraphicList, MIL.M_DRAW_EDGES + MIL.M_DRAW_BOX + MIL.M_DRAW_POSITION, i_loop, MIL.M_DEFAULT );
						}
					}
					// 測定結果が見つからなかった場合
					else
					{
						// デバッグメッセージ表示
						str_log						= "The model was not found or the number of models found is greater than the specified maximum number of occurrence.";
						setLogDevice( str_log );
						setLabel( str_log );
						System.Diagnostics.Debug.WriteLine( "[GMF]" + str_log );
					}
					// ログ残し
					str_log		= "End of geometric model finder.";
					setLogDevice( str_log );
					setLabel( str_log );
					// 終了
					b_ret		= true;
					break;
				}
			}
			catch( System.Exception ex )
			{
				str_log			= "Failed GMF! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret			= false;
			}
			finally
			{
				// Free MIL objects.
				if( MIL.M_NULL != mil_Object )
				{
					MIL.MbufFree( mil_Object );
				}
				if( MIL.M_NULL != mil_Image )
				{
					MIL.MbufFree( mil_Image );
				}
				if( MIL.M_NULL != mil_GraphicList )
				{
					MIL.MgraFree( mil_GraphicList );
				}
				if( MIL.M_NULL != mil_SearchContext )
				{
					MIL.MmodFree( mil_SearchContext );
				}
				if( MIL.M_NULL != mil_Result )
				{
					MIL.MmodFree( mil_Result );
				}
			}

			return		b_ret;
		}
		#endregion
	}
}
