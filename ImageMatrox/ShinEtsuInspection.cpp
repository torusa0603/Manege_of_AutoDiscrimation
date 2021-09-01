#include "stdafx.h"
#include "ShinEtsuInspection.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>

#define DEBUG_OUTPUT_HEATMAP_FILES				// for heatmap
#pragma warning(disable : 4995)

#define PI (atan(1.0)*4.0)

CShinEtsuInspection::CShinEtsuInspection()
{
}


CShinEtsuInspection::~CShinEtsuInspection()
{
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		信越化学工業検査アルゴリズム(リリース版)

	2.パラメタ説明
		nstrInputImage	(IN)	検査画像ファイルパス
		nstrResultImage (OUT)	結果画像ファイルパス
		ndMinDefectSize (IN)	最小欠けサイズ(pixel)
		npiNGContent	(OUT)	NG種別 1:傷、2:欠け 3:傷&欠け
		npiCurve		(OUT)	カーブ有無(0:カーブ無し　1:カーブ有)



	3.概要
		終了処理

	4.機能説明
		終了処理

	5.戻り値
		0:	OK
		1:	NG
		-1: 検査失敗(エッジが見つからない)
		-2: 検査失敗(エッジと重なるブロブがない)
		-3: 検査失敗(膨張、収縮の差分画像と重なるエッジがない)

	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CShinEtsuInspection::execInspection(string nstrInputImage, string nstrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve)
{
	const int ci_min_blob_num = 100;	//	ブロブ解析時、この値以下のブロブは無視する

	MIL_ID	mil_inspection_image;
	MIL_ID  mil_result_image;
	MIL_ID	mil_inspection_image_binary;
	MIL_ID  mil_edge_image;
	MIL_ID	mil_blob_image;
	MIL_ID	mil_diff_image;
	MIL_ID	mil_temp_image;
	MIL_ID mil_blob_result;
	MIL_ID mil_blob_feature_list;
	MIL_ID mil_edge;
	MIL_ID mil_edge_result;
	MIL_ID	mil_stat_result;
	MIL_INT  l_num_max = 0;
	MIL_INT	i_edge_found_num = 0;
	MIL_INT l_blob_num;
	MIL_ID mil_average_image;
	int		i_loop;
	int		i_loop2;
	int		i_ret = 0;
	SIZE sz_image_size;
	bool	b_crack_detected = false;	//	欠けを検出
	bool	b_cut_detected = false;		//	傷を検出
	bool	b_curve = false;			//	曲線を検出

	vector< int > vec_exclude_point_index;
	vector< XYty > vec_final_edge_data;
	vector< double > vec_angle;
	vector< double > vec_angle_new;

	vector < LineInfo > vec_line_info;
	vector< int > vec_same_line_index;
	vector< vector< CrackInfo > > vec_all_crack_info;
	vector< CrackInfo > vec_crack_info;
	vector< XYty > vec_edge_data_temp;

	MimAllocResult(m_milSys, M_DEFAULT, M_STAT_LIST, &mil_stat_result);

	//	画像をファイルからロードする
	MbufRestore(nstrInputImage.c_str(), m_milSys, &mil_inspection_image);
	//	画像サイズ取得
	sz_image_size.cx = (long)MbufInquire(mil_inspection_image, M_SIZE_X, M_NULL);
	sz_image_size.cy = (long)MbufInquire(mil_inspection_image, M_SIZE_Y, M_NULL);

	//	二値化用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_inspection_image_binary);
	//	エッジ画像用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_edge_image);
	//	ブロブ画像用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_blob_image);
	//	検査対象ブロブの膨張と縮小の差分画像の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_diff_image);
	//	作業用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_temp_image);
	//	平均画像用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);
	//	結果画像用の画像バッファを確保
	MbufAllocColor(m_milSys, 3, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_result_image);
	//	結果画像バッファに検査画像をコピーしておく
	MbufCopyColor(mil_inspection_image, mil_result_image, M_ALL_BANDS);

	// ************************
	//
	//	メディアンフィルタでノイズを除去しておく
	//
	// ************************

	MimRank(mil_inspection_image, mil_inspection_image, M_3X3_RECT, M_MEDIAN, M_GRAYSCALE);

	// ************************
	//
	//	二値化
	//
	// ************************

	//	二値化を実行。閾値は自動で
	MimBinarize(mil_inspection_image, mil_inspection_image_binary, M_BIMODAL + M_GREATER, M_NULL, M_NULL);

	// ************************
	//
	//	ブロブ解析
	//
	// ************************

	//　ブロブ解析結果格納バッファを確保する
	MblobAllocResult(m_milSys, &mil_blob_result);
	//　ブロブ解析特微量リストバッファを確保する
	MblobAllocFeatureList(m_milSys, &mil_blob_feature_list);

	//	全ての特徴を設定する
	MblobSelectFeature(mil_blob_feature_list, M_ALL_FEATURES);
	//	ブロブ解析をバイナリモードにする(高速化)
	MblobControl(mil_blob_result, M_IDENTIFIER_TYPE, M_BINARY);
	//	ブロブ解析を実行
	MblobCalculate(mil_inspection_image_binary, M_NULL, mil_blob_feature_list, mil_blob_result);
	//	最小特徴サイズに満たないブロブは削除する。
	//	基準未満のブロブを測定結果から削除
	MblobSelect(mil_blob_result, M_DELETE, M_AREA, M_LESS_OR_EQUAL, ci_min_blob_num, M_NULL);
	//	解析されたブロブの数を取得する
	MblobGetNumber(mil_blob_result, &l_blob_num);

	// ************************
	//
	//	エッジ検出
	//
	// ************************


	//	エッジファインダーコンテキストを確保
	MedgeAlloc(m_milSys, M_CONTOUR, M_DEFAULT, &mil_edge);
	MedgeAllocResult(m_milSys, M_DEFAULT, &mil_edge_result);

	//	smoothnessを設定
	MedgeControl(mil_edge, M_FILTER_SMOOTHNESS, 50.0);
	//	閾値をvery highに
	MedgeControl(mil_edge, M_THRESHOLD_MODE, M_VERY_HIGH);
	//	エッジ間のギャップを埋める 20ピクセル
	MedgeControl(mil_edge, M_FILL_GAP_DISTANCE, 20);

	//	エッジ検出実行
	MedgeCalculate(mil_edge, mil_inspection_image, M_NULL, M_NULL, M_NULL, mil_edge_result, M_DEFAULT);

	//	エッジ数を取得
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_NUMBER_OF_CHAINS + M_TYPE_MIL_INT, &i_edge_found_num, M_NULL);
	//	エッジ画像を作成する
	if (i_edge_found_num > 0)
	{
		MgraColor(m_milGraphic, M_COLOR_WHITE);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);
	}
	//	エッジが一つもなければ検査失敗
	else
	{
		i_ret = -1;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}

	// ************************
	//
	//	ブロブの中から検査対象のパターンのブロブを抽出する
	//
	// ************************

	MIL_DOUBLE *d_label;
	MIL_INT  l_num;
	int	i_max_blob_index = -1;

	//	各ブロブのインデックスを取得
	d_label = new MIL_DOUBLE[(int)l_blob_num];
	MblobGetResult(mil_blob_result, M_LABEL_VALUE, d_label);

	//	各ブロブごとにチェックしていく
	for (i_loop = 0; i_loop < (int)l_blob_num; i_loop++)
	{
		//	注目しているブロブだけを取り出してブロブ画像を作る。
		MbufClear(mil_blob_image, 0);
		MblobSelect(mil_blob_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, d_label[i_loop], M_NULL);
		MblobFill(mil_blob_result, mil_blob_image, M_INCLUDED_BLOBS, 255);

		//	ブロブを少し膨張させる
		MimDilate(mil_blob_image, mil_blob_image, 2, M_GRAYSCALE);

		//	ブロブとエッジ画像の重なったところだけを抽出
		MimArith(mil_blob_image, mil_edge_image, mil_blob_image, M_AND);
		//	重なったところの画素数をカウント
		MimStat(mil_blob_image, mil_stat_result, M_NUMBER, M_GREATER, 1, M_NULL);
		MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_MIL_INT, &l_num);
		//	最大の数となるブロブのインデックスを記憶
		if (l_num > l_num_max)
		{
			l_num_max = l_num;
			i_max_blob_index = (int)d_label[i_loop];
		}

	}
	delete[] d_label;

	//	もしエッジと重なるものがなければエラーで返す
	if (i_max_blob_index == -1)
	{
		i_ret = -2;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}
	//	重なってるものがあれば、そのブロブを検査対象パターンとする
	else
	{
		MbufClear(mil_blob_image, 0);
		MblobSelect(mil_blob_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_blob_index, M_NULL);
		MblobFill(mil_blob_result, mil_blob_image, M_INCLUDED_BLOBS, 255);
	}

	// ************************
	//
	//	検査対象ブロブを膨張したものと、縮小したものの差分画像を作成する
	//
	// ************************

	int i_boutyou_num = 8;

	//	膨張画像作成
	MimDilate(mil_blob_image, mil_diff_image, i_boutyou_num, M_GRAYSCALE);
	//	縮小画像作成
	MimErode(mil_blob_image, mil_temp_image, i_boutyou_num, M_GRAYSCALE);
	//	膨張-差分画像作成
	MimArith(mil_diff_image, mil_temp_image, mil_diff_image, M_SUB_ABS + M_SATURATION);

	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_0DiffImage.bmp";
		MbufExport(str_file.c_str(), M_BMP, mil_diff_image);
	}


	// ************************
	//
	//	差分画像内に含まれるエッジ(チェーン)のうち、含まれるピクセル数が最大となるエッジ（チェーン)を一つ抽出する
	//
	// ************************

	int	i_max_edge_index = -1;
	MIL_DOUBLE *d_edge_label;
	d_edge_label = new MIL_DOUBLE[(int)i_edge_found_num];
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_LABEL_VALUE, d_edge_label, M_NULL);

	l_num_max = 0;
	//	各ブエッジごとにチェックしていく
	for (i_loop = 0; i_loop < (int)i_edge_found_num; i_loop++)
	{

		//	注目しているエッジだけを取り出してエッジ画像を作る。
		MbufClear(mil_edge_image, 0);
		MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, d_edge_label[i_loop], M_NULL);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);

		//	差分画像とエッジ画像の重なったところだけを抽出
		MimArith(mil_diff_image, mil_edge_image, mil_edge_image, M_AND);

		//	重なったところの画素数をカウント
		MimStat(mil_edge_image, mil_stat_result, M_NUMBER, M_GREATER, 1, M_NULL);
		MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_MIL_INT, &l_num);
		//	最大の数となるエッジのインデックスを記憶
		if (l_num > l_num_max)
		{
			l_num_max = l_num;
			i_max_edge_index = (int)d_edge_label[i_loop];
		}
	}
	delete[] d_edge_label;

	//	もしエッジと重なるものがなければエラーで返す
	if (i_max_edge_index == -1)
	{
		i_ret = -3;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}

	if (m_bDebugON == true)
	{
		MbufClear(mil_edge_image, 0);
		MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_edge_index, M_NULL);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);

		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_1EdgeChain.bmp";
		MbufExport(str_file.c_str(), M_BMP, mil_edge_image);
	}


	// ************************
	//
	//	抽出したエッジの点群を取得し、差分画像からはみ出た点群を記憶する
	//
	// ************************

	MIL_DOUBLE d_edge_chain_num[1];


	//	抽出したエッジを選択
	MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_edge_index, M_NULL);
	//	エッジのポイント数を取得
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_NUMBER_OF_CHAINED_EDGELS, d_edge_chain_num, M_NULL);

	//	エッジ点群を取得
	MIL_DOUBLE *d_edge_chain_X;
	MIL_DOUBLE *d_edge_chain_Y;
	d_edge_chain_X = new MIL_DOUBLE[(int)d_edge_chain_num[0]];
	d_edge_chain_Y = new MIL_DOUBLE[(int)d_edge_chain_num[0]];
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_CHAIN, d_edge_chain_X, d_edge_chain_Y);

	BYTE bt_pixel_value[1];
	//	エッジ点群の中で差分画像からはみ出た点のインデックスを求める
	for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
	{
		//	差分画像上のエッジ座標の輝度値を取得し、それが255でなければはみ出ていると判断
		MbufGet2d(mil_diff_image, (MIL_INT)d_edge_chain_X[i_loop], (MIL_INT)d_edge_chain_Y[i_loop], 1, 1, bt_pixel_value);
		if (bt_pixel_value[0] != 255)
		{
			vec_exclude_point_index.push_back(i_loop);
		}
	}

	// ************************
	//
	//	エッジの点群から、はみ出た点群を除去して、新たな点群配列を作成する
	//
	// ************************

	XYty xy_data;

	//	はみ出た点が存在する
	if (vec_exclude_point_index.size() != 0)
	{
		for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
		{
			for (i_loop2 = 0; i_loop2 < (int)vec_exclude_point_index.size(); i_loop2++)
			{
				//	はみ出たデータである
				if (vec_exclude_point_index[i_loop2] == i_loop)
				{
					break;
				}
			}
			//	はみ出ていなければ配列に格納していく
			if (i_loop2 == (int)vec_exclude_point_index.size())
			{
				xy_data.dX = d_edge_chain_X[i_loop];
				xy_data.dY = d_edge_chain_Y[i_loop];
				vec_final_edge_data.push_back(xy_data);
			}
		}
	}
	//	はみ出た点が存在しない
	else
	{
		for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
		{
			xy_data.dX = d_edge_chain_X[i_loop];
			xy_data.dY = d_edge_chain_Y[i_loop];
			vec_final_edge_data.push_back(xy_data);

		}
	}

	//	このエッジの点群配列を逆さまに入れ替える。(水平方向のエッジのXが大きいほうから順に入っているから、小さい順に配列に入れるようにする)
	for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
	{
		vec_edge_data_temp.push_back(vec_final_edge_data[vec_final_edge_data.size() - i_loop - 1]);
	}
	vec_final_edge_data = vec_edge_data_temp;


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_2EdgeChainExclude.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
		{
			sprintf_s(chr_buff, "%f,%f", vec_final_edge_data[i_loop].dX, vec_final_edge_data[i_loop].dY);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}


	// ************************
	//
	//	エッジ点において、隣接するエッジ点との角度を求める
	//
	// ************************

	int i_average_num = 3;	//	前3点の平均、後ろ3点の平均の計2点から角度を求める
	XYty xy_data2;
	double d_angle;
	for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
	{
		if (i_loop >= i_average_num && i_loop + i_average_num < (int)vec_final_edge_data.size())
		{
			xy_data.dX = xy_data.dY = xy_data2.dX = xy_data2.dY = 0.0;
			//	前N点の平均
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				xy_data.dX += vec_final_edge_data[i_loop - i_loop2 - 1].dX;
				xy_data.dY += vec_final_edge_data[i_loop - i_loop2 - 1].dY;
			}
			xy_data.dX /= (double)i_average_num;
			xy_data.dY /= (double)i_average_num;
			//	後ろN点の平均
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				xy_data2.dX += vec_final_edge_data[i_loop + i_loop2 + 1].dX;
				xy_data2.dY += vec_final_edge_data[i_loop + i_loop2 + 1].dY;
			}
			xy_data2.dX /= (double)i_average_num;
			xy_data2.dY /= (double)i_average_num;

			//	角度を求める(-180～180°)となる
			d_angle = atan2(xy_data.dY - xy_data2.dY, xy_data.dX - xy_data2.dX);
			vec_angle.push_back(d_angle * 180.0 / PI);
		}
	}
	//	このままだと、角度の配列(vector)には前後3点分のデータが入ってないので、隣の角度で埋めてしまう
	{
		d_angle = vec_angle[0];
		auto it = vec_angle.begin();
		for (i_loop = 0; i_loop < i_average_num; i_loop++)
		{
			it = vec_angle.insert(it, d_angle);
		}
		d_angle = vec_angle.back();
		for (i_loop = 0; i_loop < i_average_num; i_loop++)
		{
			vec_angle.push_back(d_angle);
		}
	}

	//	180°と-180°の境界線付近は実際は1,2°しかずれてなくても179、-179とか358°とかずれるので
	//	連続させる
	//	隣接する角度が180°以上離れてたら360°回転して補正する
	for (i_loop = 0; i_loop < (int)vec_angle.size() - 1; i_loop++)
	{
		//	今の点と次の点で、引き算を行い、その結果が180°以上であれば(前の点が+、次の点が-)、次の点に360°加算
		//	結果が-180°以下であれば(前の点が-、次の点が+)、次の点から360°減算
		if (vec_angle[i_loop] - vec_angle[i_loop + 1] > 180.0)
		{
			vec_angle[i_loop + 1] = vec_angle[i_loop + 1] + 360.0;
		}
		else if (vec_angle[i_loop] - vec_angle[i_loop + 1] < -180.0)
		{
			vec_angle[i_loop + 1] = vec_angle[i_loop + 1] - 360.0;
		}
	}

	// ************************
	//
	//	求めたエッジ角度配列を平均化しノイズを取る
	//
	// ************************

	i_average_num = 5;		//	自分自身のデータを中心に平均したいので奇数にする

	for (i_loop = 0; i_loop < (int)vec_angle.size(); i_loop++)
	{
		//	データ端の平均化出来ないところはそのままにしておく

		if (i_loop >= i_average_num / 2 && i_loop + (i_average_num / 2) < (int)vec_angle.size())
		{
			d_angle = 0.0;
			//	データ端でないので平均
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				d_angle = d_angle + vec_angle[i_loop - (i_average_num / 2) + i_loop2];
			}
			vec_angle_new.push_back(d_angle / (double)i_average_num);
		}
		else
		{
			vec_angle_new.push_back(vec_angle[i_loop]);
		}
	}
	vec_angle = vec_angle_new;

	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_3Angle.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_angle.size(); i_loop++)
		{
			sprintf_s(chr_buff, "%f", vec_angle[i_loop]);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}

	// ************************
	//
	//	先頭から順に角度を見ていき、同じ角度が100個連続で続いたものを直線とする
	//
	// ************************
	bool b_line_start = false;
	LineInfo line_info;
	const double cd_same_line_angle_threshold = 5.0;	//前後の角度ずれが5°以内なら直線とみなす
	const int ci_line_length_threshold = 100;
	double d_line_start_angle = 0.0;

	line_info.i_start_index = 0;

	for (i_loop = 0; i_loop < (int)vec_angle.size() - 1; i_loop++)
	{
		//	直線の始点が定まってる場合
		if (b_line_start == true)
		{
			//	開始角度と現在の角度の差分を見る
			d_angle = fabs(vec_angle[i_loop] - d_line_start_angle);
		}
		//	直線の始点がまだ定まっていない場合
		else
		{
			//	隣接する2点の角度の差を求める
			d_angle = vec_angle[i_loop + 1] - vec_angle[i_loop];
		}



		//	直線である
		if (fabs(d_angle) < cd_same_line_angle_threshold)
		{
			//	まだ直線の始点を定めていなければここで定める
			if (b_line_start == false)
			{
				b_line_start = true;
				line_info.i_start_index = i_loop;
				d_line_start_angle = (vec_angle[i_loop + 1] + vec_angle[i_loop]) / 2.0;
			}
		}
		//	角度の差が大きい(直線でない)
		else
		{
			//	直線の始点は設定された状態で、なおかつ十分に直線が長ければ、ここで直線の終点を確定させ、直線を登録する
			if (b_line_start == true && i_loop - line_info.i_start_index > ci_line_length_threshold)
			{
				line_info.i_end_index = i_loop;
				//	この直線の平均角度を求める
				d_angle = 0.0;
				for (i_loop2 = line_info.i_start_index; i_loop2 <= line_info.i_end_index; i_loop2++)
				{
					d_angle += vec_angle[i_loop2];
				}
				line_info.d_average_angle = d_angle / (double)(line_info.i_end_index - line_info.i_start_index);
				line_info.i_line_ID = (int)vec_line_info.size();
				//	直線の登録
				vec_line_info.push_back(line_info);
			}
			//	始点決定フラグを下げる
			b_line_start = false;
		}

	}
	//	最後の直線を判断する
	if (b_line_start == true && i_loop - line_info.i_start_index > ci_line_length_threshold)
	{
		line_info.i_end_index = i_loop;
		//	この直線の平均角度を求める
		d_angle = 0.0;
		for (i_loop2 = line_info.i_start_index; i_loop2 <= line_info.i_end_index; i_loop2++)
		{
			d_angle += vec_angle[i_loop2];
		}
		line_info.d_average_angle = d_angle / (double)(line_info.i_end_index - line_info.i_start_index);
		line_info.i_line_ID = (int)vec_line_info.size();
		//	直線の登録
		vec_line_info.push_back(line_info);
	}

	//	直線が一つもない、もしくは1つしかなくその長さもエッジ長さの半分以下であれば、大きな欠けが存在するとしてしまう
	if (vec_line_info.size() == 0)
	{
		//	欠け検出フラグを立てる
		b_crack_detected = true;
	}
	else if (vec_line_info.size() == 1)
	{
		if ((vec_line_info[0].i_end_index - vec_line_info[0].i_start_index) < (int)(vec_angle.size() * 0.5))
		{
			//	欠け検出フラグを立てる
			b_crack_detected = true;
		}
	}


	// ************************
	//
	//	求めた直線のうち、隣接する直線で同一直線上にあるものがあればそれを連結する
	//
	// ************************

	int i_line_length;
	double *d_x;
	double *d_y;
	double d_coeff[2];
	double d_distance;

	CGlobalMath c_global_math;
	const double cd_same_angle_threshold = 2.0;			//	°
	const double cd_same_line_point_threshold = 2.0;  //	ピクセル座標

	//	直線が2本以上あれば行う
	if ((int)vec_line_info.size() >= 2)
	{
		for (i_loop = 0; i_loop < (int)vec_line_info.size() - 1; i_loop++)
		{
			//	N本目の直線とN+1本目の直線が角度が異なれば、同一直線上にいることはありえないので
			//	N本目はそのまま登録
			if (fabs(vec_line_info[i_loop].d_average_angle - vec_line_info[i_loop + 1].d_average_angle) > cd_same_angle_threshold)
			{
				continue;
			}

			//	同一角度なら同一直線上にいる可能性があるのでチェック

#if 0
			XYty one_data;
			//	N本目の直線のxyを取得
			i_line_length = vec_line_info[i_loop].i_end_index - vec_line_info[i_loop].i_start_index + 1;
			d_x = new double[i_line_length];
			d_y = new double[i_line_length];

			for (i_loop2 = 0; i_loop2 < i_line_length; i_loop2++)
			{
				d_x[i_loop2] = vec_final_edge_data[vec_line_info[i_loop].i_start_index + i_loop2].dX;
				d_y[i_loop2] = vec_final_edge_data[vec_line_info[i_loop].i_start_index + i_loop2].dY;
			}
			//	この直線上の点群から回帰直線を求める
			c_global_math.ApproximationByLeastSquaresMethod(d_x, d_y, i_line_length, 1, d_coeff);

			//	N+1本目の直線の1点目と、この直線との距離を求める

			one_data.dX = vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index].dX;
			one_data.dY = vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index].dY;
			//	点と直線の距離の公式
			d_distance = fabs(d_coeff[0] * one_data.dX - one_data.dY + d_coeff[1]) / sqrt(d_coeff[0] * d_coeff[0] + 1);
			//	点が直線上にいれば、N+1本目の直線もN本目の直線であり、その間には欠けがある可能性があるということが分かる
			//	N+1本目の直線のIDをN本目のIDと同一にする
			if (d_distance < cd_same_line_point_threshold)
			{
				vec_line_info[i_loop + 1].i_line_ID = vec_line_info[i_loop].i_line_ID;
			}
			delete[] d_x;
			delete[] d_y;
