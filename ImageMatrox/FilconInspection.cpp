#include "stdafx.h"
#include "FilconInspection.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>

#define DEBUG_OUTPUT_HEATMAP_FILES				// for heatmap
#pragma warning(disable : 4995)

#define PI (atan(1.0)*4.0)

CFilconInspection::CFilconInspection()
{
}


CFilconInspection::~CFilconInspection()
{
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		フィルコンマスク検査アルゴリズム(デモ簡易版)

	2.パラメタ説明
		nstrInputImage	(IN)	検査画像ファイルパス
		nstrResultImage (OUT)	結果画像ファイルパス

	3.概要
		フィルコンマスク検査アルゴリズム(

	4.機能説明
		フィルコンマスク検査アルゴリズム(

	5.戻り値
		0:	OK
		1:	NG

	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CFilconInspection::execInspection(string nstrInputImage, string nstrResultImage)
{
	MIL_ID mil_inspection_image;
	MIL_ID mil_average_image;
	MIL_ID mil_diff_image;
	MIL_ID mil_result_image;
	MIL_ID mil_stat_result;
	SIZE sz_image_size;
	bool b_cut_detected;

	MimAllocResult(m_milSys, M_DEFAULT, M_STAT_LIST, &mil_stat_result);

	//	画像をファイルからロードする
	MbufRestore(nstrInputImage.c_str(), m_milSys, &mil_inspection_image);
	//	画像サイズ取得
	sz_image_size.cx = (long)MbufInquire(mil_inspection_image, M_SIZE_X, M_NULL);
	sz_image_size.cy = (long)MbufInquire(mil_inspection_image, M_SIZE_Y, M_NULL);

	//	検査対象ブロブの膨張と縮小の差分画像の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_diff_image);
	//	平均画像用の画像バッファを確保
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);
	//	結果画像用の画像バッファを確保
	MbufAllocColor(m_milSys, 3, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_result_image);
	//	結果画像バッファに検査画像をコピーしておく
	MbufCopyColor(mil_inspection_image, mil_result_image, M_ALL_BANDS);


	//	画像をコピー
	MbufCopy(mil_inspection_image, mil_average_image);

	//	クロージングとオープニングでレンズの汚れ等のゴミを除去する
	MimClose(mil_average_image, mil_average_image, 5, M_GRAYSCALE);
	MimOpen(mil_average_image, mil_average_image, 5, M_GRAYSCALE);

	//	差分を取る
	MimArith(mil_inspection_image, mil_average_image, mil_diff_image, M_SUB_ABS);

	//	差分画像の二値化
	MimBinarize(mil_diff_image, mil_diff_image, M_GREATER_OR_EQUAL, 10, M_NULL);

	//	オープニングで微小な差分を除去
	//MimOpen(mil_diff_image, mil_diff_image, 1, M_GRAYSCALE);
	MimRank(mil_diff_image, mil_diff_image, M_3X3_RECT, M_MEDIAN, M_GRAYSCALE);

	//	もし差分が何もなければOK
	//	傷が検出されたかどうかは、傷画像の画素値が255となる画素があるかどうかで判断する
	MIL_DOUBLE d_count;
	MimStat(mil_diff_image, mil_stat_result, M_NUMBER, M_EQUAL, 255, M_NULL);
	MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_DOUBLE, &d_count);
	if (d_count > 10)
	{
		b_cut_detected = true;
	}
	else
	{
		b_cut_detected = false;
	}



	//	これを検査画像と合成する
	MimArith(mil_result_image, mil_diff_image, mil_result_image, M_ADD + M_SATURATION);

	//	ファイル出力
	MbufExport(nstrResultImage.c_str(), M_BMP, mil_result_image);

	MbufFree(mil_inspection_image);
	MbufFree(mil_average_image);
	MbufFree(mil_diff_image);
	MbufFree(mil_result_image);

	MimFree(mil_stat_result);

	if (b_cut_detected == true)
	{
		return 1;
	}
	else
	{
		return 0;
	}

}
