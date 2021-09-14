using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.IO;

namespace ImageMatrox
{
    public class CMatroxCommon : GlobalStructure
    {


        #region"使用する変数群"
        public static MIL_ID m_milApp;             //	アプリケーションID
        public static MIL_ID m_milSys;             //	システムID
        public static MIL_ID m_milDisp;                //	ディスプレイID
        public static MIL_ID m_milDigitizer;           //	デジタイザID
        public MIL_ID m_milShowImage;           //	画像バッファ(表示用)
        public static MIL_ID m_milPreShowImage;        //	1つ前にグラブした画像バッファ
        public static MIL_ID m_milDiffOrgImage;        //	差分用オリジナル画像
        public static MIL_ID m_milDiffTargetImage;     //	差分用ターゲット画像
        public static MIL_ID[] m_milImageGrab;   //	画像バッファ(グラブ専用)
        public static MIL_ID[] m_milAverageImageGrab;    //	平均化用画像バッファ(グラブ専用)
        public static MIL_ID m_milAverageImageCalc;    //	平均化用画像バッファ(積算用)
        public MIL_ID m_milMonoImage;           //	1プレーンの画像(モノクロなら表示用バッファと同じ、カラーならばRGBのいずれか)
        public MIL_ID m_milOriginalImage;       //	オリジナル画像(カラーバッファとして確保)
        public static MIL_ID m_milGraphic;         //	グラフィックバッファ
        public static MIL_ID m_milOverLay;         //	オーバーレイバッファ
        public static string m_strCameraFilePath;  //	DCFファイル名
        public static SIZE m_szImageSize;          //	画像サイズ
        public static SIZE m_szImageSizeForCamera;
        public static int m_iBoardType;            //	使用ボードタイプ
        public static int m_iNowColor;         //	現在のカラー
        public static bool m_bMainInitialFinished;
        public static bool m_bThroughFlg;          //	スルーならばTrue、フリーズならFalse
        public static double m_dNowMag;                //	現在の表示画像の倍率
        public static IntPtr m_hWnd;                 //	ウインドウのハンドル
        public static bool m_bNowDiffMode;         //	現在差分表示モードか否か
        public readonly MIL_INT TRANSPARENT_COLOR = MIL.M_RGB888(1, 1, 1);      //透過色

        public static IntPtr m_hWndForInspectionResult;  //	ウインドウのハンドル(検査結果表示用)
        public static MIL_ID m_milDispForInspectionResult;             //	ディスプレイID(検査結果表示用)
        public static MIL_ID m_milInspectionResultImage;           //	画像バッファ(検査結果表示用)
        public static MIL_ID m_milOverLayForInspectionResult;          //	オーバーレイバッファ(検査結果表示用)
        public static double m_dNowMagForInspectionResult;             //	現在の検査結果表示画像の倍率
        public static MIL_ID m_milDraphicSaveImage;                        //	グラフィックを画像に保存するためのバッファ

        public bool m_bFatalErrorOccured;   //	致命的なエラー発生(ソフト再起動必須)

        public static MIL_ID m_milInspectionResultImageTemp;
        public static double m_dFrameRate;         //	カメラのフレームレート(FPS)
        public static string m_strIPAddress;           //  カメラのIPアドレス

        public static int m_iEachLightPatternMatching; //照明毎にパターンマッチングを行うか否か　1:照明毎にパターンマッチング行う　0:一つの照明のみでパターンマッチング

        public static List<MIL_ID> m_lstImageGrab;		// ImageGrabのリスト

        public bool m_bDebugON;                //	デバッグ情報を出力する
        public string m_strDebugFolder;        //	デバッグ情報出力フォルダー
        public string m_strIniFilePAth = ".\\file.ini";
        public static string m_strDebugFileIdentifiedName;
        private string m_strExePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
        private Encoding m_Encoding = Encoding.GetEncoding("Shift_JIS");
        #endregion

        public CMatroxCommon()
        {
            m_milApp = MIL.M_NULL;
            m_milSys = MIL.M_NULL;
            m_milDisp = MIL.M_NULL;
            m_milDispForInspectionResult = MIL.M_NULL;
            m_milDigitizer = MIL.M_NULL;
            m_milShowImage = MIL.M_NULL;
            m_milPreShowImage = MIL.M_NULL;
            m_milMonoImage = MIL.M_NULL;
            m_milDiffOrgImage = MIL.M_NULL;
            m_milDiffTargetImage = MIL.M_NULL;
            m_milOriginalImage = MIL.M_NULL;
            m_milGraphic = MIL.M_NULL;
            m_milOverLay = MIL.M_NULL;
            m_milOverLayForInspectionResult = MIL.M_NULL;
            m_milDraphicSaveImage = MIL.M_NULL;
            m_milImageGrab = new MIL_ID[MAX_IMAGE_GRAB_NUM] { 0, 0 };
            m_milAverageImageGrab = new MIL_ID[MAX_AVERAGE_IMAGE_GRAB_NUM] { 0, 0 };
            m_milAverageImageCalc = MIL.M_NULL;
            m_milInspectionResultImage = MIL.M_NULL;
            m_milInspectionResultImageTemp = MIL.M_NULL;
            m_strCameraFilePath = "";
            m_szImageSize = new SIZE(0, 0);
            m_szImageSizeForCamera = new SIZE(0, 0);
            m_iBoardType = -1;
            m_bMainInitialFinished = false;
            m_bThroughFlg = false;
            m_iNowColor = 0;
            m_dNowMag = 1.0;
            m_dNowMagForInspectionResult = 1.0;
            m_bNowDiffMode = false;
            m_dFrameRate = 10.0;
            m_bFatalErrorOccured = false;
            m_strIPAddress = "";
            m_iEachLightPatternMatching = 0;
            m_strDebugFileIdentifiedName = "";
            m_bDebugON = true;
            m_strDebugFolder = ".\\DebugFile\\";
        }

