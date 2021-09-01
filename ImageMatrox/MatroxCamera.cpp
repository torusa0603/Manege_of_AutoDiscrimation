#include "StdAfx.h"
#include "MatroxCamera.h"

CMatroxCamera::CMatroxCamera(void)
{
}

CMatroxCamera::~CMatroxCamera(void)
{
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		スルーを行う

	2.パラメタ説明
		なし
		
	3.概要
		スルーを行う

	4.機能説明
		スルーを行う

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::doThrough()
{
	if( m_bMainInitialFinished == false )
	{
		return -1;
	}

	if( m_bThroughFlg == false )
	{
		//	画像読み込み等でカメラのサイズと画像バッファのサイズが異なっている場合は、
		//	スルー前にカメラのサイズにバッファを合わせる
		if( m_szImageSizeForCamera.cx != m_szImageSize.cx || m_szImageSizeForCamera.cy != m_szImageSize.cy )
		{
			reallocMilImage( m_szImageSizeForCamera );
		}
		if( m_iBoardType != MTX_HOST )
		{
			//	フック関数を使用する
			MdigProcess(m_milDigitizer, m_milImageGrab, MAX_IMAGE_GRAB_NUM,
								M_START, M_DEFAULT, (MIL_DIG_HOOK_FUNCTION_PTR)&ProcessingFunction, this );
		}

		m_bThroughFlg = true;
	}

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		フリーズを行う

	2.パラメタ説明
		なし
		
	3.概要
		フリーズを行う

	4.機能説明
		フリーズを行う

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::doFreeze()
{
	if( m_bMainInitialFinished == false )
	{
		return -1;
	}

	if( m_bThroughFlg == true )
	{
		if( m_iBoardType != MTX_HOST )
		{
			//	フック関数を使用する
			MdigProcess(m_milDigitizer, m_milImageGrab,MAX_IMAGE_GRAB_NUM,
						M_STOP + M_WAIT, M_DEFAULT, (MIL_DIG_HOOK_FUNCTION_PTR)&ProcessingFunction, this );
		}

		m_bThroughFlg = false;
	}

	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		フック関数

	2.パラメタ説明
		なし
		
	3.概要
		画像のグラブを行う

	4.機能説明
		画像のグラブを行う

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
long MFTYPE	CMatroxCamera::ProcessingFunction( long nlHookType, MIL_ID nEventId, void *npUserDataPtr )
{
	CMatroxCamera *p_matrox;
	MIL_ID mil_modified_image;

	nlHookType = 0;
	//　送られてきたポインタをマトロックスクラスポインタにキャスティングする
	p_matrox = ( CMatroxCamera* )npUserDataPtr;
	//　変更されたバッファIDを取得する
	MdigGetHookInfo( nEventId, M_MODIFIED_BUFFER + M_BUFFER_ID, &mil_modified_image );

	MbufCopy( mil_modified_image, p_matrox->m_milOriginalImage );
	//	表示用画像バッファにコピーする
	if( p_matrox->IsDiffMode() == true )
	{
		p_matrox->makeDiffImage();
	}
	else
	{
		MbufCopy( mil_modified_image, p_matrox->m_milShowImage );
		MbufCopy(mil_modified_image, p_matrox->m_milMonoImage);
	}

	// リスト化する(リングバッファ数以上あれば削除)
	m_lstImageGrab.push_back( mil_modified_image );
	while( MAX_AVERAGE_IMAGE_GRAB_NUM < m_lstImageGrab.size() )
	{
		m_lstImageGrab.pop_front();
	}

	return( 0 );
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		1枚画像をGrabする

	2.パラメタ説明


	3.概要
		1枚画像をGrabする

	4.機能説明
		1枚画像をGrabする

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCamera::getOneGrab()
{
	if( m_bMainInitialFinished == false )
	{
		return;
	}
	if( m_iBoardType == MTX_HOST )
	{
		return;
	}
	//	フリーズ状態でのみ有効
	if( m_bThroughFlg == false )
	{
		//	今の画像を１つ前の画像として保存する
		MbufCopy( m_milShowImage, m_milPreShowImage );
		//	画像をGrabする
		MdigGrab( m_milDigitizer, m_milShowImage );
		MdigGrabWait(m_milDigitizer, M_GRAB_END);
		MbufCopy(m_milShowImage, m_milMonoImage);
	}

	return;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		差分画像を作成する

	2.パラメタ説明


	3.概要
		差分画像を作成する

	4.機能説明
		差分画像を作成する

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCamera::makeDiffImage()
{
	//	直前と今の画像の絶対値差分を取る
	MimArith( m_milOriginalImage, m_milDiffOrgImage, m_milShowImage, M_SUB_ABS + M_SATURATION );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		平均画像を取得する

	2.パラメタ説明


	3.概要
		平均画像を取得する

	4.機能説明
		平均画像を取得する

	5.戻り値
		０
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CMatroxCamera::getAveragedImageGrab( int niAverageNum, string ncsFilePath, BOOL nbSaveMono, BOOL nbSaveOnMemory )
{
	bool b_now_through_flg;
	int i_loop;

	MIL_DOUBLE StartTime, EndTime;

	b_now_through_flg = m_bThroughFlg;

	if( m_bMainInitialFinished == false )
	{
		return;
	}
	if( m_iBoardType == MTX_HOST )
	{
		return;
	}
	if( niAverageNum <= 0 || niAverageNum > 10 )
	{
		return;
	}

	//	スルーであればフリーズする
	if( m_bThroughFlg == true )
	{
		doFreeze();
	}

	MappTimer(M_DEFAULT, M_TIMER_READ, &StartTime);

	//	フック関数を使用する。指定枚数画像を取得する
	MdigProcess(m_milDigitizer, m_milAverageImageGrab, niAverageNum,
								M_SEQUENCE, M_SYNCHRONOUS, (MIL_DIG_HOOK_FUNCTION_PTR)&ProcessingFunction, this );
	
	MappTimer(M_DEFAULT, M_TIMER_READ, &EndTime);
	//時間をログ出力
	outputTimeLog("MdigProcess", (EndTime - StartTime)*1000.0);

	//	平均化積算バッファクリア
	MbufClear( m_milAverageImageCalc, 0 );

	//	平均化する
	for( i_loop = 0; i_loop < niAverageNum; i_loop++ )
	{
		MimArith( m_milAverageImageGrab[i_loop], m_milAverageImageCalc, m_milAverageImageCalc,  M_ADD );
	}
	MimArith( m_milAverageImageCalc, (MIL_DOUBLE)niAverageNum, m_milAverageImageCalc,  M_DIV_CONST );

#ifdef MIL10

	//	モノクロ(1バンド)出力
	if( nbSaveMono == TRUE )
	{
		MappTimer(M_DEFAULT, M_TIMER_READ, &StartTime);

		MIL_ID	mil_save_image;
		MbufAllocColor(m_milSys, 1, m_szImageSize.cx, m_szImageSize.cy, 16+M_UNSIGNED, M_IMAGE+M_PROC, &mil_save_image );
		MbufCopy( m_milAverageImageCalc, mil_save_image );

		MappTimer(M_DEFAULT, M_TIMER_READ, &EndTime);
		//時間をログ出力
		outputTimeLog("BufCopy", (EndTime - StartTime)*1000.0);
		MappTimer(M_DEFAULT, M_TIMER_READ, &StartTime);

		//	ファイルに保存
		if (nbSaveOnMemory == FALSE)
		{
			MbufExport(ncsFilePath.c_str(), M_BMP, mil_save_image);

			MappTimer(M_DEFAULT, M_TIMER_READ, &EndTime);
			//時間をログ出力
			outputTimeLog("ImageSave", (EndTime - StartTime)*1000.0);
		}
		//	メモリ上に保存
		else
		{
			MappTimer(M_DEFAULT, M_TIMER_READ, &EndTime);
			//時間をログ出力
			outputTimeLog("ImageSaveOnMemory", (EndTime - StartTime)*1000.0);
		}

		MbufFree(mil_save_image);
	}
	//	カラー(3バンド)出力
	else
	{
		//	ファイルに保存
		MbufExport(ncsFilePath.c_str(), M_BMP, m_milAverageImageCalc);
	}


#else
		//	ファイルに保存
		MbufExport((MIL_TEXT_PTR)ncsFilePath.c_str(), M_BMP, m_milAverageImageCalc);

#endif

	//	スルーだったらスルーに戻す
	if( b_now_through_flg == true )
	{
		doThrough();
	}

}

/*------------------------------------------------------------------------------------------
	1.日本語名
		Gainと露光時間をセットする

	2.パラメタ説明


	3.概要
		Gainと露光時間をセットする

	4.機能説明
		Gainと露光時間をセットする

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setGainAndExposureTime(double ndGain, double ndExposureTime)
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MIL_INT32 i_gain_raw = (MIL_INT32)ndGain;
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_raw);
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTimeAbs"),M_TYPE_DOUBLE ,&ndExposureTime);
	}
	// Point grey
	else
	{
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("Gain"),M_TYPE_DOUBLE ,&ndGain);
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTime"),M_TYPE_DOUBLE ,&ndExposureTime);
	}
	delete [] str_vendor_name;

#endif
	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		ブラックレベルをセットする

	2.パラメタ説明


	3.概要
		ブラックレベルをセットする

	4.機能説明
		ブラックレベルをセットする

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setBlackLevel(double ndBlackLevel)
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MIL_INT32 i_black_level_raw = (MIL_INT32)ndBlackLevel;
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("BlackLevelRaw"),M_TYPE_MIL_INT32 ,&i_black_level_raw);
	}
	// Point grey
	else
	{
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("BlackLevel"),M_TYPE_DOUBLE ,&ndBlackLevel);
	}
	delete [] str_vendor_name;

#endif
	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		Gainをセットする

	2.パラメタ説明


	3.概要
		Gainをセットする

	4.機能説明
		Gainをセットする

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setGain(double ndGain)
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MIL_INT32 i_gain_raw = (MIL_INT32)ndGain;
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_raw);
	}
	// Point grey
	else
	{
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("Gain"),M_TYPE_DOUBLE ,&ndGain);
	}
	delete [] str_vendor_name;

#endif
	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		露光時間をセットする

	2.パラメタ説明


	3.概要
		露光時間をセットする

	4.機能説明
		露光時間をセットする

	5.戻り値

		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setExposureTime(double ndExposureTime)
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTimeAbs"),M_TYPE_DOUBLE ,&ndExposureTime);
	}
	// Point grey
	else
	{
		MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("ExposureTime"),M_TYPE_DOUBLE ,&ndExposureTime);
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
int CMatroxCamera::getBlackLevelMaxMin( double *npdMax, double *npdMin )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MIL_INT32 i_black_level_raw_max = 0;
		MIL_INT32 i_black_level_raw_min = 0;
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MAX ,MIL_TEXT("BlackLevelRaw"),M_TYPE_MIL_INT32 ,&i_black_level_raw_max);
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MIN ,MIL_TEXT("BlackLevelRaw"),M_TYPE_MIL_INT32 ,&i_black_level_raw_min);
		*npdMax = (double)i_black_level_raw_max;
		*npdMin = (double)i_black_level_raw_min;
	}
	// Point grey
	else
	{
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MAX,MIL_TEXT("BlackLevel"),M_TYPE_DOUBLE ,npdMax);
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MIN,MIL_TEXT("BlackLevel"),M_TYPE_DOUBLE ,npdMin);
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
int CMatroxCamera::getGainMaxMin( double *npdMax, double *npdMin )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MIL_INT32 i_gain_max = 0;
		MIL_INT32 i_gain_min = 0;
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MAX ,MIL_TEXT("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_max);
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MIN ,MIL_TEXT("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_min);
		*npdMax = (double)i_gain_max;
		*npdMin = (double)i_gain_min;
	}
	// Point grey
	else
	{
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MAX,MIL_TEXT("Gain"),M_TYPE_DOUBLE ,npdMax);
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MIN,MIL_TEXT("Gain"),M_TYPE_DOUBLE ,npdMin);
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
int CMatroxCamera::getExposureTimeMaxMin( double *npdMax, double *npdMin )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigInquireFeature(m_milDigitizer, M_FEATURE_MAX, MIL_TEXT("ExposureTimeAbs"), M_TYPE_DOUBLE, npdMax);
		MdigInquireFeature(m_milDigitizer, M_FEATURE_MIN, MIL_TEXT("ExposureTimeAbs"), M_TYPE_DOUBLE, npdMin);
	}
	// Point grey
	else
	{
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MAX,MIL_TEXT("ExposureTime"),M_TYPE_DOUBLE ,npdMax);
		MdigInquireFeature(m_milDigitizer,M_FEATURE_MIN,MIL_TEXT("ExposureTime"),M_TYPE_DOUBLE ,npdMin);
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
int CMatroxCamera::setTriggerModeOff( void )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "Off" ) );
	}
	// Point grey
	else
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "Off" ) );
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
		MILHelpのmilgige.cppを参考にする。
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setTriggerModeSoftware( void )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "On" ) );
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerSource" ), M_TYPE_ENUMERATION, MIL_TEXT( "Software" ) );
	}
	// Point grey
	else
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "On" ) );
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerSource" ), M_TYPE_ENUMERATION, MIL_TEXT( "Software" ) );
	}
	delete [] str_vendor_name;

#endif
	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		トリガモード ハードウェア設定

	2.パラメタ説明
		nstrTrigger		トリガ名 (Ex.acA1300-30gmの場合Line1)

	3.概要
		トリガモード ハードウェア設定

	4.機能説明


	5.戻り値


	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CMatroxCamera::setTriggerModeHardware( const string nstrTrigger )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "On" ) );
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerSource" ), M_TYPE_ENUMERATION, MIL_TEXT( nstrTrigger.c_str() ) );
	}
	// Point grey
	else
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerMode" ), M_TYPE_ENUMERATION, MIL_TEXT( "On" ) );
		MdigControlFeature( m_milDigitizer, M_FEATURE_VALUE_AS_STRING, MIL_TEXT( "TriggerSource" ), M_TYPE_ENUMERATION, MIL_TEXT( nstrTrigger.c_str() ) );
	}
	delete [] str_vendor_name;

#endif
	return 0;
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
int CMatroxCamera::executeSoftwareTrigger( void )
{
#ifdef MIL10

	if (m_bMainInitialFinished == false)
	{
		return -1;
	}
	else if (m_iBoardType != MTX_GIGE)
	{
		return 0;
	}

	MIL_TEXT_PTR str_vendor_name;
	str_vendor_name = new MIL_TEXT_CHAR[256];
	MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

	//	Basler
	if( strstr(str_vendor_name,"Basler") != NULL )
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_EXECUTE, MIL_TEXT( "TriggerSoftware" ), M_TYPE_COMMAND, M_NULL );
	}
	// Point grey
	else
	{
		MdigControlFeature( m_milDigitizer, M_FEATURE_EXECUTE, MIL_TEXT( "TriggerSoftware" ), M_TYPE_COMMAND, M_NULL );
	}
	delete [] str_vendor_name;

#endif
	return 0;

}