#else
			double d_x_first_ave = 0.0;
			double d_y_first_ave = 0.0;
			double d_x_second_ave = 0.0;
			double d_y_second_ave = 0.0;
			const double cd_same_angle_range = 10.0;

			//N本目の終点の3点の平均座標を求める
			for (i_loop2 = 0; i_loop2 < 3; i_loop2++)
			{
				d_x_first_ave += vec_final_edge_data[vec_line_info[i_loop].i_end_index + i_loop2 - 3].dX;
				d_y_first_ave += vec_final_edge_data[vec_line_info[i_loop].i_end_index + i_loop2 - 3].dY;
			}
			d_x_first_ave /= 3.0;
			d_y_first_ave /= 3.0;
			//	N+1本目の始点の3点の平均座標を求める
			for (i_loop2 = 0; i_loop2 < 3; i_loop2++)
			{
				d_x_second_ave += vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index + i_loop2].dX;
				d_y_second_ave += vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index + i_loop2].dY;
			}
			d_x_second_ave /= 3.0;
			d_y_second_ave /= 3.0;

			//	この2点がなす角度がNもしくはN+1の角度と同じようであれば同一直線上とし、その間に欠けがある可能性がある
			d_angle = atan2(d_y_first_ave - d_y_second_ave, d_x_first_ave - d_x_second_ave);
			d_angle = d_angle * 180.0 / PI;

			//	角度を連続させる
			if (d_angle - vec_line_info[i_loop].d_average_angle > 180.0)
			{
				d_angle = d_angle - 360.0;
			}
			else if (d_angle - vec_line_info[i_loop].d_average_angle < -180.0)
			{
				d_angle = d_angle + 360.0;
			}

			if (fabs(d_angle - vec_line_info[i_loop].d_average_angle) < cd_same_angle_range)
			{
				vec_line_info[i_loop + 1].i_line_ID = vec_line_info[i_loop].i_line_ID;
			}