        public int Initial(IntPtr nhDispHandle, string nstrSettingPath)
        {
            int i_loop;
            //SYSTEM_INFO SystemInfo = { 0 };
            //bool b_32bitSoft_on_64bitOS;

            //	パラメータファイルを読み込む
            readParameter(nstrSettingPath);

            //	アプリケーションID取得
            MIL.MappAlloc(MIL.M_DEFAULT, ref m_milApp);
            if (m_milApp == MIL.M_NULL)
            {
                return -1;
            }

            //　エラーメッセージを出さないようにする
            MIL.MappControl(MIL.M_ERROR, MIL.M_PRINT_DISABLE);
            //	エラーフック関数登録
            // 本クラスのポインター
            GCHandle hUserData = GCHandle.Alloc(this);
            // フック関数のポインタ
            MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_APP_HOOK_FUNCTION_PTR(hookErrorHandler);
            // 設定
            MIL.MappHookFunction(MIL.M_ERROR_CURRENT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));

            //	OSが64bitで、アプリケーションが32bitであるかどうかチェック
            //GetNativeSystemInfo(&SystemInfo);
            //if (SystemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64 ref & sizeof(void*) == 4)
            //{
            //    b_32bitSoft_on_64bitOS = true;
            //}
            //else
            //{
            //    b_32bitSoft_on_64bitOS = false;
            //}

