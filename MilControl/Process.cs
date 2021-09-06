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
	/// MILを使用した画像処理実行クラス
	/// </summary>
	/// <remarks>
	///  C++で作られたCMatroxImageProcessの中から必要部分を移植
	/// </remarks>
	public class CProcess : CBase
	{
		#region クラス内定義
		new private const string			m_strDEVICE_NAME		= "MIL.Process";		// デバイス名
		#endregion


		#region ローカル変数
		#endregion


		#region プログラム開始時終了時
		/// <summary>
		/// デストラクタ
		/// </summary>
		~CProcess()
		{
		}


		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CProcess()
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


		#region 回転テーブル中心を求める
		/// <summary>
		/// 回転テーブル中心を求める実行関数
		/// </summary>
		/// <param name="nStrTargetFileName">検索対象画像（グレイ化済み）のフルパスファイル名</param>
		/// <param name="ndCenterX">中心座標 X(結果戻り値)</param>
		/// <param name="ndCenterY">中心座標 Y(結果戻り値)</param>
		/// <param name="niThreshold">2値化用閾値 0から255でなければパラメータの値を使用</param>
		/// <returns>true:成功</returns>
		public bool get_rotate_table_center( string nStrImageFileName, out double ndCenterX, out double ndCenterY, int niThreshold = -1 )
		{
			bool			b_ret				= false;
			string 			str_log;
			MIL_INT			i_object_width, i_object_height;				// 画像サイズ
			MIL_INT			i_center_x, i_center_y;							// 画像中心
			MIL_ID			mil_Image			= MIL.M_NULL;				// 検索対象画像ID
			MIL_ID			mil_BlobImage		= MIL.M_NULL;				// ブロブ画像ID
			MIL_ID			mil_BlobResult		= MIL.M_NULL;				// ブロブ解析結果画像ID
			MIL_ID			mil_BlobFeature		= MIL.M_NULL;				// ブロブ解析特微量リストバッファ画像ID

			// 結果初期化
			ndCenterX					= 0;
			ndCenterY					= 0;
			// 2値化閾値の設定
			if( 0 > niThreshold || niThreshold > 255 )
			{
				// 0から255でなければパラメータの値を使用
				niThreshold				= m_cParaMilControl.FrtcBinarizeThreshold;
			}

			// 開始
			try
			{
				while( true )
				{
					// ログ残し
					str_log			= "Start of find rotate table center.";
					setLogDevice( str_log );
					setLabel( str_log );

					// 2値化
					{
						// 検索対象画像画像ロード&表示
						MIL.MbufRestore( nStrImageFileName, m_MilSystem, ref mil_Image );
						// 画像サイズ取得
						i_object_width		= MIL.MbufInquire( mil_Image, MIL.M_SIZE_X, MIL.M_NULL );
						i_object_height		= MIL.MbufInquire( mil_Image, MIL.M_SIZE_Y, MIL.M_NULL );
						// 画像中心取得
						i_center_x			= i_object_width / 2;
						i_center_y			= i_object_height / 2;
						// メディアンフィルタでノイズ除去
						MIL.MimRank( mil_Image, mil_Image, MIL.M_3X3_RECT, MIL.M_MEDIAN, MIL.M_GRAYSCALE );
						// 2値化 パラメータの閾値を使用
						MIL.MimBinarize( mil_Image, mil_Image, MIL.M_GREATER_OR_EQUAL, niThreshold, MIL.M_NULL );
						// 表示の設定
						set_show_display( mil_Image );
					}

					// ブロブ解析
					{
						// ブロブ解析結果格納バッファを確保する
						MIL.MblobAllocResult( m_MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mil_BlobResult );
						// ブロブ解析特微量リストバッファを確保する
						MIL.MblobAlloc( m_MilSystem, MIL.M_DEFAULT, MIL.M_DEFAULT, ref mil_BlobFeature );

						// 全ての特徴を設定する
						MIL.MblobControl( mil_BlobFeature, MIL.M_ALL_FEATURES, MIL.M_ENABLE );
						// M_BOXを有効にする(上でM_ALL_FEATURESを指定しているので不要なんだけど念の為)
						MIL.MblobControl( mil_BlobFeature, MIL.M_BOX, MIL.M_ENABLE );
						// ブロブ解析をバイナリモードにする(高速化)
						MIL.MblobControl( mil_BlobResult, MIL.M_IDENTIFIER_TYPE, MIL.M_BINARY );
						// 全ての特徴を設定する
						//MIL.MblobControl( mil_BlobResult, MIL.M_ALL_FEATURES, MIL.M_ENABLE );
						// ブロブ穴埋め。テーブル上の物体を無視するため
						MIL.MblobReconstruct( mil_Image, MIL.M_NULL, mil_Image, MIL.M_FILL_HOLES, MIL.M_BINARY );

						// ブロブ解析を実行
						MIL.MblobCalculate( mil_BlobFeature, mil_Image, MIL.M_NULL, mil_BlobResult );
						// 最小特徴サイズに満たないブロブは削除する。
						MIL.MblobSelect( mil_BlobResult, MIL.M_DELETE, MIL.M_AREA, MIL.M_LESS_OR_EQUAL, m_cParaMilControl.FrtcMinimumFeatureSize, MIL.M_NULL );
						// 表示の設定
						set_show_display( mil_Image );
					}

					// 解析されたブロブの数を取得する
					{
						MIL_INT		i_blob_num	= 0;
						//MIL.MblobGetNumber( mil_BlobResult, ref i_blob_num );
						MIL.MblobGetResult( mil_BlobResult, MIL.M_DEFAULT, MIL.M_NUMBER + MIL.M_TYPE_MIL_INT, ref i_blob_num );
						bool		b_find		= false;
						// ブロブが1つ見つかった場合
						if( 1 == i_blob_num )
						{
							b_find				= true;
						}
						// ブロブが複数見つかった場合はセンター寄りの画像を使用
						else if( 1 < i_blob_num )
						{
							// ボックス座標取得
							MIL_INT []	i_box_min_x		= new MIL_INT[ i_blob_num ];
							MIL_INT []	i_box_max_x		= new MIL_INT[ i_blob_num ];
							MIL_INT []	i_box_min_y		= new MIL_INT[ i_blob_num ];
							MIL_INT []	i_box_max_y		= new MIL_INT[ i_blob_num ];
							MIL.MblobGetResult( mil_BlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MIN + MIL.M_TYPE_MIL_INT, ref i_box_min_x[ 0 ] );
							MIL.MblobGetResult( mil_BlobResult, MIL.M_DEFAULT, MIL.M_BOX_X_MAX + MIL.M_TYPE_MIL_INT, ref i_box_max_x[ 0 ] );
							MIL.MblobGetResult( mil_BlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MIN + MIL.M_TYPE_MIL_INT, ref i_box_min_y[ 0 ] );
							MIL.MblobGetResult( mil_BlobResult, MIL.M_DEFAULT, MIL.M_BOX_Y_MAX + MIL.M_TYPE_MIL_INT, ref i_box_max_y[ 0 ] );
							for( int i_loop = 0; i_loop < i_blob_num; i_loop++ )
							{
								if( i_box_min_x[ i_loop ] <= i_center_x && i_center_x <= i_box_max_x[ i_loop ] &&
									i_box_min_y[ i_loop ] <= i_center_y && i_center_y <= i_box_max_y[ i_loop ] )
								{
									b_find		= true;
									break;
								}
							}
						}
						// ブロブが見つからない、または複数見つかった中から候補が無い
						if( false == b_find )
						{
							// ログ残し
							str_log				= "Failed not find rotate table(" + i_blob_num.ToString() + ").";
							setLogDevice( str_log );
							setLabel( str_log );
							// 終了
							b_ret				= false;
							break;
						}
					}
					// 輪郭線の描画
					{
						// 各ブロブのラベルを取得
						MIL_INT		i_label		= 0;
						// 各ブロブのうち、テーブルのブロブを取り出す
						// これはブロブ面積が一番大きいものとか、画面の中央座標を含むものとか色々ある
						// ここでは画面の中央座標を含むブロブをテーブルのブロブとする。
						// テーブルがそんな端っこに動くことはないと思うので
						MIL.MblobGetLabel( mil_BlobResult, i_center_x, i_center_y, ref i_label );
						// ブロブ画像バッファ確保
						MIL.MbufAlloc2d( m_MilSystem, i_object_width, i_object_height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_BlobImage );
						// 表示の設定
						// テーブルのブロブだけが描画されている画像作成
						MIL.MbufClear( mil_BlobImage, 0 );
						MIL.MblobSelect( mil_BlobResult, MIL.M_INCLUDE_ONLY, MIL.M_LABEL_VALUE, MIL.M_EQUAL, i_label, MIL.M_NULL );
						MIL.MblobDraw( MIL.M_DEFAULT, mil_BlobResult, mil_BlobImage, MIL.M_DRAW_BLOBS, MIL.M_INCLUDED_BLOBS, MIL.M_DEFAULT );
						// ソベルフィルタでテーブル形状の輪郭抽出
						MIL.MimConvolve( mil_BlobImage, mil_BlobImage, MIL.M_EDGE_DETECT_SOBEL_FAST );
						// 2値化(必要性が疑わしいが念のため残す、必要であれば77をパラメータ化)
						//MIL.MimBinarize( mil_BlobImage, mil_BlobImage, MIL.M_GREATER_OR_EQUAL, 77, MIL.M_NULL );
						// 細線化し太さ1の輪郭にする
						MIL.MimThin( mil_BlobImage, mil_BlobImage, MIL.M_TO_SKELETON, MIL.M_BINARY );
						// 表示の為コピー
						MIL.MbufCopy( mil_BlobImage, mil_Image );
						MIL.MbufExport( "circle.bmp", MIL.M_BMP, mil_Image );

                    }
					// 最小二乗法用データ設定
					int				i_array		= 0;
					double[]		d_edge_x	= new double[ 0 ];
					double[]		d_edge_y	= new double[ 0 ];
					{
						// 輪郭を構成する点群の座標を取得
						for( int i_loop_y = 0; i_loop_y < i_object_height; i_loop_y++ )
						{
							// 間引き(最初の50個は使う、後は間引く)
							if( 50 < i_array && 0 != i_loop_y % 5 )
							{
								continue;
							}
							// X方向を確認
							Parallel.For( 0, i_object_width, i_loop_x =>
							{
								byte[] 		by_value	= new byte[ 1 ];
								MIL.MbufGet2d( mil_BlobImage, i_loop_x, i_loop_y, 1, 1, by_value );
								// 輪郭があった
								if( 0 != by_value[ 0 ] )
								{
									lock( System.Threading.Thread.CurrentContext )
									{
										Array.Resize( ref d_edge_x, i_array + 1 );
										Array.Resize( ref d_edge_y, i_array + 1 );
										d_edge_x[ i_array ]		= ( double )i_loop_x;
										d_edge_y[ i_array ]		= ( double )i_loop_y;
										i_array++;
									}
								}
							} );
						}
					}
					// 輪郭の座標から最小二乗法により近似円を算出（中心座標、半径取得）
					double			d_center_x	= 0;
					double			d_center_y	= 0;
					double			d_radius	= 0;
					if( 0 < i_array )
					{
						if( true == CGlobalMath.CircleApproxByLeastSquaresMethod( d_edge_x, d_edge_y, i_array, out d_center_x, out d_center_y, out d_radius ) )
						{
							// ログ残し
							str_log				= "End of find rotate table center. (";
							str_log				+= d_center_x.ToString( "F3" );
							str_log				+= ", ";
							str_log				+= d_center_y.ToString( "F3" );
							str_log				+= ")";
							// 終了
							b_ret				= true;
						}
						else
						{
							// ログ残し
							str_log				= "Failed find rotate table center! (no circle2)";
						}
						ndCenterX				= d_center_x;
						ndCenterY				= d_center_y;
					}
					else
					{
						// ログ残し
						str_log					= "Failed find rotate table center! (no circle1)";
					}
					setLogDevice( str_log );
					setLabel( str_log );
					break;
				}
			}
			catch( System.Exception ex )
			{
				str_log					= "Failed find rotate table center! " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
				setLogError( str_log );
				b_ret					= false;
			}
			finally
			{
				// Free MIL objects.
				if( MIL.M_NULL != mil_Image )
				{
					MIL.MbufFree( mil_Image );
				}
				if( MIL.M_NULL != mil_BlobImage )
				{
					MIL.MbufFree( mil_BlobImage );
				}
				if( MIL.M_NULL != mil_BlobResult )
				{
					MIL.MblobFree( mil_BlobResult );
				}
				if( MIL.M_NULL != mil_BlobFeature )
				{
					MIL.MblobFree( mil_BlobFeature );
				}
			}

			return		b_ret;
		}
		#endregion
	}
}