#endif

		}
	}


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_4LineInfo.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			sprintf_s(chr_buff, "Line%d\nStartIndex,%d\nEndIndex,%d\nAverageAngle,%f\nLineID,%d", i_loop, vec_line_info[i_loop].i_start_index, vec_line_info[i_loop].i_end_index,
				vec_line_info[i_loop].d_average_angle, vec_line_info[i_loop].i_line_ID);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}

	// ************************
	//
	//	曲線が存在するか判定する
	//
	// ************************

	//	直線が1本しかない場合は曲線は存在しない
	if ((int)vec_line_info.size() < 2)
	{
		b_curve = false;
	}
	else
	{
		//	1本目の直線のIDと異なるものがあった時点で曲線が存在すると判断する
		int i_ID = vec_line_info[0].i_line_ID;
		for (i_loop = 1; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			if (vec_line_info[i_loop].i_line_ID != i_ID)
			{
				b_curve = true;
				break;
			}
		}
	}

	// ************************
	//
	//	各直線で欠けを検出する(判定はまだしない)
	//
	// ************************

	CrackInfo crack_info;

	const double cd_min_angle = 80.0;		//	Y軸に平行な直線とみなす角度1
	const double cd_max_angle = 100.0;		//	Y軸に平行な直線とみなす角度2    角度1 < abs(θ) < 角度2  であればY軸に平行な直線とする。

	//	欠けは連結された直線の間にあるので、まず連結された直線を求める

	//	直線が2本以上あれば行う。直線1本以下なら欠けはない
	if ((int)vec_line_info.size() >= 2)
	{
		//	最初に求めた直線の本数でループさせるが、IDをループさせている意味。ID番号は直線の本数より大きい数字にはならない
		for (i_loop = 0; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			vec_same_line_index.clear();
			//	IDが同じ直線インデックスを求める
			for (i_loop2 = 0; i_loop2 < (int)vec_line_info.size(); i_loop2++)
			{
				if (vec_line_info[i_loop2].i_line_ID == i_loop)
				{
					vec_same_line_index.push_back(i_loop2);
				}
			}
			//	同じ直線IDの直線が2本以上あれば、その直線上に欠けがある
			if ((int)vec_same_line_index.size() >= 2)
			{
				//	同じ直線IDの直線から回帰直線を求める
				//	本当は同じ直線IDの全ての直線成分から回帰直線を求めるのがベストだが、面倒くさいので、一つ目の直線IDの直線から求める

				//	直線のxyを取得
				i_line_length = vec_line_info[vec_same_line_index[0]].i_end_index - vec_line_info[vec_same_line_index[0]].i_start_index + 1;
				d_x = new double[i_line_length];
				d_y = new double[i_line_length];

				for (i_loop2 = 0; i_loop2 < i_line_length; i_loop2++)
				{
					//	角度が80<θ<100のとき、直線はY軸に平行に近くなるため近似直線がうまくひけないので反転させる
					if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
					{
						//xy反転
						d_y[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dX;
						d_x[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dY;
					}
					else
					{
						d_x[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dX;
						d_y[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dY;
					}

				}
				//	この直線上の点群から回帰直線を求める
				c_global_math.ApproximationByLeastSquaresMethod(d_x, d_y, i_line_length, 1, d_coeff);

				//	欠けを求める範囲は、同じ直線IDで、直線と直線の間の区間。これをまず求める
				for (i_loop2 = 0; i_loop2 < (int)vec_same_line_index.size() - 1; i_loop2++)
				{
					vec_crack_info.clear();

					i_line_length = vec_line_info[vec_same_line_index[i_loop2 + 1]].i_start_index - vec_line_info[vec_same_line_index[i_loop2]].i_end_index + 1;
					for (int i_loop3 = 0; i_loop3 < i_line_length; i_loop3++)
					{
						//	角度が80<θ<100のとき、直線はY軸に平行に近くなるため近似直線がうまくひけないので反転させる
						if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
						{
							//	XY反転
							crack_info.xy_pos.dY = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dX;
							crack_info.xy_pos.dX = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dY;
						}
						else
						{
							crack_info.xy_pos.dX = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dX;
							crack_info.xy_pos.dY = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dY;
						}

						vec_crack_info.push_back(crack_info);
					}
					//	欠けを求める。回帰直線と、欠け候補の点との距離を求める
					for (int i_loop3 = 0; i_loop3 < (int)vec_crack_info.size(); i_loop3++)
					{
						//	点と直線の距離の公式
						d_distance = fabs(d_coeff[0] * vec_crack_info[i_loop3].xy_pos.dX - vec_crack_info[i_loop3].xy_pos.dY + d_coeff[1]) / sqrt(d_coeff[0] * d_coeff[0] + 1);
						vec_crack_info[i_loop3].d_crack_length = d_distance;
						vec_crack_info[i_loop3].d_slope_coeff = d_coeff[0];

					}
					//	角度が80<θ<100のとき、直線はY軸に平行に近くなるため近似直線がうまくひけないので反転させる
					if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
					{
						//	欠けを求めたのでXY座標を元に戻す
						for (int i_loop3 = 0; i_loop3 < (int)vec_crack_info.size(); i_loop3++)
						{
							XYty xy_temp;
							xy_temp.dX = vec_crack_info[i_loop3].xy_pos.dX;
							xy_temp.dY = vec_crack_info[i_loop3].xy_pos.dY;
							vec_crack_info[i_loop3].xy_pos.dX = xy_temp.dY;
							vec_crack_info[i_loop3].xy_pos.dY = xy_temp.dX;
						}
					}

					vec_all_crack_info.push_back(vec_crack_info);
				}
				delete[] d_x;
				delete[] d_y;
			}
		}
	}


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_5CrackInfo.csv";
		char chr_buff[MAX_PATH];

		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_all_crack_info.size(); i_loop++)
		{
			sprintf_s(chr_buff, "Crack%d\nX,Y,Length", i_loop);
			write_file << chr_buff << endl;
			for (i_loop2 = 0; i_loop2 < (int)vec_all_crack_info[i_loop].size(); i_loop2++)
			{
				sprintf_s(chr_buff, "%f,%f,%f", vec_all_crack_info[i_loop][i_loop2].xy_pos.dX, vec_all_crack_info[i_loop][i_loop2].xy_pos.dY, vec_all_crack_info[i_loop][i_loop2].d_crack_length);
				write_file << chr_buff << endl;
			}
		}
		write_file.close();
	}


	// ************************
	//
	//	欠けが閾値以上の欠けかを判定する。判定後、欠けを囲む矩形を作成する
	//
	// ************************

//	const double cd_crack_threshold = 5.0;		//	ピクセル座標
	double d_max_crack_length;
	CrackInfo max_crack;
	XYty crack_box[4];							//	矩形を囲む平行四辺形
	XYty minimum_crack_rect[4];					//	平行四辺形の最小外接矩形

	double d_max_x = -9999.9;
	double d_max_y = -9999.9;
	double d_min_x = 9999.9;
	double d_min_y = 9999.9;;

	max_crack.d_crack_length = max_crack.d_slope_coeff = max_crack.xy_pos.dX = max_crack.xy_pos.dY = 0.0;

	for (i_loop = 0; i_loop < (int)vec_all_crack_info.size(); i_loop++)
	{
		d_max_crack_length = -9999.999;
		for (i_loop2 = 0; i_loop2 < (int)vec_all_crack_info[i_loop].size(); i_loop2++)
		{
			//	欠けの中で最大の長さを求める
			if (vec_all_crack_info[i_loop][i_loop2].d_crack_length > d_max_crack_length)
			{
				d_max_crack_length = vec_all_crack_info[i_loop][i_loop2].d_crack_length;
				max_crack = vec_all_crack_info[i_loop][i_loop2];
			}
		}
		//	閾値以上の欠けであれば欠陥とし、矩形を作成する
		if (d_max_crack_length > ndMinDefectSize)
		{
			//	欠け検出フラグを立てる
			b_crack_detected = true;

			//	始点、終点、最大欠陥長さとなる点、の3点を囲う最小外接矩形を求める
			crack_box[0] = vec_all_crack_info[i_loop].front().xy_pos;	//	始点
			crack_box[1] = vec_all_crack_info[i_loop].back().xy_pos;	//	終点

			//MinX
			d_min_x = min(crack_box[0].dX, crack_box[1].dX);
			d_min_x = min(d_min_x, max_crack.xy_pos.dX);

			//MinY
			d_min_y = min(crack_box[0].dY, crack_box[1].dY);
			d_min_y = min(d_min_y, max_crack.xy_pos.dY);

			//MaxX
			d_max_x = max(crack_box[0].dX, crack_box[1].dX);
			d_max_x = max(d_max_x, max_crack.xy_pos.dX);

			//MaxY
			d_max_y = max(crack_box[0].dY, crack_box[1].dY);
			d_max_y = max(d_max_y, max_crack.xy_pos.dY);

			minimum_crack_rect[0].dX = d_min_x;
			minimum_crack_rect[0].dY = d_min_y;
			minimum_crack_rect[1].dX = d_max_x;
			minimum_crack_rect[1].dY = d_min_y;
			minimum_crack_rect[2].dX = d_min_x;
			minimum_crack_rect[2].dY = d_max_y;
			minimum_crack_rect[3].dX = d_max_x;
			minimum_crack_rect[3].dY = d_max_y;

			// ************************
			//
			//	矩形を画像に描画する
			//
			// ************************

			MgraColor(m_milGraphic, M_RGB888(255, 0, 0));
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[0].dX, (int)minimum_crack_rect[0].dY, (int)minimum_crack_rect[1].dX, (int)minimum_crack_rect[1].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[1].dX, (int)minimum_crack_rect[1].dY, (int)minimum_crack_rect[3].dX, (int)minimum_crack_rect[3].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[2].dX, (int)minimum_crack_rect[2].dY, (int)minimum_crack_rect[3].dX, (int)minimum_crack_rect[3].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[0].dX, (int)minimum_crack_rect[0].dY, (int)minimum_crack_rect[2].dX, (int)minimum_crack_rect[2].dY);
		}
	}

	//////////////////////////////////////////
	//	ここまでが欠け検出アルゴリズム
	//////////////////////////////////////////

	//////////////////////////////////////////
	//	ここからは外観検査(傷検査)
	//////////////////////////////////////////

#if 0

	//	平均画像用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);

	//	画像をコピー
	MbufCopy(mil_inspection_image, mil_average_image);

	//	クロージングとオープニングでレンズの汚れ等のゴミを除去する。サイズを10にすることで以下の平均輝度値との置き換えをやめた
	MimClose(mil_average_image, mil_average_image, 10, M_GRAYSCALE);
	MimOpen(mil_average_image, mil_average_image, 10, M_GRAYSCALE);


#if 0
	//	検査対象領域の平均輝度値を求める
	MimStat(mil_average_image, mil_stat_result, M_MEAN, M_MASK, mil_blob_image, M_NULL);
	MimGetResult(mil_stat_result, M_MEAN + M_TYPE_DOUBLE, &d_average_value);
	//	ブロブを膨張する
	MimDilate(mil_blob_image, mil_temp_image, 30, M_GRAYSCALE);
	//	膨張ブロブ画像とブロブ画像の差分を取る
	MimArith(mil_temp_image, mil_blob_image, mil_diff_image, M_SUB_ABS);
	//	これを膨張する
	MimDilate(mil_diff_image, mil_diff_image, 20, M_GRAYSCALE);
	//	その部分を平均輝度値に置換する
	MimClip(mil_diff_image, mil_temp_image, M_IN_RANGE, 255, 255, d_average_value, M_NULL);
	//	これを元の画像の膨張部分に置き換える
	MimArith(mil_average_image, mil_diff_image, mil_diff_image, M_SUB);
	MimArith(mil_diff_image, mil_temp_image, mil_average_image, M_MAX);

	//	50回平均する
	for (i_loop = 0; i_loop < 50; i_loop++)
	{
		MimConvolve(mil_average_image, mil_average_image, M_SMOOTH);
	}
#endif

	//	差分を取る
	MimArith(mil_inspection_image, mil_average_image, mil_diff_image, M_SUB_ABS);

	//	差分画像の二値化
	MimBinarize(mil_diff_image, mil_temp_image, M_GREATER_OR_EQUAL, 7, M_NULL);

	//	オープニングで微小な差分を除去
	MimOpen(mil_temp_image, mil_temp_image, 1, M_GRAYSCALE);

	//	この画像と、検査対象のブロブの画像で重なったところだけを抽出する
	//	エッジのところは微妙なのでブロブを収縮させて、エッジ付近の傷は見ないようにする
	MimErode(mil_blob_image, mil_diff_image, 10, M_GRAYSCALE);
	MimArith(mil_temp_image, mil_diff_image, mil_temp_image, M_AND);

	//	傷が検出されたかどうかは、傷画像の画素値が255となる画素があるかどうかで判断する
	MIL_DOUBLE d_count;
	MimStat(mil_temp_image, mil_stat_result, M_NUMBER, M_EQUAL, 255, M_NULL);
	MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_DOUBLE, &d_count);
	if (d_count > 0)
	{
		b_cut_detected = true;
	}
	else
	{
		b_cut_detected = false;
	}

	//	これを検査画像と合成する
	MimArith(mil_result_image, mil_temp_image, mil_result_image, M_ADD + M_SATURATION);

