using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;

namespace ImageMatrox
{
    public class CMatroxCamera : CMatroxCommon
    {
        GCHandle hUserData_ProcessingFunction;
        CMatroxCamera p_matrox;
        GCHandle hUserData_doThrough;
        MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;
        public CMatroxCamera()
        {
            
        }
        public int doThrough()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            //MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milOriginalImage);
            //MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milShowImage);
            //MIL.MbufAllocColor(m_milSys, 3, m_szImageSize.Width, m_szImageSize.Height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC + MIL.M_PACKED + MIL.M_BGR24, ref m_milMonoImage);

            if (m_bThroughFlg == false)
            {
                //	画像読み込み等でカメラのサイズと画像バッファのサイズが異なっている場合は、
                //	スルー前にカメラのサイズにバッファを合わせる
                if (m_szImageSizeForCamera.Width != m_szImageSize.Width || m_szImageSizeForCamera.Height != m_szImageSize.Height)
                {
                    reallocMilImage(m_szImageSizeForCamera);
                }
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    hUserData_doThrough = GCHandle.Alloc(this);
                    ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                    //	フック関数を使用する
                    MIL.MdigProcess(m_milDigitizer, m_milImageGrab, MAX_IMAGE_GRAB_NUM,
                                        MIL.M_START, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData_doThrough));
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
        public int doFreeze()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            if (m_bThroughFlg == true)
            {
                if (m_iBoardType != (int)MTX_TYPE.MTX_HOST)
                {
                    GCHandle hUserData = GCHandle.Alloc(this);
                    MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                    //	フック関数を使用する
                    MIL.MdigProcess(m_milDigitizer, m_milImageGrab, MAX_IMAGE_GRAB_NUM,
                                MIL.M_STOP + MIL.M_WAIT, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));
                    //MIL.MdigHalt(m_milDigitizer);
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
        protected MIL_INT ProcessingFunction(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            if (!IntPtr.Zero.Equals(npUserDataPtr))
            {
                MIL_ID mil_modified_image = MIL.M_NULL;

                nlHookType = 0;
                //　送られてきたポインタをマトロックスクラスポインタにキャスティングする
                hUserData_ProcessingFunction = GCHandle.FromIntPtr(npUserDataPtr);
                p_matrox = hUserData_ProcessingFunction.Target as CMatroxCamera;
                //　変更されたバッファIDを取得する
                MIL.MdigGetHookInfo(nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image);
                MIL.MbufCopy(mil_modified_image, CMatroxCamera.m_milOriginalImage);
                //	表示用画像バッファにコピーする
                if (p_matrox.IsDiffMode() == true)
                {
                    p_matrox.makeDiffImage();
                }
                else
                {
                    MIL.MbufCopy(mil_modified_image, CMatroxCamera.m_milShowImage);
                    MIL.MbufCopy(mil_modified_image, CMatroxCamera.m_milMonoImage);
                }

                // リスト化する(リングバッファ数以上あれば削除)
                m_lstImageGrab.Add(mil_modified_image);
                while (MAX_AVERAGE_IMAGE_GRAB_NUM < m_lstImageGrab.Count())
                {
                    m_lstImageGrab.RemoveAt(0);
                }
                //MIL.MimArith(mil_modified_image, MIL.M_NULL, CMatroxCamera.m_milShowImage, MIL.M_NOT);
            }
            return (0);
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
        public void getOneGrab()
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }
            if (m_iBoardType == (int)MTX_TYPE.MTX_HOST)
            {
                return;
            }
            //	フリーズ状態でのみ有効
            if (m_bThroughFlg == false)
            {
                //	今の画像を１つ前の画像として保存する
                MIL.MbufCopy(m_milShowImage, m_milPreShowImage);
                //	画像をGrabする
                MIL.MdigGrab(m_milDigitizer, m_milShowImage);
                MIL.MdigGrabWait(m_milDigitizer, MIL.M_GRAB_END);
                MIL.MbufCopy(m_milShowImage, m_milMonoImage);
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
        public int makeDiffImage()
        {
            //	直前と今の画像の絶対値差分を取る
            MIL.MimArith(m_milOriginalImage, m_milDiffOrgImage, m_milShowImage, MIL.M_SUB_ABS + MIL.M_SATURATION);
            return 0;
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
        public void getAveragedImageGrab(int niAverageNum, string ncsFilePath, bool nbSaveMono, bool nbSaveOnMemory)
        {
            bool b_now_through_flg;
            int i_loop;

            double StartTime, EndTime;
            StartTime = 0;
            EndTime = 0;

            b_now_through_flg = m_bThroughFlg;

            if (m_bMainInitialFinished == false)
            {
                return;
            }
            if (m_iBoardType == (int)MTX_TYPE.MTX_HOST)
            {
                return;
            }
            if (niAverageNum <= 0 || niAverageNum > 10)
            {
                return;
            }

            //	スルーであればフリーズする
            if (m_bThroughFlg == true)
            {
                doFreeze();
            }

            MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref StartTime);

            // 本クラスのポインター
            GCHandle hUserData = GCHandle.Alloc(this);
            // フック関数のポインタ
            MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);

            //	フック関数を使用する。指定枚数画像を取得する
            MIL.MdigProcess(m_milDigitizer, m_milAverageImageGrab, niAverageNum,
                                MIL.M_SEQUENCE, MIL.M_SYNCHRONOUS, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData));

            MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref EndTime);
            //時間をログ出力
            outputTimeLog("MdigProcess", (EndTime - StartTime) * 1000.0);

