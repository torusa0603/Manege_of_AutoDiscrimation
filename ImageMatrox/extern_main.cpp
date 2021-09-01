#include "stdafx.h"
#include "MatroxCommon.h"
#include "MatroxCamera.h"
#include "MatroxGraphic.h"
#include "MatroxImageProcess.h"
#include "SampleInspection.h"
#include <vector>
#include <locale.h>
#include <omp.h>

CMatroxCommon*	pMatroxCommon(NULL);
CMatroxCamera*	pMatroxCamera(NULL);
CMatroxGraphic*	pMatroxGraphic(NULL);
CMatroxImageProcess*	pMatroxImageProcess(NULL);

static int si_inspection_count = 0;

extern "C" __declspec(dllexport) int WINAPI sifSaveInspectionResultImage( char *nchrFilePath );

#pragma warning(disable:4996)

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像処理ボードの初期化を行う関数

	2.パラメタ説明
		nhDispHandle	画像ウインドウのハンドル

	3.概要
		画像処理ボードの初期化を行う

	4.機能説明
		画像処理ボードの初期化を行う

	5.戻り値
		0:OK

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifInitializeImageProcess( HWND nhDispHandle, char* nchrSettingPath)
{
#ifdef NOMTX
	return 0;
#endif
	int i_ret = 0;
	string str_setting_path;

	//	オブジェクト作成
	if( pMatroxCommon == NULL )
	{
		pMatroxCommon = new CMatroxCommon();
	}
	if( pMatroxCamera == NULL )
	{
		pMatroxCamera = new CMatroxCamera();
	}
	if( pMatroxGraphic == NULL )
	{
		pMatroxGraphic = new CMatroxGraphic();
	}
	if( pMatroxImageProcess == NULL )
	{
		pMatroxImageProcess = new CMatroxImageProcess();
	}

	//	初期化ファイル格納フォルダ
	if( nchrSettingPath == NULL )
	{
		str_setting_path = "";
	}
	else
	{
		str_setting_path = nchrSettingPath;
	}

	//	マトロックスの初期化を行う
	i_ret = pMatroxCommon->Initial( nhDispHandle, str_setting_path );
	if( i_ret == 0 )
	{
		//	描画先を通常のバッファにする
		pMatroxGraphic->changeDrawDestination(0);
	}

	pMatroxImageProcess->init();

	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		i_ret = FATAL_ERROR_ID;
	}

	return i_ret;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像処理ボードのクローズ処理を行う関数

	2.パラメタ説明
		なし

	3.概要
		画像処理ボードの初期化を行う

	4.機能説明
		画像処理ボードの初期化を行う

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifCloseImageProcess()
{

#ifdef NOMTX
	return;
#endif

	if (pMatroxImageProcess != NULL)
	{
		//	終了処理
		pMatroxImageProcess->close();
	}

	//	マトロックスの終了処理を行う
	if (pMatroxCommon != NULL)
	{
		if (pMatroxCommon->getThoughStatus() == true)
		{
			if (pMatroxCamera != NULL)
			{
				pMatroxCamera->setTriggerModeOff();
				pMatroxCamera->doFreeze();
			}
		}
		pMatroxCommon->CloseMatrox();
	}

	//	オブジェクト破棄
	if( pMatroxImageProcess != NULL )
	{
		delete pMatroxImageProcess;
		pMatroxImageProcess = NULL;
	}
	if( pMatroxCommon != NULL )
	{
		delete pMatroxCommon;
		pMatroxCommon = NULL;
	}
	if( pMatroxCamera != NULL )
	{
		delete pMatroxCamera;
		pMatroxCamera = NULL;
	}
	if( pMatroxGraphic != NULL )
	{
		delete pMatroxGraphic;
		pMatroxGraphic = NULL;
	}

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		指定した画像上のポイントの輝度値を取得する

	2.パラメタ説明
		 nNowPoint	: 画像上のポイント
		 RValue		: Rの輝度値
		 GValue		: Gの輝度値
		 BValue		: Bの輝度値
				
	3.概要
		指定した画像上のポイントの輝度値を取得する

	4.機能説明
		指定した画像上のポイントの輝度値を取得する

	5.戻り値
		0: 指定された位置に画像が存在した
		-1:指定された位置に画像が存在しない

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetPixelValueOnPosition( POINT nNowPoint, int *RValue, int *GValue,int *BValue )
{

#ifdef NOMTX
	return 0;
#endif

	int r,g,b;
	int i_ret;

	//	指定した画像上のポイントの輝度値を取得する
	i_ret = pMatroxCommon->getPixelValueOnPosition( nNowPoint, r, g, b );
	*RValue = r;
	*GValue = g;
	*BValue = b;

	return i_ret;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		現在のデジタルズームの倍率を取得する関数

	2.パラメタ説明
		 なし
				
	3.概要
		現在のデジタルズームの倍率を取得する

	4.機能説明
		現在のデジタルズームの倍率を取得する

	5.戻り値
		ズーム倍率

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) double WINAPI sifGetZoomMag()
{

#ifdef NOMTX
	return 0.0;
#endif

	double d_zoom_mag;

	d_zoom_mag = pMatroxCommon->getZoomMag();
	return d_zoom_mag;

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		スルーにする

	2.パラメタ説明
		 なし
				
	3.概要
		スルーにする

	4.機能説明
		スルーにする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifThrough()
{

#ifdef NOMTX
	return 0;
#endif

	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	//画像をスルーにする
	pMatroxCamera->doThrough();

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		フリーズする

	2.パラメタ説明
		 なし
				
	3.概要
		フリーズする

	4.機能説明
		フリーズする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifFreeze()
{

#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	//画像をフリーズする
	pMatroxCamera->doFreeze();

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		1枚画像をGrabする

	2.パラメタ説明
		 なし
				
	3.概要
		1枚画像をGrabする

	4.機能説明
		1枚画像をGrabする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifGetOneGrab()
{

#ifdef NOMTX
	return;
#endif

	//画像をフリーズする
	pMatroxCamera->doFreeze();
	//	1回grabする
	pMatroxCamera->getOneGrab();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		矩形を描画する

	2.パラメタ説明
		 nptLeftTopPoint	左上の画面座標
		nptRightBottomPoint	右下の画面座標
				
	3.概要
		矩形を描画する

	4.機能説明
		矩形を描画する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifDrawRectangle( POINT nptLeftTopPoint, POINT nptRightBottomPoint )
{

#ifdef NOMTX
	return;
#endif

	pMatroxGraphic->setGraphicColor( GRAPHIC_COLOR_GREEN );
	pMatroxGraphic->DrawRectangle( nptLeftTopPoint, nptRightBottomPoint );
}


/*------------------------------------------------------------------------------------------
	1.日本語名
		グラフィックをクリアする

	2.パラメタ説明
		 なし
				
	3.概要
		グラフィックをクリアする

	4.機能説明
		グラフィックをクリアする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifDrawClear()
{

#ifdef NOMTX
	return;
#endif

	pMatroxGraphic->clearAllGraphic();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像を保存する

	2.パラメタ説明
				
	3.概要
		画像を保存する

	4.機能説明
		画像を保存する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifSaveImage( RECT nrctSaveArea, BOOL nAllSaveFlg, char *nchrFilePath, BOOL nbSaveMono )
{

#ifdef NOMTX
	return;
#endif

	string cs_file_path;
	cs_file_path = nchrFilePath;

	pMatroxCommon->saveImage( nrctSaveArea, nAllSaveFlg, cs_file_path, nbSaveMono );
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		オリジナル画像を保存する

	2.パラメタ説明
				
	3.概要
		オリジナル画像を保存する

	4.機能説明
		オリジナル画像を保存する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifSaveOrigImage( RECT nrctSaveArea, BOOL nAllSaveFlg, char *nchrFilePath )
{

#ifdef NOMTX
	return;
#endif

	string cs_file_path;
	cs_file_path = nchrFilePath;

	pMatroxCommon->saveOrigImage( nrctSaveArea, nAllSaveFlg, cs_file_path );

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		十字を描画する

	2.パラメタ説明
		niCenterX   十字の中心X
		niCenterY	十字の中心Y
				
	3.概要
		十字を描画する

	4.機能説明
		十字を描画する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifDrawCross( int niCenterX, int niCenterY )
{

#ifdef NOMTX
	return;
#endif
//	cGraphic.setGraphicColor( GRAPHIC_COLOR_YELLOW );

	pMatroxGraphic->setGraphicColor( GRAPHIC_COLOR_YELLOW );
	//	太さ2で書く。(デジタルズームで縮小されている可能性があるので)

	POINT	pt_point_s, pt_point_e;
	pt_point_s.x = niCenterX - 5;
	pt_point_s.y = niCenterY;
	pt_point_e.x = niCenterX + 5;
	pt_point_e.y = niCenterY;
	pMatroxGraphic->DrawLine( pt_point_s, pt_point_e );

	pt_point_s.x = niCenterX;
	pt_point_s.y = niCenterY - 5;
	pt_point_e.x = niCenterX;
	pt_point_e.y = niCenterY + 5;
	pMatroxGraphic->DrawLine( pt_point_s, pt_point_e );

	niCenterY = niCenterY + 1;
	pt_point_s.x = niCenterX - 5;
	pt_point_s.y = niCenterY;
	pt_point_e.x = niCenterX + 5;
	pt_point_e.y = niCenterY;
	pMatroxGraphic->DrawLine( pt_point_s, pt_point_e );

	niCenterY = niCenterY - 1;
	niCenterX = niCenterX + 1;
	pt_point_s.x = niCenterX;
	pt_point_s.y = niCenterY - 5;
	pt_point_e.x = niCenterX;
	pt_point_e.y = niCenterY + 5;
	pMatroxGraphic->DrawLine( pt_point_s, pt_point_e );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		直線を描画する

	2.パラメタ説明
		niCenterX   十字の中心X
		niCenterY	十字の中心Y
				
	3.概要
		直線を描画する

	4.機能説明
		直線を描画する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifDrawLine( POINT nptStartPoint, POINT nptEndPoint )
{

#ifdef NOMTX
	return;
#endif
	pMatroxGraphic->setGraphicColor( GRAPHIC_COLOR_WHITE );
	pMatroxGraphic->DrawLine( nptStartPoint, nptEndPoint );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		今スルー中かフリーズ中か取得する

	2.パラメタ説明
		なし

	3.概要
		今スルー中かフリーズ中か取得する

	4.機能説明
		今スルー中かフリーズ中か取得する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) BOOL WINAPI sifGetThoughStatus()
{

#ifdef NOMTX
	return true;
#endif
	return pMatroxCommon->getThoughStatus();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		今写ってる画像をフリーズ直後の画像に戻す

	2.パラメタ説明
		なし

	3.概要
		今写ってる画像をフリーズ直後の画像に戻す

	4.機能説明
		今写ってる画像をフリーズ直後の画像に戻す

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifSetOriginImage()
{

#ifdef NOMTX
	return;
#endif
	pMatroxCommon->setOriginImage();
	return;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		画像の倍率設定を行う

	2.パラメタ説明
		ndMag	倍率

	3.概要
		画像の倍率設定を行う

	4.機能説明
		画像の倍率設定を行う

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetMag( double ndMag )
{

#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	pMatroxCommon->setShowImageMag( ndMag );

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像サイズを取得する

	2.パラメタ説明
		なし

	3.概要
		画像サイズを取得する

	4.機能説明
		画像サイズを取得する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) SIZE WINAPI sifGetImageSize()
{

#ifdef NOMTX
	SIZE a;
	return a;
#endif
	return pMatroxCommon->getImageSize();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像をロードする

	2.パラメタ説明
		nchModelImageFilePath		ロードする画像ファイルパス

	3.概要
		画像をロードする

	4.機能説明
		画像をロードする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifLoadImage( char *nchLoadImageFilePath )
{

#ifdef NOMTX
	return 0;
#endif

	string cs_load_image_path;
	cs_load_image_path = nchLoadImageFilePath;

	if( pMatroxCommon->getThoughStatus() == true )
	{
		pMatroxCamera->doFreeze();
	}
	return pMatroxCommon->loadImage(cs_load_image_path);

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		差分用オリジナル画像を現在表示されている画像で登録する

	2.パラメタ説明
		なし

	3.概要
		画像をロードする

	4.機能説明
		画像をロードする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifsetDiffOrgImage()
{

#ifdef NOMTX
	return 0;
#endif
	return pMatroxCommon->setDiffOrgImage();

}
/*------------------------------------------------------------------------------------------
	1.日本語名
		差分用オリジナル画像を現在表示されている画像で登録する

	2.パラメタ説明
		なし

	3.概要
		画像をロードする

	4.機能説明
		画像をロードする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) void WINAPI sifresetDiffMode()
{

#ifdef NOMTX
	return ;
#endif
	pMatroxCommon->resetDiffMode();

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		Gainと露光時間をセットする

	2.パラメタ説明
		なし

	3.概要
		Gainと露光時間をセットする

	4.機能説明
		Gainと露光時間をセットする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetGainAndExposureTime(double ndGain, double ndExposureTime)
{
#ifdef NOMTX
	return 0;
#endif

	return pMatroxCamera->setGainAndExposureTime(ndGain,ndExposureTime);
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		平均画像をGrabする。ファイルに保存

	2.パラメタ説明
		 niAverageNum	平均回数
		 nchrFilePath	出力ファイルパス
		 nbSaveMono		true: モノクロ(1バンド)出力　false: カラー(3バンド)出力
				
	3.概要
		平均画像をGrabする

	4.機能説明
		平均画像をGrabする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetAveragedImageGrab (int niAverageNum,char *nchrFilePath, BOOL nbSaveMono, BOOL nbSaveOnMemory)
{

#ifdef NOMTX
	return;
#endif
	string cs_file_path;
	cs_file_path = nchrFilePath;

	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}

	//	1回grabする
	pMatroxCamera->getAveragedImageGrab(niAverageNum, cs_file_path, nbSaveMono, nbSaveOnMemory);

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		Gainをセットする

	2.パラメタ説明
		なし

	3.概要
		Gainをセットする

	4.機能説明
		Gainをセットする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetGain(double ndGain)
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;

	}
	return pMatroxCamera->setGain(ndGain);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		露光時間をセットする

	2.パラメタ説明
		なし

	3.概要
		露光時間をセットする

	4.機能説明
		露光時間をセットする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetExposureTime(double ndExposureTime)
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	return pMatroxCamera->setExposureTime(ndExposureTime);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		ブラックレベルをセットする

	2.パラメタ説明
		なし

	3.概要
		ブラックレベルをセットする

	4.機能説明
		ブラックレベルをセットする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetBlackLevel(double ndBlackLevel)
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	return pMatroxCamera->setBlackLevel(ndBlackLevel);
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		ブラックレベルのMAX,MINを取得する

	2.パラメタ説明


	3.概要
		ブラックレベルのMAX,MINを取得する

	4.機能説明
		ブラックレベルのMAX,MINを取得する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetBlackLevelMaxMin( double *npdMax, double *npdMin )
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	return pMatroxCamera->getBlackLevelMaxMin(npdMax,npdMin);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		GainのMAX,MINを取得する

	2.パラメタ説明


	3.概要
		GainのMAX,MINを取得する

	4.機能説明
		GainのMAX,MINを取得する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetGainMaxMin( double *npdMax, double *npdMin )
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}

	return pMatroxCamera->getGainMaxMin(npdMax,npdMin);
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		露光時間のMAX,MINを取得する

	2.パラメタ説明


	3.概要
		露光時間のMAX,MINを取得する

	4.機能説明
		露光時間のMAX,MINを取得する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetExposureTimeMaxMin( double *npdMax, double *npdMin )
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	return pMatroxCamera->getExposureTimeMaxMin(npdMax,npdMin);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		検査結果画像表示用のディスプレイハンドルを設定する

	2.パラメタ説明


	3.概要
		検査結果画像表示用のディスプレイハンドルを設定する

	4.機能説明
		検査結果画像表示用のディスプレイハンドルを設定する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetDispHandleForInspectionResult( HWND nhDispHandleForInspectionResult )
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}

	return pMatroxCommon->setDispHandleForInspectionResult( nhDispHandleForInspectionResult );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		検査結果画像の倍率設定を行う

	2.パラメタ説明
		ndMag	倍率

	3.概要
		検査結果画像の倍率設定を行う

	4.機能説明
		検査結果画像の倍率設定を行う

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetMagForInspectionResult( double ndMag )
{

#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}

	pMatroxCommon->setZoomMagForInspectionResult( ndMag );
	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		現在のデジタルズームの倍率を取得する関数(検査結果画像)

	2.パラメタ説明
		 なし
				
	3.概要
		現在のデジタルズームの倍率を取得する(検査結果画像)

	4.機能説明
		現在のデジタルズームの倍率を取得する(検査結果画像)

	5.戻り値
		ズーム倍率

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) double WINAPI sifGetZoomMagForInspectionResult()
{

#ifdef NOMTX
	return 0.0;
#endif

	double d_zoom_mag;

	d_zoom_mag = pMatroxCommon->getZoomMagForInspectionResult();
	return d_zoom_mag;

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		矩形領域内の平均輝度値を取得する

	2.パラメタ説明
				
	3.概要
		矩形領域内の平均輝度値を取得する

	4.機能説明
		矩形領域内の平均輝度値を取得する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetAveragePixelValueOnRegion( RECT nrctRegion )
{

#ifdef NOMTX
	return -1;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}
	return pMatroxCommon->getAveragePixelValueOnRegion( nrctRegion );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		カメラ映像が映るディスプレイを切り替える

	2.パラメタ説明
		nhDispHandle   ディスプレイハンドル

	3.概要
		カメラ映像が映るディスプレイを切り替える

	4.機能説明
		カメラ映像が映るディスプレイを切り替える

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetDisplayHandle(HWND nhDispHandle)
{
#ifdef NOMTX
	return 0;
#endif
	//	致命的なエラーチェック
	if (pMatroxCommon->getFatalErrorOccured() == true)
	{
		return FATAL_ERROR_ID;
	}

	pMatroxCommon->SetDisplayHandle(nhDispHandle);

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		致命的なエラーが起きたか否か取得する

	2.パラメタ説明


	3.概要
		致命的なエラーが起きたか否か取得する

	4.機能説明


	5.戻り値
		

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) BOOL WINAPI sifGetFatalErrorOccured()
{
#ifdef NOMTX
	return FALSE;
#endif
	return pMatroxCommon->getFatalErrorOccured();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		信越化学工業検査アルゴリズム(リリース版)

	2.パラメタ説明


	3.概要
		終了処理

	4.機能説明
		終了処理

	5.戻り値
		0:	欠陥なし
		1:	欠け
		2:	傷
		3:　傷&欠け
		-1: 検査失敗

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifExecShinEtsuInspection(char* nchrInputImage, char* nchrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve)
{
#ifdef NOMTX
	return;
#endif
	return pMatroxImageProcess->execShinEtsuInspection(nchrInputImage, nchrResultImage, ndMinDefectSize, npiNGContent, npiCurve);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		フィルコンマスク検査アルゴリズム(デモ簡易版)

	2.パラメタ説明


	3.概要
		フィルコンマスク検査アルゴリズム(デモ簡易版)

	4.機能説明
		フィルコンマスク検査アルゴリズム(デモ簡易版)

	5.戻り値
		0:	OK
		1:	NG

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifExecFilconMaskInspection(char* nchrInputImage, char* nchrResultImage)
{
#ifdef NOMTX
	return;
#endif
	return pMatroxImageProcess->execFilconMaskInspection(nchrInputImage, nchrResultImage);
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		トリガモード オフ設定

	2.パラメタ説明


	3.概要
		トリガモード オフ設定

	4.機能説明


	5.戻り値


	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetTriggerModeOff( void )
{
#ifdef NOMTX
	return	0;
#endif
	return	pMatroxCamera->setTriggerModeOff();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		トリガモード ソフトウェア設定

	2.パラメタ説明


	3.概要
		トリガモード ソフトウェア設定

	4.機能説明


	5.戻り値


	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetTriggerModeSoftware( void )
{
#ifdef NOMTX
	return	0;
#endif
	return	pMatroxCamera->setTriggerModeSoftware();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		トリガモード ハードウェア設定

	2.パラメタ説明
		nszTrigger		トリガ名 (Ex.acA1300-30gmの場合Line1)

	3.概要
		トリガモード ハードウェア設定

	4.機能説明


	5.戻り値


	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSetTriggerModeHardware( const char* nszTrigger )
{
#ifdef NOMTX
	return	0;
#endif
	return	pMatroxCamera->setTriggerModeHardware( nszTrigger );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		ソフトウェアトリガ実行

	2.パラメタ説明


	3.概要
		ソフトウェアトリガ実行

	4.機能説明


	5.戻り値


	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifExecuteSoftwareTrigger( void )
{
#ifdef NOMTX
	return	0;
#endif
	return	pMatroxCamera->executeSoftwareTrigger();
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		リングバッファ(m_lstImageGrab)の白黒画像データを取得する

	2.パラメタ説明
		nlArySize	:	ユーザー側が準備した画像バッファサイズ
		npbyteData	:	返却用画像バッファ（ユーザ側でビットマップサイズ分の配列を準備の事）

	3.概要
		リングバッファ内の一番古い白黒画像データを取得する

	4.機能説明
		リングバッファ内の一番古い白黒画像データを取得する

	5.戻り値
		 0			:	成功
		-1			:	未初期化、画像無し、画像バッファサイズ少ない

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifGetMonoBitmapData( const long nlArySize, BYTE *npbyteData )
{
#ifdef NOMTX
	return	0;
#endif
	return	pMatroxCommon->getMonoBitmapData( nlArySize, npbyteData );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		サンプル動作 ソフトトリガ連続画像取得処理

	2.パラメタ説明
		nszFoldeName	:	画像ファイル保存先

	3.概要
		サンプル動作 ソフトトリガ連続画像取得処理

	4.機能説明
		サンプル動作 ソフトトリガ連続画像取得処理

	5.戻り値
		 0			:	成功
		-1			:	失敗

	6.備考
		なし
------------------------------------------------------------------------------------------*/
extern "C" __declspec(dllexport) int WINAPI sifSampleInspection( const char* nszFoldeName )
{
#ifdef NOMTX
	return	0;
#endif

	int		i_ret	= -1;

	while( true )
	{
		// ソフトトリガ設定
		if( 0 != pMatroxCamera->executeSoftwareTrigger() )
		{
			break;
		}

		// サンプル動作
		CSampleInspection	cSampleInspection;
		if( 0 != cSampleInspection.execInspection( nszFoldeName ) )
		{
			break;
		}

		// トリガオフ設定
		if( 0 != pMatroxCamera->setTriggerModeOff() )
		{
			break;
		}
		break;
	}

	return	i_ret;
}
