using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace ImageMatrox
{
    public class extern_main : GlobalStructure
    {
        CMatroxCommon pMatroxCommon;
        CMatroxCamera pMatroxCamera;
        CMatroxGraphic pMatroxGraphic;
        CMatroxImageProcess pMatroxImageProcess;
        CancellationTokenSource token_source;
        CancellationToken cansel_token;

        public event EventHandlerVoid DiffImageEvent;
        bool DiffImageEventEnable;

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~extern_main()
        {
            token_source.Cancel();
        }

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
        public int sifInitializeImageProcess(IntPtr nhDispHandle, string nchrSettingPath)
        {
            int i_ret = 0;
            string str_setting_path;

            //	オブジェクト作成
            if (pMatroxCommon == null)
            {
                pMatroxCommon = new CMatroxCommon();
            }
            if (pMatroxCamera == null)
            {
                pMatroxCamera = new CMatroxCamera();
            }
            if (pMatroxGraphic == null)
            {
                pMatroxGraphic = new CMatroxGraphic();
            }
            if (pMatroxImageProcess == null)
            {
                pMatroxImageProcess = new CMatroxImageProcess();
            }

            //	初期化ファイル格納フォルダ
            if (nchrSettingPath == null)
            {
                str_setting_path = "";
            }
            else
            {
                str_setting_path = nchrSettingPath;
            }

            //	マトロックスの初期化を行う
            i_ret = pMatroxCommon.Initial(nhDispHandle, str_setting_path);
            if (i_ret == 0)
            {
                //	描画先を通常のバッファにする
                pMatroxGraphic.changeDrawDestination(0);
            }

            pMatroxImageProcess.init();

            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
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
        public void sifCloseImageProcess()
        {
            if (pMatroxImageProcess != null)
            {
                //	終了処理
                pMatroxImageProcess.close();
            }

            //	マトロックスの終了処理を行う
            if (pMatroxCommon != null)
            {
                if (pMatroxCommon.getThoughStatus() == true)
                {
                    if (pMatroxCamera != null)
                    {
                        pMatroxCamera.setTriggerModeOff();
                        pMatroxCamera.doFreeze();
                    }
                }
                pMatroxCommon.CloseMatrox();
            }

            //	オブジェクト破棄
            if (pMatroxImageProcess != null)
            {
                pMatroxImageProcess = null;
            }
            if (pMatroxCommon != null)
            {
                pMatroxCommon = null;
            }
            if (pMatroxCamera != null)
            {
                pMatroxCamera = null;
            }
            if (pMatroxGraphic != null)
            {
                pMatroxGraphic = null;
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
        public int sifGetPixelValueOnPosition(POINT nNowPoint, out int RValue, out int GValue, out int BValue)
        {
            int[] rgb = { 0, 0, 0 };
            int i_ret;

            //	指定した画像上のポイントの輝度値を取得する
            i_ret = pMatroxCommon.getPixelValueOnPosition(nNowPoint, ref rgb[0], ref rgb[1], ref rgb[2]);
            RValue = rgb[0];
            GValue = rgb[1];
            BValue = rgb[2];

            return i_ret;
        }

        public double sifGetZoomMag()
        {
            double d_zoom_mag;

            d_zoom_mag = pMatroxCommon.getZoomMag();
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
        public int sifThrough()
        {

            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            //画像をスルーにする
            pMatroxCamera.doThrough();

            return 0;
        }

        public int sifThroughSimple()
        {

            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            //画像をスルーにする
            pMatroxCamera.doThroughSimple();

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
        public int sifFreeze()
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            //画像をフリーズする
            pMatroxCamera.doFreeze();

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
        public void sifGetOneGrab()
        {
            //画像をフリーズする
            pMatroxCamera.doFreeze();
            //	1回grabする
            pMatroxCamera.getOneGrab();
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
        public void sifDrawRectangle(POINT nptLeftTopPoint, POINT nptRightBottomPoint)
        {
            pMatroxGraphic.setGraphicColor((int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_GREEN);
            pMatroxGraphic.DrawRectangle(nptLeftTopPoint, nptRightBottomPoint);
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
        public void sifDrawClear()
        {
            pMatroxGraphic.clearAllGraphic();
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
        public void sifSaveImage(RECT nrctSaveArea, bool nAllSaveFlg, string nchrFilePath, bool nbSaveMono)
        {
            string cs_file_path;
            cs_file_path = nchrFilePath;

            pMatroxCommon.saveImage(nrctSaveArea, nAllSaveFlg, cs_file_path, nbSaveMono);
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
        public void sifSaveOrigImage(RECT nrctSaveArea, bool nAllSaveFlg, string nchrFilePath)
        {
            string cs_file_path;
            cs_file_path = nchrFilePath;

            pMatroxCommon.saveOrigImage(nrctSaveArea, nAllSaveFlg, cs_file_path);

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
        public void sifDrawCross(int niCenterX, int niCenterY)
        {
            //	cGraphic.setGraphicColor( GRAPHIC_COLOR_YELLOW );

            pMatroxGraphic.setGraphicColor((int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_YELLOW);
            //	太さ2で書く。(デジタルズームで縮小されている可能性があるので)

            POINT pt_point_s, pt_point_e;
            pt_point_s.x = niCenterX - 5;
            pt_point_s.y = niCenterY;
            pt_point_e.x = niCenterX + 5;
            pt_point_e.y = niCenterY;
            pMatroxGraphic.DrawLine(pt_point_s, pt_point_e);

            pt_point_s.x = niCenterX;
            pt_point_s.y = niCenterY - 5;
            pt_point_e.x = niCenterX;
            pt_point_e.y = niCenterY + 5;
            pMatroxGraphic.DrawLine(pt_point_s, pt_point_e);

            niCenterY = niCenterY + 1;
            pt_point_s.x = niCenterX - 5;
            pt_point_s.y = niCenterY;
            pt_point_e.x = niCenterX + 5;
            pt_point_e.y = niCenterY;
            pMatroxGraphic.DrawLine(pt_point_s, pt_point_e);

            niCenterY = niCenterY - 1;
            niCenterX = niCenterX + 1;
            pt_point_s.x = niCenterX;
            pt_point_s.y = niCenterY - 5;
            pt_point_e.x = niCenterX;
            pt_point_e.y = niCenterY + 5;
            pMatroxGraphic.DrawLine(pt_point_s, pt_point_e);
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
        public void sifDrawLine(POINT nptStartPoint, POINT nptEndPoint)
        {
            pMatroxGraphic.setGraphicColor((int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_WHITE);
            pMatroxGraphic.DrawLine(nptStartPoint, nptEndPoint);
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
        public bool sifGetThoughStatus()
        {
            return pMatroxCommon.getThoughStatus();
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
        public void sifSetOriginImage()
        {
            pMatroxCommon.setOriginImage();
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
        public int sifSetMag(double ndMag)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            pMatroxCommon.setShowImageMag(ndMag);

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
        public Size sifGetImageSize()
        {
            return pMatroxCommon.getImageSize();
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
        public int sifLoadImage(string nchLoadImageFilePath)
        {
            string cs_load_image_path;
            cs_load_image_path = nchLoadImageFilePath;

            if (pMatroxCommon.getThoughStatus() == true)
            {
                pMatroxCamera.doFreeze();
            }
            return pMatroxCommon.loadImage(cs_load_image_path);

        }

        /*------------------------------------------------------------------------------------------
            1.日本語名
                差分用オリジナル画像を現在表示されている画像で登録し、差分モードに移行する

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
        public int sifsetDiffOrgImage()
        {
            return pMatroxCommon.setDiffOrgImage(1);

        }
        /*------------------------------------------------------------------------------------------
            1.日本語名
                差分画像の解析を行う

            2.パラメタ説明
                niSleepTime:待ち時間

            3.概要
                画像をロードする

            4.機能説明
                画像をロードする

            5.戻り値
                なし

            6.備考
                なし
        ------------------------------------------------------------------------------------------*/
        public void sifanalyzeDiffImage(int niSleepTime, int niScore)
        {
            token_source = new CancellationTokenSource();
            cansel_token = token_source.Token;

            //TimerCallback timerDelegate = new TimerCallback(analyzeDiffImage);
            Thread.Sleep(1000);

            DiffImageEventEnable = true;
            pMatroxCommon.setDiffOrgImage(0);
            //Timer timer = new Timer(timerDelegate, null, niSleepTime, niSleepTime);
            Task.Run(() => analyzeDiffImage(cansel_token, niSleepTime, niScore));
        }

        /*------------------------------------------------------------------------------------------
            1.日本語名
                差分画像の解析を行う

            2.パラメタ説明
                niSleepTime:待ち時間

            3.概要
                画像をロードする

            4.機能説明
                画像をロードする

            5.戻り値
                なし

            6.備考
                なし
        ------------------------------------------------------------------------------------------*/
        private void analyzeDiffImage(CancellationToken n_CaseToken, int niSleepTime, int niScore)
        {
            while(true)
            {
                if (DiffImageEventEnable) 
                {
                    int i_ret = -1;
                    i_ret = pMatroxImageProcess.discriminantDiffImage(niScore);
                    if (i_ret == 0)
                    {
                        DiffImageEvent();
                    }
                    pMatroxCommon.setDiffOrgImage(0);
                }
                Thread.Sleep(niSleepTime);
                if (n_CaseToken.IsCancellationRequested)
                {
                    // ループを終了します。
                    break;
                }
            }
        }

        public void ChangeDiffImageEventEnable()
        {
            DiffImageEventEnable = !DiffImageEventEnable;
        }


        /*------------------------------------------------------------------------------------------
            1.日本語名
                差分モードを解除する

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
        public void sifresetDiffMode()
        {
            pMatroxCommon.resetDiffMode();

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
        public int sifSetGainAndExposureTime(double ndGain, double ndExposureTime)
        {
            return pMatroxCamera.setGainAndExposureTime(ndGain, ndExposureTime);
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
        public int sifGetAveragedImageGrab(int niAverageNum, string nchrFilePath, bool nbSaveMono, bool nbSaveOnMemory)
        {
            string cs_file_path;
            cs_file_path = nchrFilePath;

            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }

            //	1回grabする
            pMatroxCamera.getAveragedImageGrab(niAverageNum, cs_file_path, nbSaveMono, nbSaveOnMemory);

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
        public int sifSetGain(double ndGain)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;

            }
            return pMatroxCamera.setGain(ndGain);
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
        public int sifSetExposureTime(double ndExposureTime)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            return pMatroxCamera.setExposureTime(ndExposureTime);
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
        public int sifSetBlackLevel(double ndBlackLevel)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            return pMatroxCamera.setBlackLevel(ndBlackLevel);
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
        public int sifGetBlackLevelMaxMin(double npdMax, double npdMin)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            return pMatroxCamera.getBlackLevelMaxMin(ref npdMax, ref npdMin);
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
        public int sifGetGainMaxMin(double npdMax, double npdMin)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }

            return pMatroxCamera.getGainMaxMin(ref npdMax, ref npdMin);
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
        public int sifGetExposureTimeMaxMin(double npdMax, double npdMin)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            return pMatroxCamera.getExposureTimeMaxMin(ref npdMax, ref npdMin);
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
        public int sifSetDispHandleForInspectionResult(IntPtr nhDispHandleForInspectionResult)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }

            return pMatroxCommon.setDispHandleForInspectionResult(nhDispHandleForInspectionResult);
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
        public int sifSetMagForInspectionResult(double ndMag)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }

            pMatroxCommon.setZoomMagForInspectionResult(ndMag);
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
        public double sifGetZoomMagForInspectionResult()
        {
            double d_zoom_mag;

            d_zoom_mag = pMatroxCommon.getZoomMagForInspectionResult();
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
        public int sifGetAveragePixelValueOnRegion(RECT nrctRegion)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }
            return pMatroxCommon.getAveragePixelValueOnRegion(nrctRegion);
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
        public int sifSetDisplayHandle(IntPtr nhDispHandle)
        {
            //	致命的なエラーチェック
            if (pMatroxCommon.getFatalErrorOccured() == true)
            {
                return FATAL_ERROR_ID;
            }

            pMatroxCommon.SetDisplayHandle(nhDispHandle);

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
        public bool sifGetFatalErrorOccured()
        {
            return pMatroxCommon.getFatalErrorOccured();
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
        public int sifSetTriggerModeOff()
        {
            return pMatroxCamera.setTriggerModeOff();
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
        public int sifSetTriggerModeSoftware()
        {
            return pMatroxCamera.setTriggerModeSoftware();
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
        public int sifSetTriggerModeHardware(string nszTrigger)
        {
            return pMatroxCamera.setTriggerModeHardware(nszTrigger);
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
        public int sifExecuteSoftwareTrigger()
        {
            return pMatroxCamera.executeSoftwareTrigger();
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
        public int sifGetMonoBitmapData(long nlArySize, byte[] npbyteData)
        {
            return pMatroxCommon.getMonoBitmapData(nlArySize, ref npbyteData);
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
        public int sifSampleInspection(string nszFoldeName)
        {
            int i_ret = -1;

            while (true)
            {
                // ソフトトリガ設定
                if (0 != pMatroxCamera.executeSoftwareTrigger())
                {
                    break;
                }

                // サンプル動作
                CSampleInspection cSampleInspection = new CSampleInspection();
                if (0 != cSampleInspection.execInspection(nszFoldeName))
                {
                    break;
                }

                // トリガオフ設定
                if (0 != pMatroxCamera.setTriggerModeOff())
                {
                    break;
                }
                break;
            }

            return i_ret;
        }










    }
}