            //	平均化積算バッファクリア
            MIL.MbufClear(m_milAverageImageCalc, 0);

            //	平均化する
            for (i_loop = 0; i_loop < niAverageNum; i_loop++)
            {
                MIL.MimArith(m_milAverageImageGrab[i_loop], m_milAverageImageCalc, m_milAverageImageCalc, MIL.M_ADD);
            }
            MIL.MimArith(m_milAverageImageCalc, (double)niAverageNum, m_milAverageImageCalc, MIL.M_DIV_CONST);

            //	モノクロ(1バンド)出力
            if (nbSaveMono == true)
            {
                MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref StartTime);

                MIL_ID mil_save_image = MIL.M_NULL;
                MIL.MbufAllocColor(m_milSys, 1, m_szImageSize.Width, m_szImageSize.Height, 16 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref mil_save_image);
                MIL.MbufCopy(m_milAverageImageCalc, mil_save_image);

                MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref EndTime);
                //時間をログ出力
                outputTimeLog("BufCopy", (EndTime - StartTime) * 1000.0);
                MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref StartTime);

                //	ファイルに保存
                if (nbSaveOnMemory == false)
                {
                    MIL.MbufExport(ncsFilePath, MIL.M_BMP, mil_save_image);

                    MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref EndTime);
                    //時間をログ出力
                    outputTimeLog("ImageSave", (EndTime - StartTime) * 1000.0);
                }
                //	メモリ上に保存
                else
                {
                    MIL.MappTimer(MIL.M_DEFAULT, MIL.M_TIMER_READ, ref EndTime);
                    //時間をログ出力
                    outputTimeLog("ImageSaveOnMemory", (EndTime - StartTime) * 1000.0);
                }

                MIL.MbufFree(mil_save_image);
            }
            //	カラー(3バンド)出力
            else
            {
                //	ファイルに保存
                MIL.MbufExport(ncsFilePath, MIL.M_BMP, m_milAverageImageCalc);
            }

            //	スルーだったらスルーに戻す
            if (b_now_through_flg == true)
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
        public int setGainAndExposureTime(double ndGain, double ndExposureTime)
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256); 
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                int i_gain_raw = (int)ndGain;
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_raw);
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTimeAbs", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            }

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
        public int setBlackLevel(double ndBlackLevel)
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                int i_black_level_raw = (int)ndBlackLevel;
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "BlackLevelRaw", MIL.M_TYPE_MIL_INT32, ref i_black_level_raw);
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "BlackLevel", MIL.M_TYPE_DOUBLE, ref ndBlackLevel);
            }

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
        public int setGain(double ndGain)
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                int i_gain_raw = (int)ndGain;
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_raw);
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "Gain", MIL.M_TYPE_DOUBLE, ref ndGain);
            }

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
        public int setExposureTime(double ndExposureTime)
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTimeAbs", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "ExposureTime", MIL.M_TYPE_DOUBLE, ref ndExposureTime);
            }
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
        public int getBlackLevelMaxMin(ref double npdMax, ref double npdMin)
        {

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                int i_black_level_raw_max = 0;
                int i_black_level_raw_min = 0;
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "BlackLevelRaw", MIL.M_TYPE_MIL_INT32, ref i_black_level_raw_max);
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "BlackLevelRaw", MIL.M_TYPE_MIL_INT32, ref i_black_level_raw_min);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "BlackLevelRaw", MIL.M_TYPE_MIL_INT32, ref i_black_level_raw_max);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "BlackLevelRaw", MIL.M_TYPE_MIL_INT32, ref i_black_level_raw_min);
                npdMax = (double)i_black_level_raw_max;
                npdMin = (double)i_black_level_raw_min;
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "BlackLevelRaw", MIL.M_TYPE_DOUBLE, ref npdMax);
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "BlackLevelRaw", MIL.M_TYPE_DOUBLE, ref npdMin);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "BlackLevel", MIL.M_TYPE_DOUBLE, npdMax);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "BlackLevel", MIL.M_TYPE_DOUBLE, npdMin);
            }
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
        public int getGainMaxMin(ref double npdMax, ref double npdMin)
        {

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                int i_gain_max = 0;
                int i_gain_min = 0;
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_max);
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_min);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_max);
                //MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "GainRaw", MIL.M_TYPE_MIL_INT32, ref i_gain_min);
                npdMax = (double)i_gain_max;
                npdMin = (double)i_gain_min;
            }
            // Point grey
            else
            {
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "Gain", MIL.M_TYPE_DOUBLE,ref npdMax);
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "Gain", MIL.M_TYPE_DOUBLE,ref npdMin);
            }

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
        public int getExposureTimeMaxMin(ref double npdMax, ref double npdMin)
        {

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
                {
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "ExposureTimeAbs", MIL.M_TYPE_DOUBLE,ref npdMax);
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "ExposureTimeAbs", MIL.M_TYPE_DOUBLE,ref npdMin);
            }
            // Point grey
            else
            {
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MAX, "ExposureTime", MIL.M_TYPE_DOUBLE,ref npdMax);
                MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_MIN, "ExposureTime", MIL.M_TYPE_DOUBLE,ref npdMin);
            }

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
        public int setTriggerModeOff()
        {

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, "TriggerMode", MIL.M_TYPE_ENUMERATION, "Off");
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, "TriggerMode", MIL.M_TYPE_ENUMERATION, "Off");
            }

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
        public int setTriggerModeSoftware()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, "DeviceVendorName", MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerMode"), MIL.M_TYPE_ENUMERATION, ("On"));
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerSource"), MIL.M_TYPE_ENUMERATION, ("Software"));
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerMode"), MIL.M_TYPE_ENUMERATION, ("On"));
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerSource"), MIL.M_TYPE_ENUMERATION, ("Software"));
            }

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
        public int setTriggerModeHardware(string nstrTrigger)
        {

            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, ("DeviceVendorName"), MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerMode"), MIL.M_TYPE_ENUMERATION, ("On"));
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerSource"), MIL.M_TYPE_ENUMERATION, (nstrTrigger));
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerMode"), MIL.M_TYPE_ENUMERATION, ("On"));
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_VALUE_AS_STRING, ("TriggerSource"), MIL.M_TYPE_ENUMERATION, (nstrTrigger));
            }

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
        public int executeSoftwareTrigger()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }
            else if (m_iBoardType != (int)MTX_TYPE.MTX_GIGE)
            {
                return 0;
            }

            StringBuilder str_vendor_name = new StringBuilder(256);
            MIL.MdigInquireFeature(m_milDigitizer, MIL.M_FEATURE_VALUE, ("DeviceVendorName"), MIL.M_TYPE_STRING, str_vendor_name);

            //	Basler
            if ((str_vendor_name.ToString()).IndexOf("Basler") != -1)
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_EXECUTE, ("TriggerSoftware"), MIL.M_TYPE_COMMAND, MIL.M_NULL);
            }
            // Point grey
            else
            {
                MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_EXECUTE, ("TriggerSoftware"), MIL.M_TYPE_COMMAND, MIL.M_NULL);
            }

            return 0;

        }
    }
}

