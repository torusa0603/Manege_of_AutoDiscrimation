#pragma once
#include "MatroxCommon.h"
#include "ShinEtsuInspection.h"
#include "FilconInspection.h"
#include "shlwapi.h"

#include <vector>

class CMatroxImageProcess :public CMatroxCommon
{
public:
	CMatroxImageProcess(void);
	~CMatroxImageProcess(void);
	//	初期処理
	void init();
	//	終了処理
	void close();


	//	信越化学工業検査アルゴリズム(リリース版)
	int execShinEtsuInspection(string nstrInputImage, string nstrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve);
	//	フィルコンマスク検査アルゴリズム(デモ簡易版)
	int execFilconMaskInspection(string nstrInputImage, string nstrResultImage);

private:
	CShinEtsuInspection m_cShinEtsu;
	CFilconInspection m_cFilcon;

};