#endif

	// ************************
	//
	//	検査結果の作成
	//
	// ************************

	//	ファイル出力
	MbufExport(nstrResultImage.c_str(), M_BMP, mil_result_image);

	//	傷もしくは欠けが検出されたらNGを返す
	if (b_crack_detected == true || b_cut_detected == true)
	{
		i_ret = 1;
	}
	else
	{
		i_ret = 0;
	}

	//	曲線の有無を返す
	if (b_curve == true)
	{
		*npiCurve = 1;
	}
	else
	{
		*npiCurve = 0;
	}

	//	欠陥種別を返す
	if (b_cut_detected == true && b_crack_detected == false)
	{
		*npiNGContent = 1;
	}
	else if (b_cut_detected == false && b_crack_detected == true)
	{
		*npiNGContent = 2;
	}
	else if (b_cut_detected == true && b_crack_detected == true)
	{
		*npiNGContent = 3;
	}
	else
	{
		*npiNGContent = 0;
	}


	// ************************
	//
	//	終了処理
	//
	// ************************

Finish:
	//	メモリ開放
	MbufFree(mil_inspection_image);
	MbufFree(mil_inspection_image_binary);
	MbufFree(mil_edge_image);
	MbufFree(mil_blob_image);
	MbufFree(mil_diff_image);
	MbufFree(mil_temp_image);
	MbufFree(mil_result_image);
	MbufFree(mil_average_image);

	MblobFree(mil_blob_result);
	MblobFree(mil_blob_feature_list);

	//	エッジファインダーコンテキストを解放
	MedgeFree(mil_edge);
	MedgeFree(mil_edge_result);

	MimFree(mil_stat_result);

	return i_ret;
}
