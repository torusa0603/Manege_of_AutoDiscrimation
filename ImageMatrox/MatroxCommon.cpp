#include "StdAfx.h"
#include "MatroxCommon.h"
#include <time.h>
#include <fstream>
//	スタティック変数の初期化
MIL_ID CMatroxCommon::m_milApp = M_NULL;
MIL_ID CMatroxCommon::m_milSys = M_NULL;
MIL_ID CMatroxCommon::m_milDisp = M_NULL;
MIL_ID CMatroxCommon::m_milDispForInspectionResult = M_NULL;
MIL_ID CMatroxCommon::m_milDigitizer = M_NULL;
MIL_ID CMatroxCommon::m_milShowImage = M_NULL;
MIL_ID CMatroxCommon::m_milPreShowImage = M_NULL;
MIL_ID CMatroxCommon::m_milMonoImage = M_NULL;
MIL_ID CMatroxCommon::m_milDiffOrgImage = M_NULL;
MIL_ID CMatroxCommon::m_milDiffTargetImage = M_NULL;
MIL_ID CMatroxCommon::m_milOriginalImage = M_NULL;
MIL_ID CMatroxCommon::m_milGraphic = M_NULL;
MIL_ID CMatroxCommon::m_milOverLay = M_NULL;
MIL_ID CMatroxCommon::m_milOverLayForInspectionResult = M_NULL;
MIL_ID CMatroxCommon::m_milDraphicSaveImage = M_NULL;
MIL_ID CMatroxCommon::m_milImageGrab[MAX_IMAGE_GRAB_NUM] = {0};
MIL_ID CMatroxCommon::m_milAverageImageGrab[MAX_AVERAGE_IMAGE_GRAB_NUM] = {0};
MIL_ID CMatroxCommon::m_milAverageImageCalc = M_NULL;
MIL_ID CMatroxCommon::m_milInspectionResultImage = M_NULL;
MIL_ID CMatroxCommon::m_milInspectionResultImageTemp = M_NULL;
string CMatroxCommon::m_strCameraFilePath = "";
SIZE CMatroxCommon::m_szImageSize = {0,0};
SIZE CMatroxCommon::m_szImageSizeForCamera = {0,0};
int CMatroxCommon::m_iBoardType = -1;
HWND   CMatroxCommon::m_hWnd   = NULL;
HWND   CMatroxCommon::m_hWndForInspectionResult   = NULL;
bool   CMatroxCommon::m_bMainInitialFinished   = false;
bool   CMatroxCommon::m_bThroughFlg   = false;
int   CMatroxCommon::m_iNowColor   = 0;
double   CMatroxCommon::m_dNowMag   = 1.0;
double   CMatroxCommon::m_dNowMagForInspectionResult   = 1.0;
bool   CMatroxCommon::m_bNowDiffMode   = false;
double   CMatroxCommon::m_dFrameRate = 10.0;
bool   CMatroxCommon::m_bFatalErrorOccured = false;
string CMatroxCommon::m_strIPAddress = "";
int   CMatroxCommon::m_iEachLightPatternMatching = 0;
string CMatroxCommon::m_strDebugFileIdentifiedName = "";
std::list< MIL_ID > CMatroxCommon::m_lstImageGrab;

CMatroxCommon::CMatroxCommon(void)
	:m_bDebugON(true)
	, m_strDebugFolder(".\\DebugFile\\")
{
}

