#pragma once
#include "matroxcommon.h"

class CMatroxCamera :public CMatroxCommon
{
public:
	CMatroxCamera(void);
	~CMatroxCamera(void);

	//	カメラ映像をフリーズする
	int doFreeze();
	//	カメラ映像をスルーにする
	int doThrough();
	//	1枚画像をGrabする
	void getOneGrab();
	//	差分画像を作成する
	void makeDiffImage();
	//	平均画像を取得する
	void getAveragedImageGrab( int niAverageNum, string ncsFilePath, BOOL nbSaveMono, BOOL nbSaveOnMemory);

	//	Gainと露光時間をセットする
	int setGainAndExposureTime(double ndGain, double ndExposureTime);
	//	ブラックレベルをセットする
	int setBlackLevel( double ndBlackLevel );
	//	Gainをセットする
	int setGain( double ndGain );
	//	露光時間をセットする
	int setExposureTime(double ndExposureTime);

	//	ブラックレベルのMAX,MINを取得する
	int getBlackLevelMaxMin( double *npdMax, double *npdMin );
	//	GainのMAX,MINを取得する
	int getGainMaxMin( double *npdMax, double *npdMin );
	//	露光時間のMAX,MINを取得する
	int getExposureTimeMaxMin( double *npdMax, double *npdMin );

	//	トリガモード オフ設定
	int setTriggerModeOff( void );
	//	トリガモード ソフトウェア設定
	int setTriggerModeSoftware( void );
	//	トリガモード ハードウェア設定
	int setTriggerModeHardware( const string nstrTrigger );
	//	ソフトウェアトリガ実行
	int executeSoftwareTrigger( void );
private:

	//	フック関数
	static long MFTYPE ProcessingFunction( long nlHookType, MIL_ID nEventId, void *npUserDataPtr ); 
};
