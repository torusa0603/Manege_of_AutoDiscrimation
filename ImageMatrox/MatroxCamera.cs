using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Runtime.InteropServices;
using System.Drawing;


namespace ImageMatrox
{
    public class CMatroxCamera : CMatroxCommon
    {
        GCHandle hUserData_ProcessingFunction;
        CMatroxCamera p_matrox;
        GCHandle hUserData_doThrough;
        MIL_DIG_HOOK_FUNCTION_PTR ProcessingFunctionPtr;
        public bool m_bWritableShowImageBaffa;
        public bool m_ThroughSimple = false;

        public CMatroxCamera()
        {
            
        }
        /// <summary>
        /// フック関数を利用して画像表示を開始する
        /// </summary>
        /// <returns></returns>
        public int doThrough()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

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
                    m_bWritableShowImageBaffa = true;

                    hUserData_doThrough = GCHandle.Alloc(this);
                    ProcessingFunctionPtr = new MIL_DIG_HOOK_FUNCTION_PTR(ProcessingFunction);
                    //	フック関数を使用する
                    MIL.MdigProcess(m_milDigitizer, m_milImageGrab, MAX_IMAGE_GRAB_NUM,
                                        MIL.M_START, MIL.M_DEFAULT, ProcessingFunctionPtr, GCHandle.ToIntPtr(hUserData_doThrough));
                }
                m_ThroughSimple = false;
                m_bThroughFlg = true;
            }

            return 0;
        }

        /// <summary>
        /// MILの関数(MdigGrabContinuous)を利用して画像表示を開始する
        /// </summary>
        /// <returns></returns>
        public int doThroughSimple()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            if (m_bThroughFlg == false)
            {
                //	画像読み込み等でカメラのサイズと画像バッファのサイズが異なっている場合は、
                //	スルー前にカメラのサイズにバッファを合わせる
                if (m_szImageSizeForCamera.Width != m_szImageSize.Width || m_szImageSizeForCamera.Height != m_szImageSize.Height)
                {
                    reallocMilImage(m_szImageSizeForCamera);
                }
                MIL.MdigGrabContinuous(m_milDigitizer, m_milShowImage);
                m_ThroughSimple = true;
                m_bThroughFlg = true;
            }

            return 0;
        }

        /// <summary>
        /// フリーズを行う
        /// </summary>
        /// <returns></returns>
        public int doFreeze()
        {
            if (m_bMainInitialFinished == false)
            {
                return -1;
            }

            if (m_bThroughFlg == true)
            {
                if (m_ThroughSimple) 
                {
                    MIL.MdigHalt(m_milDigitizer);
                }
                else
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
                }

                m_bThroughFlg = false;
            }

            return 0;
        }

        /// <summary>
        /// フック関数-カメラから画像を取得する
        /// </summary>
        /// <param name="nlHookType"></param>
        /// <param name="nEventId"></param>
        /// <param name="npUserDataPtr"></param>
        /// <returns></returns>
        protected MIL_INT ProcessingFunction(MIL_INT nlHookType, MIL_ID nEventId, IntPtr npUserDataPtr)
        {
            if (!IntPtr.Zero.Equals(npUserDataPtr))
            {
                if (m_bWritableShowImageBaffa)
                {
                    m_bWritableShowImageBaffa = false;

                    MIL_ID mil_modified_image = MIL.M_NULL;

                    nlHookType = 0;
                    //　送られてきたポインタをマトロックスクラスポインタにキャスティングする
                    hUserData_ProcessingFunction = GCHandle.FromIntPtr(npUserDataPtr);
                    p_matrox = hUserData_ProcessingFunction.Target as CMatroxCamera;
                    //　変更されたバッファIDを取得する
                    MIL.MdigGetHookInfo(nEventId, MIL.M_MODIFIED_BUFFER + MIL.M_BUFFER_ID, ref mil_modified_image);
                    MIL.MbufCopy(mil_modified_image, CMatroxCamera.m_milOriginalImage);
                    MIL.MbufCopy(CMatroxCamera.m_milOriginalImage, CMatroxCamera.m_milShowImage);
                    if (CMatroxCamera.m_bDiffPicDisciminateMode)
                    {
                        if(CMatroxCamera.m_milDiffOrgImage != MIL.M_NULL)
                        {
                            p_matrox.makeDiffImage();
                            MIL.MbufCopy(CMatroxCamera.m_milDiffDstImage, CMatroxCamera.m_milMonoImage);
                            p_matrox.discriminateDiffPic();
                        }
                        p_matrox.setDiffOrgImage(null);
                    }
                    else
                    {
                        if (p_matrox.IsDiffMode() == 0)
                        {
                            MIL.MbufCopy(mil_modified_image, CMatroxCamera.m_milMonoImage);
                        }
                        else
                        {
                            p_matrox.makeDiffImage();
                            MIL.MbufCopy(CMatroxCamera.m_milDiffDstImage, CMatroxCamera.m_milMonoImage);
                            if (p_matrox.IsDiffMode() == 1)
                            {
                                //	表示用画像バッファにコピーする
                                MIL.MbufCopy(CMatroxCamera.m_milDiffDstImage, CMatroxCamera.m_milShowImage);
                            }
                        }
                    }

                    // リスト化する(リングバッファ数以上あれば削除)
                    m_lstImageGrab.Add(mil_modified_image);
                    while (MAX_AVERAGE_IMAGE_GRAB_NUM < m_lstImageGrab.Count())
                    {
                        m_lstImageGrab.RemoveAt(0);
                    }
                    //MIL.MimArith(mil_modified_image, MIL.M_NULL, CMatroxCamera.m_milShowImage, MIL.M_NOT);
                    m_bWritableShowImageBaffa = true;
                }
                else
                {

                }
            }
            return (0);
        }

        //private bool IsDiffPicDisciminateMode()
        //{
        //    return m_bDiffPicDisciminateMode;
        //}

        private void discriminateDiffPic()
        {
            bool b_dicriminate_result = discriminantDiffImage();
            if (m_bDiffState != b_dicriminate_result)
            {
                m_bDiffState = b_dicriminate_result;
                if (b_dicriminate_result)
                {
                    m_evDiffEnable_True?.Invoke();
                }
                else
                {
                    m_evDiffEnable_False?.Invoke();
                }
            }

        }

        private bool discriminantDiffImage()
        {
            bool i_ret = false;
            MIL_ID mil_stat_result = MIL.M_NULL;
            MIL_INT i_average_value = MIL.M_NULL;

            MIL.MimAllocResult(m_milSys, MIL.M_DEFAULT, MIL.M_STAT_LIST, ref mil_stat_result);
            MIL.MimStat(m_milMonoImage, mil_stat_result, MIL.M_MEAN, MIL.M_NULL, MIL.M_NULL, MIL.M_NULL);
            MIL.MimGetResult(mil_stat_result, MIL.M_MEAN + MIL.M_TYPE_MIL_INT, ref i_average_value);
            //	メモリ解放
            MIL.MimFree(mil_stat_result);
            // 差分が設定されている値に対して大きいかの判定を返す
            if (m_iDiscriminateDiffPicValue < (int)i_average_value)
            {
                i_ret = true;
            }
            else
            {
                i_ret = false;
            }
            return i_ret;
        }

























        /// <summary>
        /// 画像を一枚取得する
        /// </summary>
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

        /// <summary>
        /// 差分画像を作成する
        /// </summary>
        /// <returns></returns>
        public int makeDiffImage()
        {
            //	直前と今の画像の絶対値差分を取る
            MIL.MimArith(m_milOriginalImage, m_milDiffOrgImage, m_milDiffDstImage, MIL.M_SUB_ABS + MIL.M_SATURATION);
            return 0;
        }

        /// <summary>
        /// 平均画像を取得する____
        /// メモリー上では"m_milAverageImageCalc"に保存される
        /// </summary>
        /// <param name="niAverageNum">平均化に使用する画像枚数</param>
        /// <param name="ncsFilePath">平均画像の保存ファイルパス</param>
        /// <param name="nbSaveMono">平均画像のモノクロ化を行うかのフラグ</param>
        /// <param name="nbSaveOnMemory">モノクロ平均画像をメモリー上にのみ保存を行うかのフラグ</param>
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

        /// <summary>
        /// ゲインと露光時間を設定する
        /// </summary>
        /// <param name="ndGain">ゲイン値</param>
        /// <param name="ndExposureTime">露光時間</param>
        /// <returns></returns>
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

        /// <summary>
        /// ブラックレベルを設定する
        /// </summary>
        /// <param name="ndBlackLevel">ブラックレベル</param>
        /// <returns></returns>
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

        /// <summary>
        /// ゲインを設定する
        /// </summary>
        /// <param name="ndGain">ゲイン値</param>
        /// <returns></returns>
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

        /// <summary>
        /// 露光時間を設定する
        /// </summary>
        /// <param name="ndExposureTime">露光時間</param>
        /// <returns></returns>
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

        /// <summary>
        /// ブラックレベルの最大値、最小値を取得する
        /// </summary>
        /// <param name="npdMax">最大値</param>
        /// <param name="npdMin">最小値</param>
        /// <returns></returns>
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

        /// <summary>
        /// ゲインの最大値、最小値を取得する
        /// </summary>
        /// <param name="npdMax">最大値</param>
        /// <param name="npdMin">最小値</param>
        /// <returns></returns>
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

        /// <summary>
        /// 露光時間の最大値、最小値を取得する
        /// </summary>
        /// <param name="npdMax">最大値</param>
        /// <param name="npdMin">最小値</param>
        /// <returns></returns>
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

        /// <summary>
        /// トリガーモードをオフにする
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// トリガーモードのソフトウェア設定____
        /// MILHelpのmilgige.cppを参考にする
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// トリガーモードのハードウェア設定
        /// </summary>
        /// <param name="nstrTrigger">トリガ名(Ex.acA1300-30gmの場合Line1)</param>
        /// <returns></returns>
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

        /// <summary>
        /// ソフトウェアトリガー実行
        /// </summary>
        /// <returns></returns>
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