CMatroxCommon::~CMatroxCommon(void)
{

}
/*------------------------------------------------------------------------------------------
	1.日本語名
		マトロックスの基本的な初期化

	2.パラメタ説明
		nhDispHandle (IN)		ディスプレイハンドル (メインアプリが使用するハンドル)
		
	3.概要
		マトロックスの基本的な初期化

	4.機能説明
		マトロックスの基本的な初期化

	5.戻り値
		0  :	正常に初期化完了
		-1 :    初期化エラー
		-99:    例外エラーにより初期化エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::Initial( HWND nhDispHandle, string nstrSettingPath )
{
	int i_loop;
	SYSTEM_INFO SystemInfo = { 0 };
	bool b_32bitSoft_on_64bitOS;

	//	パラメータファイルを読み込む
	readParameter( nstrSettingPath );

	//	アプリケーションID取得
	MappAlloc( M_DEFAULT, &m_milApp );
	if (m_milApp == M_NULL)
	{
		return -1;
	}

	//　エラーメッセージを出さないようにする
	MappControl(M_ERROR, M_PRINT_DISABLE);
	//	エラーフック関数登録
	MappHookFunction(M_ERROR_CURRENT, (MIL_APP_HOOK_FUNCTION_PTR)&hookErrorHandler, this);

	//	OSが64bitで、アプリケーションが32bitであるかどうかチェック
	GetNativeSystemInfo(&SystemInfo);
	if (SystemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 && sizeof(void *) == 4)
	{
		b_32bitSoft_on_64bitOS = true;
	}
	else
	{
		b_32bitSoft_on_64bitOS = false;
	}

	//	システムID取得
	switch( m_iBoardType )
	{
		case MTX_MORPHIS:
			MsysAlloc( M_SYSTEM_MORPHIS, M_DEV0, M_DEFAULT, &m_milSys );
			break;
		case MTX_SOLIOSXCL:
		case MTX_SOLIOSXA:
			MsysAlloc( M_SYSTEM_SOLIOS, M_DEV0, M_DEFAULT, &m_milSys );
			break;
		case MTX_METEOR2MC:
#ifndef MIL10
			MsysAlloc( M_SYSTEM_METEOR_II, M_DEV0, M_DEFAULT, &m_milSys );
#endif
			break;
		case MTX_GIGE:
#ifdef MIL10
			if (b_32bitSoft_on_64bitOS == true)
			{
				MsysAlloc(MIL_TEXT("dmiltcp:\\\\localhost\\M_SYSTEM_GIGE_VISION"), M_DEV0, M_DEFAULT, &m_milSys);
			}
			else
			{
				MsysAlloc(M_SYSTEM_GIGE_VISION, M_DEV0, M_DEFAULT, &m_milSys);
			}
#endif
			break;
		case MTX_HOST:
			MsysAlloc( M_SYSTEM_HOST, M_DEV0, M_DEFAULT, &m_milSys );
			break;
		default:
			return -1;
	}
	if (m_milSys == M_NULL)
	{
		return -1;
	}

	//	ディスプレイID取得
#ifdef MIL10
	MdispAlloc( m_milSys, M_DEFAULT, MIL_TEXT("M_DEFAULT"), M_DEFAULT, &m_milDisp );
	if (m_milDisp == M_NULL)
	{
		return -1;
	}
	MdispAlloc( m_milSys, M_DEFAULT, MIL_TEXT("M_DEFAULT"), M_DEFAULT, &m_milDispForInspectionResult );
	if (m_milDispForInspectionResult == M_NULL)
	{
		return -1;
	}
#else
	MdispAlloc( m_milSys, M_DEFAULT, M_DISPLAY_SETUP, M_DEFAULT, &m_milDisp );
	MdispAlloc( m_milSys, M_DEFAULT, M_DISPLAY_SETUP, M_DEFAULT, &m_milDispForInspectionResult );
#endif

	if( m_iBoardType != MTX_HOST )
	{
		//	デジタイザID取得
#ifdef MIL10
		if (m_strIPAddress != "")
		{
			MdigAlloc(m_milSys, M_GC_CAMERA_ID(MT(LPCTSTR(m_strIPAddress.c_str()))), m_strCameraFilePath.c_str(), M_GC_DEVICE_IP_ADDRESS, &m_milDigitizer);
		}
		else
		{
			MdigAlloc( m_milSys,  M_DEV0, m_strCameraFilePath.c_str(), M_DEFAULT, &m_milDigitizer );
		}
#else
		MdigAlloc( m_milSys,  M_DEV0, (MIL_TEXT_PTR)m_strCameraFilePath.c_str(), M_DEFAULT, &m_milDigitizer );
#endif
		if (m_milDigitizer == M_NULL)
		{
			return -1;
		}
	}
	//	表示用画像バッファ
	if( m_iBoardType != MTX_HOST )
	{
		MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_GRAB + M_DISP + M_PACKED + M_BGR24, &m_milShowImage );
	}
	else
	{
		//	M_GRABは外す
		MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milShowImage );
	}
	//	モノクロ画像バッファ
	MbufAlloc2d(m_milSys, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC, &m_milMonoImage );	
	//	オリジナル画像バッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milOriginalImage );
	//	表示用画像バッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milPreShowImage );
	//	差分用オリジナル画像バッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milDiffOrgImage );
	//	平均化用積算画像バッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 16+M_UNSIGNED, M_IMAGE+M_PROC, &m_milAverageImageCalc );
	//検査結果表示用画像バッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milInspectionResultImage );
	//グラフィックを画像に保存するためのバッファ
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milDraphicSaveImage );
	
	if( m_iBoardType != MTX_HOST )
	{
		for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
		{
			MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_GRAB+M_PROC, &m_milImageGrab[i_loop]);
		}
		for( i_loop = 0; i_loop < MAX_AVERAGE_IMAGE_GRAB_NUM; i_loop++ )
		{
			MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_GRAB+M_PROC, &m_milAverageImageGrab[i_loop]);
		}
	}

	//	グラフィックバッファ
	MgraAlloc( m_milSys, &m_milGraphic );

	if( m_iBoardType != MTX_HOST )
	{
		//　取込オフセットを0にする
		MdigControl( m_milDigitizer, M_SOURCE_OFFSET_X, 0 );
		MdigControl( m_milDigitizer, M_SOURCE_OFFSET_Y, 0 );
		//　取込幅をデフォルトの取込幅にする
		MdigControl( m_milDigitizer, M_SOURCE_SIZE_X, ( double )m_szImageSize.cx );
		MdigControl( m_milDigitizer, M_SOURCE_SIZE_Y, ( double )m_szImageSize.cy );
	}

	//	画像バッファクリア
	MbufClear( m_milShowImage, 0 );
	MbufClear( m_milMonoImage, 0 );
	MbufClear( m_milOriginalImage, 0 );

	if( m_iBoardType != MTX_HOST )
	{
		for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
		{
			MbufClear( m_milImageGrab[i_loop], 255 );
		}
	}

	MdispControl(m_milDisp, M_INTERPOLATION_MODE, M_NEAREST_NEIGHBOR);
	MdispControl(m_milDisp, M_OVERLAY, M_ENABLE);
	MdispControl(m_milDisp, M_OVERLAY_SHOW, M_ENABLE );
	MdispControl(m_milDisp, (MIL_INT64)M_TRANSPARENT_COLOR, (MIL_DOUBLE)TRANSPARENT_COLOR);

	MdispControl(m_milDispForInspectionResult, M_INTERPOLATION_MODE, M_NEAREST_NEIGHBOR);
	MdispControl(m_milDispForInspectionResult, M_OVERLAY, M_ENABLE);
	MdispControl(m_milDispForInspectionResult, M_OVERLAY_SHOW, M_ENABLE );
	MdispControl(m_milDispForInspectionResult, (MIL_INT64)M_TRANSPARENT_COLOR, (MIL_DOUBLE)TRANSPARENT_COLOR);

	MdispControl(m_milDispForInspectionResult, M_UPDATE, M_DISABLE);

	MdispSelectWindow( m_milDisp, m_milShowImage, nhDispHandle );
	//MdispZoom( m_milDisp, 0.1, 0.1 );
	MdispControl( m_milDisp, M_CENTER_DISPLAY, M_ENABLE );	// センターに表示
	MdispControl( m_milDisp, M_SCALE_DISPLAY, M_ENABLE );	// ストレッチして表示
	
	//	オーバーレイバッファ
	MdispInquire( m_milDisp, M_OVERLAY_ID, &m_milOverLay );

	m_szImageSizeForCamera = m_szImageSize;

#ifdef MIL10
	//for only gigE camera
	if( m_iBoardType == MTX_GIGE )
	{
		MIL_TEXT_PTR str_vendor_name;
		str_vendor_name = new MIL_TEXT_CHAR[256];
		MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

		// ********** Sonyカメラの対応が未設定ではあるが、ここでの処理は重要ではないためすっ飛ばす	2021/03/12 ***************************************
		//	Basler initial
		//if( strstr(str_vendor_name,"Basler") != NULL )
		//{
		//	MIL_INT32 i_gain_raw = 301;
		//	MIL_DOUBLE d_frame_rate_abs = m_dFrameRate;
		//	MIL_DOUBLE d_exposure_time_abs = (1.0 / m_dFrameRate)*1.0e6 - 100.0;
		//	MIL_BOOL b_acquition_frame_rate_enable = M_TRUE;
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,MIL_TEXT("GainAuto"),M_TYPE_ENUMERATION,MIL_TEXT("Off"));
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,MIL_TEXT("ExposureAuto"),M_TYPE_ENUMERATION,MIL_TEXT("Off"));
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("AcquisitionFrameRateEnable"),M_TYPE_BOOLEAN,&b_acquition_frame_rate_enable);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_raw);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("AcquisitionFrameRateAbs"),M_TYPE_DOUBLE ,&d_frame_rate_abs);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTimeAbs"),M_TYPE_DOUBLE ,&d_exposure_time_abs);
		//}
		////	Point grey initial
		//else
		//{
		//	MIL_DOUBLE d_blackLevel = 0.0;
		//	MIL_DOUBLE d_gain = 00.0;
		//	MIL_DOUBLE d_frame_rate = m_dFrameRate;
		//	MIL_DOUBLE d_exposure_time = (1.0 / m_dFrameRate)*1.0e6 - 1000.0;
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,MIL_TEXT("GainAuto"),M_TYPE_ENUMERATION,MIL_TEXT("Off"));
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,MIL_TEXT("ExposureAuto"),M_TYPE_ENUMERATION,MIL_TEXT("Off"));
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,MIL_TEXT("AcquisitionFrameRateAuto"),M_TYPE_ENUMERATION,MIL_TEXT("Off"));
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("BlackLevel"),M_TYPE_DOUBLE ,&d_blackLevel);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("Gain"),M_TYPE_DOUBLE ,&d_gain);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("AcquisitionFrameRate"),M_TYPE_DOUBLE ,&d_frame_rate);
		//	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTime"),M_TYPE_DOUBLE ,&d_exposure_time);

		//	MdigControlFeature(m_milDigitizer, M_FEATURE_VALUE, MIL_TEXT("PixelFormat"), M_TYPE_STRING, MIL_TEXT("Mono8"));

		//}
		delete [] str_vendor_name;
	}
#endif


	m_hWnd = nhDispHandle;

	m_bMainInitialFinished = true;

	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		パラメータファイルを読み込む

	2.パラメタ説明
		
	3.概要
		パラメータファイルを読み込む

	4.機能説明
		パラメータファイルを読み込む

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::readParameter(string nstrSettingPath)
{
	string cs_ini_file;
	char buff[256];

	//	iniファイルのパス
	if( nstrSettingPath == "" )
	{
		cs_ini_file = "PRM\\ImageProcess.ini";
	}
	else
	{
		cs_ini_file = nstrSettingPath + "\\ImageProcess.ini";
	}

	//	DCFファイルパス
	GetPrivateProfileString( "Matrox", "CameraFile", "0", buff, 256, cs_ini_file.c_str() );
	m_strCameraFilePath = buff;
	m_strCameraFilePath = "PRM\\" + m_strCameraFilePath;

	//	画像サイズ
	GetPrivateProfileString( "Matrox", "sizeX", "800", buff, 256, cs_ini_file.c_str() );
	m_szImageSize.cx = atoi( buff );
	GetPrivateProfileString( "Matrox", "sizeY", "640", buff, 256, cs_ini_file.c_str() );
	m_szImageSize.cy = atoi( buff );

	//	使用画像ボード
	GetPrivateProfileString( "Matrox", "BoardType", "0", buff, 256, cs_ini_file.c_str() );
	m_iBoardType = atoi( buff );

	//	フレームレート(FPS)
	GetPrivateProfileString("Matrox", "FrameRate", "10", buff, 256, cs_ini_file.c_str());
	m_dFrameRate = atof(buff);
	if (m_dFrameRate <= 0.0)
	{
		m_dFrameRate = 10.0;
	}

	//  カメラのIPアドレス
	GetPrivateProfileString("Matrox", "CameraIPAddress", "", buff, 256, cs_ini_file.c_str());
	m_strIPAddress = buff;

	//	照明毎にパターンマッチングを行うか否か
	GetPrivateProfileString("Matrox", "EachLightPatternMatchingEnable", "0", buff, 256, cs_ini_file.c_str());
	m_iEachLightPatternMatching = atoi(buff);
	if (m_iEachLightPatternMatching != 1)
	{
		m_iEachLightPatternMatching = 0;
	}

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		マトロックスの終了処理

	2.パラメタ説明
		
	3.概要
		マトロックスの終了処理

	4.機能説明
		マトロックスの終了処理

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::CloseMatrox()
{
	int i_loop;

	if( m_milShowImage != M_NULL )
	{
		MbufFree(m_milShowImage);
		m_milShowImage = M_NULL;
	}
	if( m_milMonoImage != M_NULL )
	{
		MbufFree(m_milMonoImage);
		m_milMonoImage = M_NULL;
	}
	if( m_milOriginalImage != M_NULL )
	{
		MbufFree(m_milOriginalImage);
		m_milOriginalImage = M_NULL;
	}

	for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
	{
		if( m_milImageGrab[i_loop] != M_NULL )
		{
			MbufFree(m_milImageGrab[i_loop]);
			m_milImageGrab[i_loop] = M_NULL;
		}
	}
	for( i_loop = 0; i_loop < MAX_AVERAGE_IMAGE_GRAB_NUM; i_loop++ )
	{
		if( m_milAverageImageGrab[i_loop] != M_NULL )
		{
			MbufFree(m_milAverageImageGrab[i_loop]);
			m_milAverageImageGrab[i_loop] = M_NULL;
		}
	}
	if( m_milPreShowImage != M_NULL )
	{
		MbufFree(m_milPreShowImage);
		m_milPreShowImage = M_NULL;
	}
	if( m_milDiffOrgImage != M_NULL )
	{
		MbufFree(m_milDiffOrgImage);
		m_milDiffOrgImage = M_NULL;
	}
	if( m_milAverageImageCalc != M_NULL )
	{
		MbufFree(m_milAverageImageCalc);
		m_milAverageImageCalc = M_NULL;
	}
	if( m_milInspectionResultImage != M_NULL )
	{
		MbufFree(m_milInspectionResultImage);
		m_milInspectionResultImage = M_NULL;
	}
	if( m_milDraphicSaveImage != M_NULL )
	{
		MbufFree(m_milDraphicSaveImage);
		m_milDraphicSaveImage = M_NULL;
	}

	if( m_milGraphic != M_NULL )
	{
		MgraFree( m_milGraphic );
		m_milGraphic = M_NULL;
	}
	if( m_milDisp != M_NULL )
	{
		MdispFree(m_milDisp);
		m_milDisp = M_NULL;
	}
	if( m_milDispForInspectionResult != M_NULL )
	{
		MdispFree(m_milDispForInspectionResult);
		m_milDispForInspectionResult = M_NULL;
	}

	if( m_iBoardType != MTX_HOST )
	{
		if( m_milDigitizer != M_NULL )
		{
			MdigFree(m_milDigitizer);
			m_milDigitizer = M_NULL;
		}
	}
	//	エラーフックを解除
	MappHookFunction(M_ERROR_CURRENT + M_UNHOOK, (MIL_APP_HOOK_FUNCTION_PTR)&hookErrorHandler, this);

	if( m_milSys != M_NULL )
	{
		MsysFree(m_milSys);
		m_milSys = M_NULL;
	}

	if( m_milApp != M_NULL )
	{
		MappFree(m_milApp);
		m_milApp = M_NULL;
	}

	m_bMainInitialFinished = false;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		画像バッファを再設定する
	2.パラメタ説明
		nszNewImageSize		新しい画像のサイズ

	3.概要
		画像バッファを再設定する

	4.機能説明
		画像バッファを再設定する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::reallocMilImage( SIZE nszNewImageSize )
{
	int i_loop;

	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//	まず画像バッファを全て開放する
	if( m_milShowImage != M_NULL )
	{
		MbufFree(m_milShowImage);
		m_milShowImage = M_NULL;
	}
	if( m_milMonoImage != M_NULL )
	{
		MbufFree(m_milMonoImage);
		m_milMonoImage = M_NULL;
	}
	if( m_milOriginalImage != M_NULL )
	{
		MbufFree(m_milOriginalImage);
		m_milOriginalImage = M_NULL;
	}

	for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
	{
		if( m_milImageGrab[i_loop] != M_NULL )
		{
			MbufFree(m_milImageGrab[i_loop]);
			m_milImageGrab[i_loop] = M_NULL;
		}
	}
	if( m_milPreShowImage != M_NULL )
	{
		MbufFree(m_milPreShowImage);
		m_milPreShowImage = M_NULL;
	}
	if( m_milDiffOrgImage != M_NULL )
	{
		MbufFree(m_milDiffOrgImage);
		m_milDiffOrgImage = M_NULL;
	}
	if( m_milDraphicSaveImage != M_NULL )
	{
		MbufFree(m_milDraphicSaveImage);
		m_milDraphicSaveImage = M_NULL;
	}
//	if( m_milInspectionResultImage != M_NULL )
//	{
//		MbufFree(m_milInspectionResultImage);
//		m_milInspectionResultImage = M_NULL;
//	}

	//	表示用画像バッファ
	if( m_iBoardType != MTX_HOST )
	{
		MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_GRAB + M_DISP + M_PACKED + M_BGR24, &m_milShowImage );
	}
	else
	{
		//	GRABを外す
		MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milShowImage );
	}

	//	モノクロ画像バッファ
	MbufAlloc2d(m_milSys, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC, &m_milMonoImage );	
	//	オリジナル画像バッファ
	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milOriginalImage );
	//	表示用画像バッファ
	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milPreShowImage );
	//	差分用オリジナル画像バッファ
	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_PACKED + M_BGR24, &m_milDiffOrgImage );
	if( m_iBoardType != MTX_HOST )
	{
		for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
		{
			MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_GRAB+M_PROC, &m_milImageGrab[i_loop]);
		}
	}
	//グラフィックを画像に保存するためのバッファ
	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milDraphicSaveImage );
//	//検査結果表示用画像バッファ
//	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &m_milInspectionResultImage );

	MdispSelectWindow( m_milDisp, m_milShowImage, m_hWnd );
	MdispInquire( m_milDisp, M_OVERLAY_ID, &m_milOverLay );

//	if( m_hWndForInspectionResult != NULL )
//	{
//		MdispSelectWindow( m_milDispForInspectionResult, m_milInspectionResultImage, m_hWndForInspectionResult );
//		MdispInquire( m_milDispForInspectionResult, M_OVERLAY_ID, &m_milOverLayForInspectionResult );
//	}

	m_szImageSize = nszNewImageSize;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		表示バッファの倍率を設定する

	2.パラメタ説明
		ndMag	設定倍率
		
	3.概要
		表示バッファの倍率を設定する

	4.機能説明
		表示バッファの倍率を設定する

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::setShowImageMag( double ndMag )
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//	制限を設けておく。とりあえず0.2倍から8倍
	if( ndMag >= 0.2 && ndMag <= 10.0 )
	{
		m_dNowMag = ndMag;
		MdispZoom( m_milDisp, ndMag, ndMag );
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		デジタルズーム倍率を取得する

	2.パラメタ説明


	3.概要
		デジタルズーム倍率を取得する

	4.機能説明
		デジタルズーム倍率を取得する

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CMatroxCommon::getZoomMag()
{
	return m_dNowMag;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		指定のエリアの画像を保存する

	2.パラメタ説明
		nrctSaveArea
		nAllSaveFlg

	3.概要
		指定のエリアの画像を保存する

	4.機能説明
		指定のエリアの画像を保存する

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::saveImage( RECT nrctSaveArea, BOOL nAllSaveFlg, string ncsFilePath, BOOL nbSaveMono )
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//	全画面保存
	if( nAllSaveFlg == TRUE )
	{
#ifdef MIL10
		if( nbSaveMono == FALSE)
		{
			MbufExport( ncsFilePath.c_str(), M_BMP, m_milShowImage );
		}
		else
		{
			MIL_ID	mil_save_image;
			MbufAllocColor(m_milSys, 1, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC, &mil_save_image );
			MbufCopy( m_milShowImage, mil_save_image );
			MbufExport( ncsFilePath.c_str(), M_BMP, mil_save_image );
			MbufFree( mil_save_image );
		}
#else
		MbufExport( (MIL_TEXT_PTR)ncsFilePath.c_str(), M_BMP, m_milShowImage );
#endif
	}
	//	指定のエリアを保存
	else
	{
		SIZE sz_image_size;
		MIL_ID	mil_area_image;

		//	画像サイズ取得
		sz_image_size.cx = (long)MbufInquire( m_milShowImage,M_SIZE_X, M_NULL );
		sz_image_size.cy = (long)MbufInquire( m_milShowImage,M_SIZE_Y, M_NULL );

		//	指定された位置に画像がない場合は0を返す
		if( nrctSaveArea.left < 0 || nrctSaveArea.top < 0 || nrctSaveArea.right > sz_image_size.cx || nrctSaveArea.bottom > sz_image_size.cy ||
		( nrctSaveArea.right - nrctSaveArea.left )== 0 || ( nrctSaveArea.bottom - nrctSaveArea.top )== 0 )
		{
			return;
		}

		//	指定のエリアの画像バッファを抽出する
		//	モデル画像バッファのメモリを確保する
		MbufAllocColor( m_milSys , 3, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
						8 + M_UNSIGNED, M_IMAGE + M_PROC + M_PACKED + M_BGR24, &mil_area_image );
		//	原画像からモデル画像を取得する
		MbufTransfer( m_milShowImage, mil_area_image, nrctSaveArea.left, nrctSaveArea.top, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, 
					M_DEFAULT, 0, 0, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, M_DEFAULT,
					M_COPY, M_DEFAULT, M_NULL, M_NULL );

		//	画像保存
#ifdef MIL10
		if( nbSaveMono == FALSE)
		{
			MbufExport( ncsFilePath.c_str(), M_BMP, mil_area_image );
		}
		else
		{
			MIL_ID	mil_save_image;
			MbufAllocColor(m_milSys, 1, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, 8+M_UNSIGNED, M_IMAGE+M_PROC, &mil_save_image );
			MbufCopy( mil_area_image, mil_save_image );
			MbufExport( ncsFilePath.c_str(), M_BMP, mil_save_image );
			MbufFree( mil_save_image );
		}
#else
		MbufExport( (MIL_TEXT_PTR)ncsFilePath.c_str(), M_BMP, mil_area_image );
#endif
		MbufFree( mil_area_image );
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		オリジナル画像を保存する

	2.パラメタ説明
		nrctSaveArea
		nAllSaveFlg

	3.概要
		オリジナル画像を保存する

	4.機能説明
		オリジナル画像を保存する

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::saveOrigImage( RECT nrctSaveArea, BOOL nAllSaveFlg, string ncsFilePath )
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//	全画面保存
	if( nAllSaveFlg == TRUE )
	{
#ifdef MIL10
		MbufExport( ncsFilePath.c_str(), M_BMP, m_milOriginalImage );
#else
		MbufExport( (MIL_TEXT_PTR)ncsFilePath.c_str(), M_BMP, m_milOriginalImage );
#endif
	}
	//	指定のエリアを保存
	else
	{
		SIZE sz_image_size;
		MIL_ID	mil_area_image;

		//	画像サイズ取得
		sz_image_size.cx = (long)MbufInquire( m_milOriginalImage,M_SIZE_X, M_NULL );
		sz_image_size.cy = (long)MbufInquire( m_milOriginalImage,M_SIZE_Y, M_NULL );

		//	指定された位置に画像がない場合は0を返す
		if( nrctSaveArea.left < 0 || nrctSaveArea.top < 0 || nrctSaveArea.right > sz_image_size.cx || nrctSaveArea.bottom > sz_image_size.cy ||
		( nrctSaveArea.right - nrctSaveArea.left )== 0 || ( nrctSaveArea.bottom - nrctSaveArea.top )== 0 )
		{
			return;
		}

		//	指定のエリアの画像バッファを抽出する
		//	モデル画像バッファのメモリを確保する
		MbufAllocColor( m_milSys , 3, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
						8 + M_UNSIGNED, M_IMAGE + M_PROC + M_PACKED + M_BGR24, &mil_area_image );
		//	原画像からモデル画像を取得する
		MbufTransfer( m_milOriginalImage, mil_area_image, nrctSaveArea.left, nrctSaveArea.top, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, 
					M_DEFAULT, 0, 0, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, M_DEFAULT,
					M_COPY, M_DEFAULT, M_NULL, M_NULL );

		//	画像保存
#ifdef MIL10
		MbufExport( ncsFilePath.c_str(), M_BMP, mil_area_image );
#else
		MbufExport( (MIL_TEXT_PTR)ncsFilePath.c_str(), M_BMP, mil_area_image );
#endif
		MbufFree( mil_area_image );
	}
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
		true:スルー中
		false:フリーズ中
	6.備考
		なし
------------------------------------------------------------------------------------------*/
bool CMatroxCommon::getThoughStatus()
{
	return m_bThroughFlg;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		フリーズ直後の画像に戻す

	2.パラメタ説明
		なし

	3.概要
		フリーズ直後の画像に戻す

	4.機能説明
		フリーズ直後の画像に戻す

	5.戻り値
		true:スルー中
		false:フリーズ中

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::setOriginImage()
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	MbufCopy( m_milOriginalImage, m_milShowImage );
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
		画像サイズ

	6.備考
		なし
------------------------------------------------------------------------------------------*/
SIZE CMatroxCommon::getImageSize()
{
	return m_szImageSize;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		画像をロードする
	2.パラメタ説明
		ncsFilePath		ファイルパス

	3.概要
		画像をロードする

	4.機能説明
		画像をロードする

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::loadImage( string ncsFilePath )
{
	int		i_loop;
	MIL_ID	mil_image;
	SIZE	sz_image_size;

	if( m_bMainInitialFinished == false )
	{
		return -1;
	}

	//	モデル画像を読み込む
#ifdef MIL10
	MbufRestore( ncsFilePath.c_str(), m_milSys, &mil_image );
#else
	MbufRestore( (MIL_TEXT_PTR)ncsFilePath.c_str(), m_milSys, &mil_image );
#endif
	//	モデル画像サイズ取得
	sz_image_size.cx = (long)MbufInquire( mil_image, M_SIZE_X, M_NULL );
	sz_image_size.cy = (long)MbufInquire( mil_image, M_SIZE_Y, M_NULL );	

	//	メモリの再設定
	reallocMilImage( sz_image_size );

	MbufCopy(mil_image, m_milShowImage);
	MbufCopy(mil_image, m_milMonoImage);
	MbufCopy(mil_image, m_milOriginalImage);
	MbufCopy(mil_image, m_milPreShowImage);
	if( m_iBoardType != MTX_HOST )
	{
		for( i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++ )
		{
			MbufCopy(mil_image, m_milImageGrab[i_loop]);
		}
	}
	//	メモリ解放
	MbufFree( mil_image );

	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		色変換を行う

	2.パラメタ説明
		niConversionType : 0 RGB	1 HSI
		niPlane RGB,R,G,B   H,S,L
		
	3.概要
		色変換を行う

	4.機能説明
		色変換を行う。
		色を変更して、画像処理後に色を再び変更するとオリジナル画像に戻ってしまう。今は

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::setColorImage( int niConversionType, int niPlane )
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//　表示用バッファをクリアする
	MbufClear( m_milShowImage ,M_RGB888( 0, 0, 0 ) );

	//	RGBの場合
	if( niConversionType == 0 )
	{
		switch(niPlane)
		{
			case RGB:
				//	RGBのときはオリジナル画像に戻す。画像処理バッファはR固定
				MbufCopy( m_milOriginalImage, m_milShowImage );
				MbufCopyColor(  m_milOriginalImage, m_milMonoImage, M_RED );
				break;
			case R:
				MbufCopyColor(  m_milOriginalImage, m_milMonoImage, M_RED );
				MbufCopy( m_milMonoImage, m_milShowImage );
				break;
			case G:
				MbufCopyColor(  m_milOriginalImage, m_milMonoImage, M_GREEN );
				MbufCopy( m_milMonoImage, m_milShowImage );
				break;
			case B:
				MbufCopyColor(  m_milOriginalImage, m_milMonoImage, M_BLUE );
				MbufCopy( m_milMonoImage, m_milShowImage );
				break;
			default:
				break;
		}

		m_iNowColor = niPlane;
	}
	// HSIの場合
	else if( niConversionType == 1 )
	{
		//HSIに変換
		MimConvert( m_milShowImage, m_milImageGrab[1], M_RGB_TO_HLS );

		switch(niPlane)
		{
			case H:
				MbufCopyColor( m_milImageGrab[1], m_milMonoImage, M_HUE );
				MbufCopyColor( m_milMonoImage, m_milShowImage, M_ALL_BANDS );
				break;
			case S:
				MbufCopyColor( m_milImageGrab[1], m_milMonoImage, M_SATURATION );
				MbufCopyColor( m_milMonoImage, m_milShowImage, M_ALL_BANDS );
				break;
			case I:
				MbufCopyColor( m_milImageGrab[1], m_milMonoImage, M_LUMINANCE );
				MbufCopyColor( m_milMonoImage, m_milShowImage, M_ALL_BANDS );
				break;
			default:
				break;
		}
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		接続されたカメラがカラーカメラかどうか調べる

	2.パラメタ説明
		なし

	3.概要
		接続されたカメラがカラーカメラかどうか調べる

	4.機能説明
		接続されたカメラがカラーカメラかどうか調べる

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::getColorCameraInfo()
{
	long l_ret;

	if( m_bMainInitialFinished == false )
	{
		return -1;
	}

	if( m_iBoardType == MTX_HOST )
	{
		return( 0 );
	}
	//  カラー情報を取得する
	l_ret = (long)MdigInquire( m_milDigitizer, M_COLOR_MODE, M_NULL );

	switch( l_ret )
	{
		//　モノクロ
		case M_MONO8_VIA_RGB:
		case M_MONOCHROME:
			return( 0 );
		//　カラー
		case M_COMPOSITE:
		case M_EXTERNAL_CHROMINANCE:
		case M_RGB:
			return( 1 );
		default:
			return( 0 );
	}

}
/*------------------------------------------------------------------------------------------
	1.日本語名
		指定の位置の輝度値RGBを取得する

	2.パラメタ説明


	3.概要
		指定の位置の輝度値RGBを取得する

	4.機能説明
		指定の位置の輝度値RGBを取得する

	5.戻り値
		0: 指定された位置に画像が存在した
		-1:指定された位置に画像が存在しない
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::getPixelValueOnPosition( POINT nNowPoint, int &RValue, int &GValue,int &BValue )
{
	BYTE bt_pixel_value[3] = {0};
	SIZE sz_image_size;
	int i_ret = -1;

	if( m_bMainInitialFinished == false )
	{
		return -1;
	}

	//	画像サイズ取得
	sz_image_size.cx = (long)MbufInquire( m_milShowImage,M_SIZE_X, M_NULL );
	sz_image_size.cy = (long)MbufInquire( m_milShowImage,M_SIZE_Y, M_NULL );

	//	指定された位置に画像がない場合は0を返す
	if( nNowPoint.x >= 0 && nNowPoint.x < sz_image_size.cx && nNowPoint.y >= 0 && nNowPoint.y < sz_image_size.cy )
	{
		MbufGet2d( m_milShowImage, nNowPoint.x, nNowPoint.y, 1, 1,bt_pixel_value );
		i_ret = 0;
	}
	RValue = ( int )bt_pixel_value[0];
	GValue = ( int )bt_pixel_value[1];
	BValue = ( int )bt_pixel_value[2];

	return i_ret;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		差分用オリジナル画像を現在表示されている画像で登録する

	2.パラメタ説明


	3.概要
		差分用オリジナル画像を現在表示されている画像で登録する

	4.機能説明
		差分用オリジナル画像を現在表示されている画像で登録する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::setDiffOrgImage()
{
	MbufCopy(m_milShowImage, m_milDiffOrgImage);
	m_bNowDiffMode = true;
	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		差分モードを終了する

	2.パラメタ説明


	3.概要
		差分モードを終了する

	4.機能説明
		差分モードを終了する

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::resetDiffMode()
{
	m_bNowDiffMode = false;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		検査結果表示用ウインドウのハンドルをセットする

	2.パラメタ説明


	3.概要
		検査結果表示用ウインドウのハンドルをセットする

	4.機能説明
		検査結果表示用ウインドウのハンドルをセットする

	5.戻り値
		0:セット成功
		-1:失敗(既にセットしてある)
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::setDispHandleForInspectionResult( HWND nhDispHandleForInspectionResult )
{
	if( m_hWndForInspectionResult == NULL )
	{
		MbufClear( m_milInspectionResultImage, CLEAR_IMAGE_BUFFER );
		m_hWndForInspectionResult = nhDispHandleForInspectionResult;
		MdispSelectWindow( m_milDispForInspectionResult, m_milInspectionResultImage, m_hWndForInspectionResult );
		//	オーバーレイバッファ
		MdispInquire( m_milDispForInspectionResult, M_OVERLAY_ID, &m_milOverLayForInspectionResult );
		return 0;
	}
	else
	{
		return -1;
	}
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		表示バッファの倍率を設定する(検査結果表示用)

	2.パラメタ説明
		ndMag	設定倍率
		
	3.概要
		表示バッファの倍率を設定する(検査結果表示用)

	4.機能説明
		表示バッファの倍率を設定する(検査結果表示用)

	5.戻り値
		なし
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::setZoomMagForInspectionResult( double ndMag )
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}

	//	制限を設けておく。とりあえず0.2倍から10倍
	if( ndMag >= 0.2 && ndMag <= 10.0 )
	{
		m_dNowMagForInspectionResult = ndMag;
		MdispZoom( m_milDispForInspectionResult, ndMag, ndMag );
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		デジタルズーム倍率を取得する(検査結果表示用)

	2.パラメタ説明


	3.概要
		デジタルズーム倍率を取得する(検査結果表示用)

	4.機能説明
		デジタルズーム倍率を取得する(検査結果表示用)

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CMatroxCommon::getZoomMagForInspectionResult()
{
	return m_dNowMagForInspectionResult;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		検査結果画像を保存する

	2.パラメタ説明


	3.概要
		検査結果画像を保存する

	4.機能説明
		検査結果画像を保存する

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::saveInspectionResultImage(string ncsFilePath )
{
	MIL_ID mil_temp;
	MIL_ID mil_result_temp;
	int i_index;
	string str_ext;

	//	オーバーレイバッファと検査結果画像バッファの一時バッファを用意
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_temp );
	MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_result_temp );
	//	一時バッファに画像をコピー
	MbufCopy( m_milOverLayForInspectionResult, mil_temp );
//	MbufCopy( m_milInspectionResultImage, mil_result_temp );
	MbufCopy(m_milInspectionResultImageTemp, mil_result_temp);

	//	オーバーレイを検査結果画像上にコピー
//	MbufTransfer( mil_temp, mil_result_temp, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_COMPOSITION, M_DEFAULT, M_RGB888(1,1,1), M_NULL );
	MbufTransfer( m_milDraphicSaveImage, mil_result_temp, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_DEFAULT, M_COMPOSITION, M_DEFAULT, M_RGB888(1,1,1), M_NULL );

	//	ファイル出力

	//	拡張子を抽出
	i_index = (int)ncsFilePath.rfind(".");
	//	拡張子がない場合は仕方ないのでビットマップの拡張子をつけてビットマップで保存する
	if( i_index < 0 )
	{
		ncsFilePath += ".bmp";
		str_ext = "bmp";
	}
	else
	{
		//	ファイル名の最後の文字が「.」だった場合もビットマップにしてしまう
		if( i_index + 1 == ncsFilePath.length() )
		{
			ncsFilePath += "bmp";
			str_ext = "bmp";
		}
		else
		{
			str_ext = ncsFilePath.substr(i_index + 1);
		}
	}
	//	jpg
	if( str_ext.compare( "jpg" ) == 0 || str_ext.compare( "JPG" ) == 0 )
	{
		MbufExport( ncsFilePath.c_str(), M_JPEG_LOSSY, mil_result_temp );
	}
	//	png
	else if( str_ext.compare( "png" ) == 0 || str_ext.compare( "PNG" ) == 0 )
	{
		MbufExport( ncsFilePath.c_str(), M_PNG, mil_result_temp );
	}
	//	bmp
	else
	{
		MbufExport( ncsFilePath.c_str(), M_BMP, mil_result_temp );
	}

	//	メモリ開放
	MbufFree(mil_temp);
	MbufFree(mil_result_temp);
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		検査結果画像バッファをクリアする(描画もクリアする)

	2.パラメタ説明


	3.概要
		検査結果画像バッファをクリアする(描画もクリアする)

	4.機能説明
		検査結果画像バッファをクリアする(描画もクリアする)

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::ClearInspectionResultImage()
{
	MbufClear( m_milInspectionResultImage, CLEAR_IMAGE_BUFFER );
	MdispControl( m_milDispForInspectionResult, M_OVERLAY_CLEAR, M_TRANSPARENT_COLOR );

	//	画面更新をOFFに
	MdispControl(m_milDispForInspectionResult, M_UPDATE, M_DISABLE);
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
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCommon::getAveragePixelValueOnRegion( RECT nrctRegion )
{
	MIL_ID mil_region;
	MIL_ID mil_stat_result;
	MIL_INT i_average_value;

	//	矩形が適切な矩形かチェック
	if( ( nrctRegion.right - nrctRegion.left <= 1 ) || ( nrctRegion.bottom - nrctRegion.top <= 1 ) )
	{
		//	小さすぎたらエラー
		return -1;
	}

	//	メモリ確保
	MbufAlloc2d(m_milSys, nrctRegion.right - nrctRegion.left, nrctRegion.bottom - nrctRegion.top, 8+M_UNSIGNED, M_IMAGE+M_PROC, &mil_region );	
	//矩形領域を抽出
	MbufTransfer( m_milMonoImage, mil_region, nrctRegion.left, nrctRegion.top, (nrctRegion.right - nrctRegion.left), (nrctRegion.bottom - nrctRegion.top), M_DEFAULT, 0, 0, (nrctRegion.right - nrctRegion.left), (nrctRegion.bottom - nrctRegion.top), M_DEFAULT, M_COPY, M_DEFAULT, M_NULL, M_NULL );

	MimAllocResult( m_milSys, M_DEFAULT, M_STAT_LIST, &mil_stat_result);
	//領域の平均値を求める
	MimStat(mil_region, mil_stat_result, M_MEAN, M_NULL, M_NULL, M_NULL);
	MimGetResult(mil_stat_result,M_MEAN + M_TYPE_MIL_INT, &i_average_value);
	
	//	メモリ解放
	MimFree(mil_stat_result);
	MbufFree( mil_region );

	return (int)i_average_value;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		カメラ映像が映るディスプレイを切り替える

	2.パラメタ説明
		nhDispHandle		ディスプレイハンドル

	3.概要
		カメラ映像が映るディスプレイを切り替える

	4.機能説明
		カメラ映像が映るディスプレイを切り替える

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::SetDisplayHandle(HWND nhDispHandle)
{
	m_hWnd = nhDispHandle;
	MdispSelectWindow(m_milDisp, m_milShowImage, m_hWnd);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		エラーフック関数

	2.パラメタ説明


	3.概要
		エラーフック

	4.機能説明


	5.戻り値
		0

	6.備考
		なし
------------------------------------------------------------------------------------------*/
long MFTYPE CMatroxCommon::hookErrorHandler(long nlHookType, MIL_ID nEventId, void *npUserDataPtr)
{
	MIL_TEXT_CHAR		ErrorMessageFunction[M_ERROR_MESSAGE_SIZE] = MIL_TEXT("");
	MIL_TEXT_CHAR		ErrorMessage[M_ERROR_MESSAGE_SIZE] = MIL_TEXT("");

	MIL_TEXT_CHAR		ErrorSubMessage1[M_ERROR_MESSAGE_SIZE] = MIL_TEXT("");
	MIL_TEXT_CHAR		ErrorSubMessage2[M_ERROR_MESSAGE_SIZE] = MIL_TEXT("");
	MIL_TEXT_CHAR		ErrorSubMessage3[M_ERROR_MESSAGE_SIZE] = MIL_TEXT("");
	string str_error;
	string str_function;
	MIL_INT64  NbSubCode;

	CMatroxCommon *p_matrox_common;
	//	ポインタをキャスト
	p_matrox_common = (CMatroxCommon*)npUserDataPtr;

	try
	{
		//	エラー文字列を取得する

		//	エラー発生関数
		MappGetHookInfo(nEventId, M_MESSAGE + M_CURRENT_FCT, ErrorMessageFunction);
		//	エラー内容
		MappGetHookInfo(nEventId, M_MESSAGE + M_CURRENT, ErrorMessage);
		//	エラー内容詳細の文字列数
		MappGetHookInfo(nEventId, M_CURRENT_SUB_NB, (MIL_INT64*)&NbSubCode);

		//　エラー内容の詳細文字列を取得する
		if (NbSubCode > 2)
		{
			MappGetHookInfo(nEventId, M_CURRENT_SUB_3 + M_MESSAGE, ErrorSubMessage3);
		}
		if (NbSubCode > 1)
		{
			MappGetHookInfo(nEventId, M_CURRENT_SUB_2 + M_MESSAGE, ErrorSubMessage2);
		}
		if (NbSubCode > 0)
		{
			MappGetHookInfo(nEventId, M_CURRENT_SUB_1 + M_MESSAGE, ErrorSubMessage1);
		}

		//	ログに出力するエラー内容を作成する

		//	まずエラー発生関数
		str_error = "Function:(";
		str_error += ErrorMessageFunction;
		str_error += ") ";
		//	次にエラー内容
		str_error += ErrorMessage;
		str_error += " ";
		//	次に詳細内容
		if (NbSubCode > 2)
		{
			str_error += ErrorSubMessage3;
			str_error += " ";
		}
		if (NbSubCode > 1)
		{
			str_error += ErrorSubMessage2;
			str_error += " ";
		}
		if (NbSubCode > 0)
		{
			str_error += ErrorSubMessage1;
			str_error += " ";
		}
		//	エラーをログ出力する
		p_matrox_common->outputErrorLog(str_error);

		//	致命的なエラーかどうか判断する
		//	MdigProcess、xxxAllocで発生するエラーは全て致命的とする
		str_function = ErrorMessageFunction;
		//if (str_function.find("MdigProcess") != string::npos || str_function.find("Alloc") != string::npos)
		if (str_function.find("Alloc") != string::npos)
		{
			p_matrox_common->m_bFatalErrorOccured = true;
		}

		return(M_NULL);
	}
	catch (...)
	{
		//	エラーフックの例外エラー
		str_error = "Unknown Error";
		//	エラーをログ出力する
		p_matrox_common->outputErrorLog(str_error);

		return(M_NULL);
	}
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		エラーログを出力する

	2.パラメタ説明


	3.概要
		エラーログを出力する

	4.機能説明


	5.戻り値
		0

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::outputErrorLog(string nstrErrorLog)
{
	string str_file_name = "MILErrorLog.log";		//	ログファイルパス
	string str_time;								//	時刻
	string str_log_data;
	char chr_time[MAX_PATH];

	//	現在時刻の取得
	time_t tm_now = time(NULL);
	//	時刻を構造体に格納
	struct tm tm_st;
	localtime_s(&tm_st, &tm_now);
	//	時刻を文字列に
	sprintf_s(chr_time, "%d/%02d/%02d %02d:%02d:%02d ", tm_st.tm_year + 1900, tm_st.tm_mon + 1, tm_st.tm_mday,
		tm_st.tm_hour, tm_st.tm_min, tm_st.tm_sec);
	str_time = chr_time;
	//	出力する文字列を作成
	str_log_data = str_time + nstrErrorLog;

	//	ファイル書き込み
	ofstream write_file;
	write_file.open(str_file_name, ios::app);
	write_file << str_log_data << endl;

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		エラーログを出力する(関数名+エラーコード)

	2.パラメタ説明


	3.概要
		エラーログを出力する(関数名+エラーコード)

	4.機能説明


	5.戻り値
		0

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::outputErrorLog(string nstrFunctionName, int niErrorCode)
{
	char chr_log[MAX_PATH];

	sprintf_s(chr_log, "%s,%d", nstrFunctionName.c_str(),niErrorCode);
	outputErrorLog(chr_log);
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		致命的なエラーが起きたか否か取得する

	2.パラメタ説明


	3.概要
		致命的なエラーが起きたか否か取得する

	4.機能説明


	5.戻り値
		0

	6.備考
		なし
------------------------------------------------------------------------------------------*/
bool CMatroxCommon::getFatalErrorOccured()
{
	return m_bFatalErrorOccured;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		処理時間を出力

	2.パラメタ説明


	3.概要
		処理時間を出力

	4.機能説明


	5.戻り値
		0

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCommon::outputTimeLog(string nstrProcessName, double ndTime)
{
	string str_file_name = "MILTimeLog.log";		//	ログファイルパス
	string str_time;								//	時刻
	string str_log_data;
	char chr_time[MAX_PATH];
	bool b_debug = false;

	if (b_debug == false)
	{
		return;
	}

	//	現在時刻の取得
	time_t tm_now = time(NULL);
	//	時刻を構造体に格納
	struct tm tm_st;
	localtime_s(&tm_st, &tm_now);
	//	時刻を文字列に
	sprintf_s(chr_time, "%d/%02d/%02d %02d:%02d:%02d ", tm_st.tm_year + 1900, tm_st.tm_mon + 1, tm_st.tm_mday,
		tm_st.tm_hour, tm_st.tm_min, tm_st.tm_sec);
	str_time = chr_time;
	str_time = str_time + nstrProcessName + "\t";
	sprintf_s(chr_time, "%.3fms", ndTime);
	str_time = str_time + chr_time;

	//	出力する文字列を作成
	str_log_data = str_time;

	//	ファイル書き込み
	ofstream write_file;
	write_file.open(str_file_name, ios::app);
	write_file << str_log_data << endl;
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
int CMatroxCommon::getMonoBitmapData( const long nlArySize, BYTE *npbyteData )
{
	// 未初期化 又は 画像が無い場合
	if( false == m_bMainInitialFinished || 0 == m_lstImageGrab.size() )
	{
		return	-1;
	}

	// 画像ID取得
	MIL_ID	mil_image	= m_lstImageGrab.front();
	m_lstImageGrab.pop_front();

	// ユーザが準備した画像バッファサイズが少ない
	if( nlArySize < m_szImageSize.cx * m_szImageSize.cy )
	{
		return	-1;
	}

	// モノクロ画像バッファ確保
	MIL_ID	mil_mono;
	MbufAlloc2d( m_milSys, m_szImageSize.cx, m_szImageSize.cy, 8+M_UNSIGNED, M_IMAGE+M_PROC, &mil_mono );
	// モノクロ画像バッファクリア
	MbufClear( mil_mono, 0 );
	// モノクロ画像バッファコピー
	MbufCopy( mil_image, mil_mono );
	// モノクロ画像バッファ ビットマップデータ取得
	MbufGet2d( mil_mono, 0, 0, m_szImageSize.cx, m_szImageSize.cy, npbyteData );
	// メモリ解放
	MbufFree( mil_mono );

	return	0;
}