            //	システムID取得
            switch (m_iBoardType)
            {
                case (int)MTX_TYPE.MTX_MORPHIS:
                    MIL.MsysAlloc(MIL.M_SYSTEM_MORPHIS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;
                case (int)MTX_TYPE.MTX_SOLIOSXCL:
                    break;
                case (int)MTX_TYPE.MTX_SOLIOSXA:
                    MIL.MsysAlloc(MIL.M_SYSTEM_SOLIOS, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;
                case (int)MTX_TYPE.MTX_METEOR2MC:

                    //MIL.MsysAlloc(MIL.M_SYSTEM_METEOR_II, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;

                //if (b_32bitSoft_on_64bitOS == true)
                //{
                //    MIL.MsysAlloc(("dmiltcp:\\\\localhost\\M_SYSTEM_GIGE_VISION"), MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                //}
                //else
                //{
                //    MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                //}
                case (int)MTX_TYPE.MTX_GIGE:
                    MIL.MsysAlloc(MIL.M_SYSTEM_GIGE_VISION, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;
                case (int)MTX_TYPE.MTX_HOST:
                    MIL.MsysAlloc(MIL.M_SYSTEM_HOST, MIL.M_DEV0, MIL.M_DEFAULT, ref m_milSys);
                    break;
                default:
                    return -1;
            }
            if (m_milSys == MIL.M_NULL)
            {
                return -1;
            }

            //	ディスプレイID取得

            MIL.MdispAlloc(m_milSys, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_milDisp);
            if (m_milDisp == MIL.M_NULL)
            {
                return -1;
            }
            MIL.MdispAlloc(m_milSys, MIL.M_DEFAULT, "M_DEFAULT", MIL.M_DEFAULT, ref m_milDispForInspectionResult);
            if (m_milDispForInspectionResult == MIL.M_NULL)
            {
                return -1;
            }


            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                //	デジタイザID取得

                if (m_strIPAddress != "")
                {
                    MIL.MdigAlloc(m_milSys, MIL.M_GC_CAMERA_ID(m_strIPAddress), m_strCameraFilePath, MIL.M_GC_DEVICE_IP_ADDRESS, ref m_milDigitizer);
                }
                else
                {
                    MIL.MdigAlloc(m_milSys, MIL.M_DEV0, m_strCameraFilePath, MIL.M_DEFAULT, ref m_milDigitizer);
                }

                if (m_milDigitizer == MIL.M_NULL)
                {
                    return -1;
                }
            }
            //	表示用画像バッファ
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milShowImage);
            }
            else
            {
                //	M_GRABは外す
                MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milShowImage);
            }
            //	モノクロ画像バッファ
            MIL.MbufAlloc2d(m_milSys, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milMonoImage);
            //	オリジナル画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milOriginalImage);
            //	表示用画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milPreShowImage);
            //	差分用オリジナル画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milDiffOrgImage);
            //	平均化用積算画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milAverageImageCalc);
            //検査結果表示用画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milInspectionResultImage);
            //グラフィックを画像に保存するためのバッファ
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDraphicSaveImage);

            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
                {
                    MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milImageGrab[i_loop]);
                }
                for (i_loop = 0; i_loop < MAX_AVERAGE_IMAGE_GRAB_NUM; i_loop++)
                {
                    MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milAverageImageGrab[i_loop]);
                }
            }

            //	グラフィックバッファ
            MIL.MgraAlloc(m_milSys, ref m_milGraphic);

            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                //　取込オフセットを0にする
                MIL.MdigControl(m_milDigitizer, MIL.M_SOURCE_OFFSET_X, 0);
                MIL.MdigControl(m_milDigitizer, MIL.M_SOURCE_OFFSET_Y, 0);
                //　取込幅をデフォルトの取込幅にする
                MIL.MdigControl(m_milDigitizer, MIL.M_SOURCE_SIZE_X, (double)m_szImageSize.cx);
                MIL.MdigControl(m_milDigitizer, MIL.M_SOURCE_SIZE_Y, (double)m_szImageSize.cy);
            }

            //	画像バッファクリア
            MIL.MbufClear(m_milShowImage, 0);
            MIL.MbufClear(m_milMonoImage, 0);
            MIL.MbufClear(m_milOriginalImage, 0);

            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
                {
                    MIL.MbufClear(m_milImageGrab[i_loop], 255);
                }
            }
            
            MIL.MdispControl(m_milDisp, MIL.M_INTERPOLATION_MODE, MIL.M_NEAREST_NEIGHBOR);
            MIL.MdispControl(m_milDisp, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisp, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);
            MIL.MdispControl(m_milDisp, (long)MIL.M_TRANSPARENT_COLOR, TRANSPARENT_COLOR);

            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_INTERPOLATION_MODE, MIL.M_NEAREST_NEIGHBOR);
            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_OVERLAY, MIL.M_ENABLE);
            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_OVERLAY_SHOW, MIL.M_ENABLE);
            MIL.MdispControl(m_milDispForInspectionResult, (long)MIL.M_TRANSPARENT_COLOR, TRANSPARENT_COLOR);

            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_UPDATE, MIL.M_DISABLE);

            MIL.MdispSelectWindow(m_milDisp, m_milShowImage, nhDispHandle);
            //MdispZoom( m_milDisp, 0.1, 0.1 );
            MIL.MdispControl(m_milDisp, MIL.M_CENTER_DISPLAY, MIL.M_ENABLE);    // センターに表示
            MIL.MdispControl(m_milDisp, MIL.M_SCALE_DISPLAY, MIL.M_ENABLE); // ストレッチして表示

            //	オーバーレイバッファ
            MIL.MdispInquire(m_milDisp, MIL.M_OVERLAY_ID, ref m_milOverLay);

            m_szImageSizeForCamera = m_szImageSize;


            //for only gigE camera
            if (m_iBoardType == (int)MTX_TYPE.MTX_GIGE)
            {
                StringBuilder str_vendor_name = new StringBuilder();
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);
                string DeviceVendorName = str_vendor_name.ToString();
                // ********** Sonyカメラの対応が未設定ではあるが、ここでの処理は重要ではないためすっ飛ばす	2021/03/12 ***************************************
                //	Basler initial
                //if( strstr(str_vendor_name,"Basler") != NULL )
                //{
                //	MIL_INT32 i_gain_raw = 301;
                //	double d_frame_rate_abs = m_dFrameRate;
                //	double d_exposure_time_abs = (1.0 / m_dFrameRate)*1.0e6 - 100.0;
                //	MIL_BOOL b_acquition_frame_rate_enable = MIL.M_TRUE;
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,("GainAuto"),M_TYPE_ENUMERATION,("Off"));
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,("ExposureAuto"),M_TYPE_ENUMERATION,("Off"));
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("AcquisitionFrameRateEnable"),M_TYPE_BOOLEAN,&b_acquition_frame_rate_enable);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("GainRaw"),M_TYPE_MIL_INT32 ,&i_gain_raw);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("AcquisitionFrameRateAbs"),M_TYPE_DOUBLE ,&d_frame_rate_abs);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("ExposureTimeAbs"),M_TYPE_DOUBLE ,&d_exposure_time_abs);
                //}
                ////	Point grey initial
                //else
                //{
                //	double d_blackLevel = 0.0;
                //	double d_gain = 00.0;
                //	double d_frame_rate = m_dFrameRate;
                //	double d_exposure_time = (1.0 / m_dFrameRate)*1.0e6 - 1000.0;
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,("GainAuto"),M_TYPE_ENUMERATION,("Off"));
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,("ExposureAuto"),M_TYPE_ENUMERATION,("Off"));
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE_AS_STRING,("AcquisitionFrameRateAuto"),M_TYPE_ENUMERATION,("Off"));
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("BlackLevel"),M_TYPE_DOUBLE ,&d_blackLevel);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("Gain"),M_TYPE_DOUBLE ,&d_gain);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("AcquisitionFrameRate"),M_TYPE_DOUBLE ,&d_frame_rate);
                //	MdigControlFeature(m_milDigitizer,M_FEATURE_VALUE,("ExposureTime"),M_TYPE_DOUBLE ,&d_exposure_time);

                //	MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, ("PixelFormat"), MIL.M_TYPE_STRING, ("Mono8"));

                //}
            }



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
        public void readParameter(string nstrSettingPath)
        {
            string cs_ini_file;
            StringBuilder buff = new StringBuilder(256);

            //	iniファイルのパス
            if (nstrSettingPath == "")
            {
                cs_ini_file = "PRM\\ImageProcess.ini";
            }
            else
            {
                cs_ini_file = nstrSettingPath + "\\ImageProcess.ini";
            }

            //	DCFファイルパス
            GetPrivateProfileString("Matrox", "CameraFile", "0", buff, 256, m_strIniFilePAth);
            m_strCameraFilePath = buff.ToString();
            m_strCameraFilePath = "PRM\\" + m_strCameraFilePath;

            //	画像サイズ
            GetPrivateProfileString("Matrox", "sizeX", "800", buff, 256, m_strIniFilePAth);
            m_szImageSize.cx = Int32.Parse(buff.ToString());
            GetPrivateProfileString("Matrox", "sizeY", "640", buff, 256, m_strIniFilePAth);
            m_szImageSize.cy = Int32.Parse(buff.ToString());

            //	使用画像ボード
            GetPrivateProfileString("Matrox", "BoardType", "0", buff, 256, m_strIniFilePAth);
            m_iBoardType = Int32.Parse(buff.ToString());

            //	フレームレート(FPS)
            GetPrivateProfileString("Matrox", "FrameRate", "10", buff, 256, m_strIniFilePAth);
            m_dFrameRate = Int64.Parse(buff.ToString());
            if (m_dFrameRate <= 0.0)
            {
                m_dFrameRate = 10.0;
            }

            //  カメラのIPアドレス
            GetPrivateProfileString("Matrox", "CameraIPAddress", "", buff, 256, m_strIniFilePAth);
            m_strIPAddress = buff.ToString();

            //	照明毎にパターンマッチングを行うか否か
            GetPrivateProfileString("Matrox", "EachLightPatternMatchingEnable", "0", buff, 256, m_strIniFilePAth);
            m_iEachLightPatternMatching = Int32.Parse(buff.ToString());
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
        public void CloseMatrox()
        {
            int i_loop;

            if (m_milShowImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milShowImage);
                m_milShowImage = MIL.M_NULL;
            }
            if (m_milMonoImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milMonoImage);
                m_milMonoImage = MIL.M_NULL;
            }
            if (m_milOriginalImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milOriginalImage);
                m_milOriginalImage = MIL.M_NULL;
            }

            for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
            {
                if (m_milImageGrab[i_loop] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_milImageGrab[i_loop]);
                    m_milImageGrab[i_loop] = MIL.M_NULL;
                }
            }
            for (i_loop = 0; i_loop < MAX_AVERAGE_IMAGE_GRAB_NUM; i_loop++)
            {
                if (m_milAverageImageGrab[i_loop] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_milAverageImageGrab[i_loop]);
                    m_milAverageImageGrab[i_loop] = MIL.M_NULL;
                }
            }
            if (m_milPreShowImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milPreShowImage);
                m_milPreShowImage = MIL.M_NULL;
            }
            if (m_milDiffOrgImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milDiffOrgImage);
                m_milDiffOrgImage = MIL.M_NULL;
            }
            if (m_milAverageImageCalc != MIL.M_NULL)
            {
                MIL.MbufFree(m_milAverageImageCalc);
                m_milAverageImageCalc = MIL.M_NULL;
            }
            if (m_milInspectionResultImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milInspectionResultImage);
                m_milInspectionResultImage = MIL.M_NULL;
            }
            if (m_milDraphicSaveImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milDraphicSaveImage);
                m_milDraphicSaveImage = MIL.M_NULL;
            }

            if (m_milGraphic != MIL.M_NULL)
            {
                MIL.MgraFree(m_milGraphic);
                m_milGraphic = MIL.M_NULL;
            }
            if (m_milDisp != MIL.M_NULL)
            {
                MIL.MdispFree(m_milDisp);
                m_milDisp = MIL.M_NULL;
            }
            if (m_milDispForInspectionResult != MIL.M_NULL)
            {
                MIL.MdispFree(m_milDispForInspectionResult);
                m_milDispForInspectionResult = MIL.M_NULL;
            }

            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                if (m_milDigitizer != MIL.M_NULL)
                {
                    MIL.MdigFree(m_milDigitizer);
                    m_milDigitizer = MIL.M_NULL;
                }
            }
            //	エラーフックを解除

            // 本クラスのポインター
            GCHandle hUserData = GCHandle.Alloc(this);
            // フック関数のポインタ
            MIL_APP_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_APP_HOOK_FUNCTION_PTR(hookErrorHandler);
            // 設定
            MIL.MappHookFunction(MIL.M_ERROR_CURRENT + MIL.M_UNHOOK, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));

            if (m_milSys != MIL.M_NULL)
            {
                MIL.MsysFree(m_milSys);
                m_milSys = MIL.M_NULL;
            }

            if (m_milApp != MIL.M_NULL)
            {
                MIL.MappFree(m_milApp);
                m_milApp = MIL.M_NULL;
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
        public void reallocMilImage(SIZE nszNewImageSize)
        {
            int i_loop;

            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //	まず画像バッファを全て開放する
            if (m_milShowImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milShowImage);
                m_milShowImage = MIL.M_NULL;
            }
            if (m_milMonoImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milMonoImage);
                m_milMonoImage = MIL.M_NULL;
            }
            if (m_milOriginalImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milOriginalImage);
                m_milOriginalImage = MIL.M_NULL;
            }

            for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
            {
                if (m_milImageGrab[i_loop] != MIL.M_NULL)
                {
                    MIL.MbufFree(m_milImageGrab[i_loop]);
                    m_milImageGrab[i_loop] = MIL.M_NULL;
                }
            }
            if (m_milPreShowImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milPreShowImage);
                m_milPreShowImage = MIL.M_NULL;
            }
            if (m_milDiffOrgImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milDiffOrgImage);
                m_milDiffOrgImage = MIL.M_NULL;
            }
            if (m_milDraphicSaveImage != MIL.M_NULL)
            {
                MIL.MbufFree(m_milDraphicSaveImage);
                m_milDraphicSaveImage = MIL.M_NULL;
            }
            //	if( m_milInspectionResultImage != MIL.M_NULL )
            //	{
            //		MbufFree(m_milInspectionResultImage);
            //		m_milInspectionResultImage = MIL.M_NULL;
            //	}

            //	表示用画像バッファ
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_GRAB + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milShowImage);
            }
            else
            {
                //	GRABを外す
                MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milShowImage);
            }

            //	モノクロ画像バッファ
            MIL.MbufAlloc2d(m_milSys, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref m_milMonoImage);
            //	オリジナル画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milOriginalImage);
            //	表示用画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milPreShowImage);
            //	差分用オリジナル画像バッファ
            MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milDiffOrgImage);
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
                {
                    MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_GRAB + MIL.M_PROC, ref m_milImageGrab[i_loop]);
                }
            }
            //グラフィックを画像に保存するためのバッファ
            MIL.MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milDraphicSaveImage);
            //	//検査結果表示用画像バッファ
            //	MbufAllocColor(m_milSys, 3, nszNewImageSize.cx, nszNewImageSize.cy, 8+M_UNSIGNED, MIL.M_IMAGE+M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref m_milInspectionResultImage );

            MIL.MdispSelectWindow(m_milDisp, m_milShowImage, m_hWnd);
            MIL.MdispInquire(m_milDisp, MIL.M_OVERLAY_ID, ref m_milOverLay);

            //	if( m_hWndForInspectionResult != NULL )
            //	{
            //		MdispSelectWindow( m_milDispForInspectionResult, m_milInspectionResultImage, m_hWndForInspectionResult );
            //		MdispInquire( m_milDispForInspectionResult, MIL.M_OVERLAY_ID, ref m_milOverLayForInspectionResult );
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
        public void setShowImageMag(double ndMag)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //	制限を設けておく。とりあえず0.2倍から8倍
            if (ndMag >= 0.2 && ndMag <= 10.0)
            {
                m_dNowMag = ndMag;
                MIL.MdispZoom(m_milDisp, ndMag, ndMag);
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
        public double getZoomMag()
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
        public void saveImage(RECT nrctSaveArea, bool nAllSaveFlg, string ncsFilePath, bool nbSaveMono)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //	全画面保存
            if (nAllSaveFlg == true)
            {

                if (nbSaveMono == false)
                {
                    MIL.MbufExport(ncsFilePath, MIL.M_BMP, m_milShowImage);
                }
                else
                {
                    MIL_ID mil_save_image = MIL.M_NULL;
                    MIL.MbufAllocColor(m_milSys, 1, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_save_image);
                    MIL.MbufCopy(m_milShowImage, mil_save_image);
                    MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_save_image);
                    MIL.MbufFree(mil_save_image);
                }

            }
            //	指定のエリアを保存
            else
            {
                SIZE sz_image_size;
                MIL_ID mil_area_image = MIL.M_NULL;

                //	画像サイズ取得
                sz_image_size.cx = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_X, MIL.M_NULL);
                sz_image_size.cy = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_Y, MIL.M_NULL);

                //	指定された位置に画像がない場合は0を返す
                if (nrctSaveArea.left < 0 || nrctSaveArea.top < 0 || nrctSaveArea.right > sz_image_size.cx || nrctSaveArea.bottom > sz_image_size.cy ||
                (nrctSaveArea.right - nrctSaveArea.left) == 0 || (nrctSaveArea.bottom - nrctSaveArea.top) == 0)
                {
                    return;
                }

                //	指定のエリアの画像バッファを抽出する
                //	モデル画像バッファのメモリを確保する
                MIL.MbufAllocColor(m_milSys, 3, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
                                8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref mil_area_image);
                //	原画像からモデル画像を取得する
                MIL.MbufTransfer(m_milShowImage, mil_area_image, nrctSaveArea.left, nrctSaveArea.top, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
                            MIL.M_DEFAULT, 0, 0, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, MIL.M_DEFAULT,
                            MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);

                //	画像保存

                if (nbSaveMono == false)
                {
                    MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_area_image);
                }
                else
                {
                    MIL_ID mil_save_image = MIL.M_NULL;
                    MIL.MbufAllocColor(m_milSys, 1, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_save_image);
                    MIL.MbufCopy(mil_area_image, mil_save_image);
                    MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_save_image);
                    MIL.MbufFree(mil_save_image);
                }

                MIL.MbufFree(mil_area_image);
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
        public void saveOrigImage(RECT nrctSaveArea, bool nAllSaveFlg, string ncsFilePath)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //	全画面保存
            if (nAllSaveFlg == true)
            {

                MIL.MbufExport(ncsFilePath, MIL.M_BMP, m_milOriginalImage);

            }
            //	指定のエリアを保存
            else
            {
                SIZE sz_image_size;
                MIL_ID mil_area_image = MIL.M_NULL;

                //	画像サイズ取得
                sz_image_size.cx = (int)MIL.MbufInquire(m_milOriginalImage, MIL.M_SIZE_X, MIL.M_NULL);
                sz_image_size.cy = (int)MIL.MbufInquire(m_milOriginalImage, MIL.M_SIZE_Y, MIL.M_NULL);

                //	指定された位置に画像がない場合は0を返す
                if (nrctSaveArea.left < 0 || nrctSaveArea.top < 0 || nrctSaveArea.right > sz_image_size.cx || nrctSaveArea.bottom > sz_image_size.cy ||
                (nrctSaveArea.right - nrctSaveArea.left) == 0 || (nrctSaveArea.bottom - nrctSaveArea.top) == 0)
                {
                    return;
                }

                //	指定のエリアの画像バッファを抽出する
                //	モデル画像バッファのメモリを確保する
                MIL.MbufAllocColor(m_milSys, 3, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
                                8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref mil_area_image);
                //	原画像からモデル画像を取得する
                MIL.MbufTransfer(m_milOriginalImage, mil_area_image, nrctSaveArea.left, nrctSaveArea.top, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top,
                            MIL.M_DEFAULT, 0, 0, nrctSaveArea.right - nrctSaveArea.left, nrctSaveArea.bottom - nrctSaveArea.top, MIL.M_DEFAULT,
                            MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);

                //	画像保存

                MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_area_image);

                MIL.MbufFree(mil_area_image);
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
        public bool getThoughStatus()
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
        public void setOriginImage()
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            MIL.MbufCopy(m_milOriginalImage, m_milShowImage);
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
        public SIZE getImageSize()
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
        public int loadImage(string ncsFilePath)
        {
            int i_loop;
            MIL_ID mil_image = MIL.M_NULL;
            SIZE sz_image_size;

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            //	モデル画像を読み込む

            MIL.MbufRestore(ncsFilePath, m_milSys, ref mil_image);

            //	モデル画像サイズ取得
            sz_image_size.cx = (int)MIL.MbufInquire(mil_image, MIL.M_SIZE_X, MIL.M_NULL);
            sz_image_size.cy = (int)MIL.MbufInquire(mil_image, MIL.M_SIZE_Y, MIL.M_NULL);

            //	メモリの再設定
            reallocMilImage(sz_image_size);

            MIL.MbufCopy(mil_image, m_milShowImage);
            MIL.MbufCopy(mil_image, m_milMonoImage);
            MIL.MbufCopy(mil_image, m_milOriginalImage);
            MIL.MbufCopy(mil_image, m_milPreShowImage);
            if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
            {
                for (i_loop = 0; i_loop < MAX_IMAGE_GRAB_NUM; i_loop++)
                {
                    MIL.MbufCopy(mil_image, m_milImageGrab[i_loop]);
                }
            }
            //	メモリ解放
            MIL.MbufFree(mil_image);

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
        public void setColorImage(int niConversionType, int niPlane)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //　表示用バッファをクリアする
            MIL.MbufClear(m_milShowImage, MIL.M_RGB888(0, 0, 0));

            //	RGBの場合
            if (niConversionType == 0)
            {
                switch (niPlane)
                {
                    case (int)RGB_TYPE.RGB:
                        //	RGBのときはオリジナル画像に戻す。画像処理バッファはR固定
                        MIL.MbufCopy(m_milOriginalImage, m_milShowImage);
                        MIL.MbufCopyColor(m_milOriginalImage, m_milMonoImage, MIL.M_RED);
                        break;
                    case (int)RGB_TYPE.R:
                        MIL.MbufCopyColor(m_milOriginalImage, m_milMonoImage, MIL.M_RED);
                        MIL.MbufCopy(m_milMonoImage, m_milShowImage);
                        break;
                    case (int)RGB_TYPE.G:
                        MIL.MbufCopyColor(m_milOriginalImage, m_milMonoImage, MIL.M_GREEN);
                        MIL.MbufCopy(m_milMonoImage, m_milShowImage);
                        break;
                    case (int)RGB_TYPE.B:
                        MIL.MbufCopyColor(m_milOriginalImage, m_milMonoImage, MIL.M_BLUE);
                        MIL.MbufCopy(m_milMonoImage, m_milShowImage);
                        break;
                    default:
                        break;
                }

                m_iNowColor = niPlane;
            }
            // HSIの場合
            else if (niConversionType == 1)
            {
                //HSIに変換
                MIL.MimConvert(m_milShowImage, m_milImageGrab[1], MIL.M_RGB_TO_HLS);

                switch (niPlane)
                {
                    case (int)HSI_TYPE.H:
                        MIL.MbufCopyColor(m_milImageGrab[1], m_milMonoImage, MIL.M_HUE);
                        MIL.MbufCopyColor(m_milMonoImage, m_milShowImage, MIL.M_ALL_BANDS);
                        break;
                    case (int)HSI_TYPE.S:
                        MIL.MbufCopyColor(m_milImageGrab[1], m_milMonoImage, MIL.M_SATURATION);
                        MIL.MbufCopyColor(m_milMonoImage, m_milShowImage, MIL.M_ALL_BANDS);
                        break;
                    case (int)HSI_TYPE.I:
                        MIL.MbufCopyColor(m_milImageGrab[1], m_milMonoImage, MIL.M_LUMINANCE);
                        MIL.MbufCopyColor(m_milMonoImage, m_milShowImage, MIL.M_ALL_BANDS);
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
        public int getColorCameraInfo()
        {
            long l_ret;

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            if (m_iBoardType == (int)MTX_TYPE.MTX_HOST)
            {
                return (0);
            }
            //  カラー情報を取得する
            l_ret = (long)MIL.MdigInquire(m_milDigitizer, MIL.M_COLOR_MODE, MIL.M_NULL);

            switch (l_ret)
            {
                //　モノクロ
                case MIL.M_MONO8_VIA_RGB:
                case MIL.M_MONOCHROME:
                    return (0);
                //　カラー
                case MIL.M_COMPOSITE:
                case MIL.M_EXTERNAL_CHROMINANCE:
                case MIL.M_RGB:
                    return (1);
                default:
                    return (0);
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
        public int getPixelValueOnPosition(POINT nNowPoint, ref int RValue, ref int GValue, ref int BValue)
        {
            byte[] bt_pixel_value = { 0, 0, 0 };
            SIZE sz_image_size;
            int i_ret = -1;

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            //	画像サイズ取得
            sz_image_size.cx = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_X, MIL.M_NULL);
            sz_image_size.cy = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_Y, MIL.M_NULL);

            //	指定された位置に画像がない場合は0を返す
            if (nNowPoint.x >= 0 && nNowPoint.x < sz_image_size.cx && nNowPoint.y >= 0 && nNowPoint.y < sz_image_size.cy)
            {
                MIL.MbufGet2d(m_milShowImage, nNowPoint.x, nNowPoint.y, 1, 1, bt_pixel_value);
                i_ret = 0;
            }
            RValue = (int)bt_pixel_value[0];
            GValue = (int)bt_pixel_value[1];
            BValue = (int)bt_pixel_value[2];

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
        public int setDiffOrgImage()
        {
            MIL.MbufCopy(m_milShowImage, m_milDiffOrgImage);
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
        public void resetDiffMode()
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
        public int setDispHandleForInspectionResult(IntPtr nhDispHandleForInspectionResult)
        {
            if (m_hWndForInspectionResult == null)
            {
                MIL.MbufClear(m_milInspectionResultImage, CLEAR_IMAGE_BUFFER);
                m_hWndForInspectionResult = nhDispHandleForInspectionResult;
                MIL.MdispSelectWindow(m_milDispForInspectionResult, m_milInspectionResultImage, m_hWndForInspectionResult);
                //	オーバーレイバッファ
                MIL.MdispInquire(m_milDispForInspectionResult, MIL.M_OVERLAY_ID, ref m_milOverLayForInspectionResult);
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
        public void setZoomMagForInspectionResult(double ndMag)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }

            //	制限を設けておく。とりあえず0.2倍から10倍
            if (ndMag >= 0.2 && ndMag <= 10.0)
            {
                m_dNowMagForInspectionResult = ndMag;
                MIL.MdispZoom(m_milDispForInspectionResult, ndMag, ndMag);
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
        public double getZoomMagForInspectionResult()
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
        public void saveInspectionResultImage(string ncsFilePath)
        {
            MIL_ID mil_temp = MIL.M_NULL;
            MIL_ID mil_result_temp = MIL.M_NULL;
            int i_index;
            string str_ext;

            //	オーバーレイバッファと検査結果画像バッファの一時バッファを用意
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_temp);
            MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_DISP + MIL.M_PACKED + MIL.M_BGR24, ref mil_result_temp);
            //	一時バッファに画像をコピー
            MIL.MbufCopy(m_milOverLayForInspectionResult, mil_temp);
            //	MbufCopy( m_milInspectionResultImage, mil_result_temp );
            MIL.MbufCopy(m_milInspectionResultImageTemp, mil_result_temp);

            //	オーバーレイを検査結果画像上にコピー
            //	MbufTransfer( mil_temp, mil_result_temp, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COMPOSITION, MIL.M_DEFAULT, MIL.M_RGB888(1,1,1), MIL.M_NULL );
            MIL.MbufTransfer(m_milDraphicSaveImage, mil_result_temp, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_DEFAULT, MIL.M_COMPOSITION, MIL.M_DEFAULT, MIL.M_RGB888(1, 1, 1), MIL.M_NULL);

            //	ファイル出力

            //	拡張子を抽出
            i_index = ncsFilePath.IndexOf(".");
            //	拡張子がない場合は仕方ないのでビットマップの拡張子をつけてビットマップで保存する
            if (i_index < 0)
            {
                ncsFilePath += ".bmp";
                str_ext = "bmp";
            }
            else
            {
                //	ファイル名の最後の文字が「.」だった場合もビットマップにしてしまう
                if (i_index + 1 == ncsFilePath.Length)
                {
                    ncsFilePath += "bmp";
                    str_ext = "bmp";
                }
                else
                {
                    str_ext = ncsFilePath.Substring(i_index + 1);
                }
            }
            //	jpg
            if (string.Compare(str_ext, "jpg") == 0 || string.Compare(str_ext, "JPG") == 0)
            {
                MIL.MbufExport(ncsFilePath, MIL.M_JPEG_LOSSY, mil_result_temp);
            }
            //	png
            else if (string.Compare(str_ext, "png") == 0 || string.Compare(str_ext, "PNG") == 0)
            {
                MIL.MbufExport(ncsFilePath, MIL.M_PNG, mil_result_temp);
            }
            //	bmp
            else
            {
                MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_result_temp);
            }

            //	メモリ開放
            MIL.MbufFree(mil_result_temp);
            MIL.MbufFree(mil_temp);
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
        public void ClearInspectionResultImage()
        {
            MIL.MbufClear(m_milInspectionResultImage, CLEAR_IMAGE_BUFFER);
            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_OVERLAY_CLEAR, MIL.M_TRANSPARENT_COLOR);

            //	画面更新をOFFに
            MIL.MdispControl(m_milDispForInspectionResult, MIL.M_UPDATE, MIL.M_DISABLE);
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
        public int getAveragePixelValueOnRegion(RECT nrctRegion)
        {
            MIL_ID mil_region = MIL.M_NULL;
            MIL_ID mil_stat_result = MIL.M_NULL;
            MIL_INT i_average_value = MIL.M_NULL;

            //	矩形が適切な矩形かチェック
            if ((nrctRegion.right - nrctRegion.left <= 1) || (nrctRegion.bottom - nrctRegion.top <= 1))
            {
                //	小さすぎたらエラー
                return -1;
            }

            //	メモリ確保
            MIL.MbufAlloc2d(m_milSys, nrctRegion.right - nrctRegion.left, nrctRegion.bottom - nrctRegion.top, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_region);
            //矩形領域を抽出
            MIL.MbufTransfer(m_milMonoImage, mil_region, nrctRegion.left, nrctRegion.top, (nrctRegion.right - nrctRegion.left), (nrctRegion.bottom - nrctRegion.top), MIL.M_DEFAULT, 0, 0, (nrctRegion.right - nrctRegion.left), (nrctRegion.bottom - nrctRegion.top), MIL.M_DEFAULT, MIL.M_COPY, MIL.M_DEFAULT, MIL.M_NULL, MIL.M_NULL);

            MIL.MimAllocResult(m_milSys, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref mil_stat_result);
            //領域の平均値を求める
            MIL.MimStat(mil_region, mil_stat_result, MIL.M_MEAN, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
            MIL.MimGetResult(mil_stat_result, MIL.M_MEAN + MIL.M_TYPE_MIL_INT, ref i_average_value);

            //	メモリ解放
            MIL.MimFree(mil_stat_result);
            MIL.MbufFree(mil_region);

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
        public void SetDisplayHandle(IntPtr nhDispHandle)
        {
            m_hWnd = nhDispHandle;
            MIL.MdispSelectWindow(m_milDisp, m_milShowImage, m_hWnd);
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
        public void outputErrorLog(string nstrErrorLog)
        {

            string str_file_name = "MILErrorLog.log";       //	ログファイルパス
            string str_file_path = $"{setFolderName(m_strExePath, "Log")}{str_file_name}";
            string str_log_data;
            DateTime time_now = System.DateTime.Now;
            str_log_data = $"{time_now.ToString("yyyyMMdd")}{time_now.ToString("HHmm")}{time_now.ToString("ssfff")}_error_{nstrErrorLog}";
            var writer = new StreamWriter(str_file_path, true, m_Encoding);
            writer.WriteLine(str_log_data);
            writer.Close();
        }
        public void OutputDebugString(string nstrDebugLog)
        {
            string str_file_name = "MILDebugLog.log";       //	ログファイルパス
            string str_file_path = $"{setFolderName(m_strExePath, "Log")}{str_file_name}";
            string str_log_data;
            DateTime time_now = System.DateTime.Now;
            str_log_data = $"{time_now.ToString("yyyyMMdd")}{time_now.ToString("HHmm")}{time_now.ToString("ssfff")}_error_{nstrDebugLog}";
            var writer = new StreamWriter(str_file_path, true, m_Encoding);
            writer.WriteLine(str_log_data);
            writer.Close();
        }

        private string setFolderName(string nstrExecFolderName, string nstrFolderName)
        {
            string str_folder_name;
            str_folder_name = $"{nstrExecFolderName}\\{nstrFolderName}";
            if (false == System.IO.File.Exists(str_folder_name))
            {
                System.IO.Directory.CreateDirectory(str_folder_name);
            }
            str_folder_name = $"{str_folder_name}\\";
            return str_folder_name;
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
        public void outputErrorLog(string nstrFunctionName, int niErrorCode)
        {
            string chr_log = $"{nstrFunctionName},{niErrorCode}";
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
        public bool getFatalErrorOccured()
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
        public void outputTimeLog(string nstrProcessName, double ndTime)
        {
            string str_file_name = "MILTimeLog.log";       //	ログファイルパス
            string str_file_path = $"{setFolderName(m_strExePath, "Log")}{str_file_name}";
            string str_log_data;
            DateTime time_now = System.DateTime.Now;
            str_log_data = $"{time_now.ToString("yyyyMMdd")}{time_now.ToString("HHmm")}{time_now.ToString("ssfff")}_time_{nstrProcessName}_{(int)ndTime}ms";
            var writer = new StreamWriter(str_file_path, true, m_Encoding);
            writer.WriteLine(str_log_data);
            writer.Close();
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
            public int getMonoBitmapData(long nlArySize, ref byte[] npbyteData)
        {
            // 未初期化 又は 画像が無い場合
            if (false == m_bMainInitialFinished || 0 == m_lstImageGrab.Count())
            {
                return -1;
            }
            
            // 画像ID取得
            MIL_ID mil_image = m_lstImageGrab[0];
            m_lstImageGrab.RemoveAt(0);

            // ユーザが準備した画像バッファサイズが少ない
            if (nlArySize < m_szImageSize.cx * m_szImageSize.cy)
            {
                return -1;
            }

            // モノクロ画像バッファ確保
            MIL_ID mil_mono = MIL.M_NULL;
            MIL.MbufAlloc2d(m_milSys, m_szImageSize.cx, m_szImageSize.cy, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_mono);
            // モノクロ画像バッファクリア
            MIL.MbufClear(mil_mono, 0);
            // モノクロ画像バッファコピー
            MIL.MbufCopy(mil_image, mil_mono);
            // モノクロ画像バッファ ビットマップデータ取得
            MIL.MbufGet2d(mil_mono, 0, 0, m_szImageSize.cx, m_szImageSize.cy, npbyteData);
            // メモリ解放
            MIL.MbufFree(mil_mono);

            return 0;
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
        protected MIL_INT hookErrorHandler(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            StringBuilder ErrorMessageFunction = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorMessage = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage1 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage2 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            StringBuilder ErrorSubMessage3 = new StringBuilder(MIL.M_ERROR_MESSAGE_SIZE);
            string str_error;
            string str_function;
            long NbSubCode = 0;


            // get the handle to the DigHookUserData object back from the IntPtr
            GCHandle hUserData = GCHandle.FromIntPtr(npUserDataPtr);
            // get a reference to the DigHookUserData object
            CMatroxCommon p_matrox_common = hUserData.Target as CMatroxCommon;

            try
            {
                //	エラー文字列を取得する

                //	エラー発生関数
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT_FCT, ErrorMessageFunction);
                //	エラー内容
                MIL.MappGetHookInfo(nEventId, MIL.M_MESSAGE + MIL.M_CURRENT, ErrorMessage);
                //	エラー内容詳細の文字列数
                MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_NB, ref NbSubCode);

                //　エラー内容の詳細文字列を取得する
                if (NbSubCode > 2)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_3 + MIL.M_MESSAGE, ErrorSubMessage3);
                }
                if (NbSubCode > 1)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_2 + MIL.M_MESSAGE, ErrorSubMessage2);
                }
                if (NbSubCode > 0)
                {
                    MIL.MappGetHookInfo(nEventId, MIL.M_CURRENT_SUB_1 + MIL.M_MESSAGE, ErrorSubMessage1);
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
                p_matrox_common.outputErrorLog(str_error);

                //	致命的なエラーかどうか判断する
                //	MdigProcess、xxxAllocで発生するエラーは全て致命的とする
                str_function = ErrorMessageFunction.ToString();
                //if (str_function.find("MdigProcess") != string::npos || str_function.find("Alloc") != string::npos)
                if (str_function.IndexOf("Alloc") != -1)
                {
                    p_matrox_common.m_bFatalErrorOccured = true;
                }

                return (MIL.M_NULL);
            }
            catch
            {
                //	エラーフックの例外エラー
                str_error = "Unknown Error";
                //	エラーをログ出力する
                p_matrox_common.outputErrorLog(str_error);

                return (MIL.M_NULL);
            }
        }
        public bool IsDiffMode()
        {
            return m_bNowDiffMode;
        }

    }
}


