#include "StdAfx.h"
#include "MatroxImageProcess.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>

#define DEBUG_OUTPUT_HEATMAP_FILES				// for heatmap
#pragma warning(disable : 4995)

#define PI (atan(1.0)*4.0)

CMatroxImageProcess::CMatroxImageProcess(void)
{
}

CMatroxImageProcess::~CMatroxImageProcess(void)
{
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		初期処理

	2.パラメタ説明
		

	3.概要
		初期処理

	4.機能説明
		初期処理

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxImageProcess::init() 
{
	//	デバッグファイル出力用フォルダーを作成
	if (m_bDebugON == true)
	{
		_mkdir(m_strDebugFolder.c_str());
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		終了処理

	2.パラメタ説明
	

	3.概要
		終了処理

	4.機能説明
		終了処理

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxImageProcess::close()
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
int CMatroxImageProcess::execShinEtsuInspection(string nstrInputImage, string nstrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve)
{
	//	デバッグ出力ONの場合、ファイル名を設定する
	if (m_bDebugON == true)
	{
		size_t path_i = nstrInputImage.find_last_of("\\") + 1;
		size_t ext_i = nstrInputImage.find_last_of(".");
		m_strDebugFileIdentifiedName = nstrInputImage.substr(path_i, ext_i - path_i);
	}

	return m_cShinEtsu.execInspection(nstrInputImage, nstrResultImage, ndMinDefectSize, npiNGContent, npiCurve);
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
int CMatroxImageProcess::execFilconMaskInspection(string nstrInputImage, string nstrResultImage)
{
	return m_cFilcon.execInspection(nstrInputImage, nstrResultImage);
}