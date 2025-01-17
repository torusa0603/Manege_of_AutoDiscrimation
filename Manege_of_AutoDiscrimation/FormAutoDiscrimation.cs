﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Management;
using CCSLightController;
using Manege_of_AutoDiscrimation.Param;



namespace Manege_of_AutoDiscrimation
{
    public partial class FormAutoDiscrimation : Form
    {

        #region"メンバー変数"
        CParaFormMain m_cParaFormMain = CParaFormMain.getInstance();
        
        // Log処理
        private LogBase.CLogBase m_cLogError = new LogBase.CLogBase(LogKind.Error);     // エラーログ
        private LogBase.CLogBase m_cLogExecute = new LogBase.CLogBase(LogKind.Execute); // 実行ログ
        private LogBase.CLogBase m_cLogCamera = new LogBase.CLogBase(LogKind.Camera);   // カメラ制御クラス用ログ
        private CameraControl.CCameraControlBase m_cCameraControlBase = null;           //カメラ制御クラス
        
        public Action evAnalyzePicture;                         //マニュアルモード時解析イベントハンドラ
        string m_strPythonFolderPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\PythonFile";   //pythonディレクトリ
        
        public static Parameter m_csParameter;
        static bool m_bInoculationEnable = true;                // 判定可能かどうかを示す
        static bool m_bJudgementFinish = false;                 // 判定結果を出力したか？
        private Thread m_ThreadWatchResultDisp;

        SocketCommunication m_cSocketCommunication;             // ソケット通信用のクラス
        SocketCommunication m_cSocketCommunicationForPython;    // pythonとのソケット通信用のクラス
        CBaseCCSLight m_cBaseCCSLight;                          // 照明通信用のクラス
        Process myProcess;                                      // pythonを起動するプロセス

        FormResultPicture form_result_picture;                  // 検査中に表示されるフォーム
        
        #endregion

        #region "   プログラム開始時処理                            "
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormAutoDiscrimation()
        {
            InitializeComponent();
        }
        #endregion

        #region "   フォームロード処理                              "
        /// <summary>
        /// フォームロード処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAutoDiscrimation_Load(object sender, EventArgs e)
        {
            try
            {
                // パラメーターの読み込み
                m_csParameter = new Parameter();
                int i_ret = CParameterIO.ReadParameter(CDefine.PRMFile, ref m_csParameter);
                if (i_ret != 0)
                {
                    MessageBox.Show("パラメーターファイルがありませんでした。");
                    this.Close();
                }

                // Paramクラスへの情報設定初期化
                m_cParaFormMain.setProductName(Application.ProductName);
                m_cParaFormMain.setComment(Application.ExecutablePath);
                // Paramファイル読み込み
                string str_log;
                if (false == m_cParaFormMain.readXmlFile(out str_log))
                {
                    m_cLogError.outputLog(str_log);
                }
                // ログクラス初期設定
                initLog();


                // カメラオープン
                i_ret = IniCamera();
                if (i_ret != 0)
                {
                    MessageBox.Show("カメラオープンに失敗しました");
                    //this.Close();
                }


                // StartPG通信
                // ソケットクラスオープン
                m_cSocketCommunication = new SocketCommunication();
                int intCount = 0;
                
                // 10回試してだめならあきらめる
                while (intCount < 10)
                {
                    i_ret = m_cSocketCommunication.Init(this, "", m_csParameter.PortNumber, 1);
                    if (i_ret == 0)
                    {
                        break;
                    }
                    intCount++;
                    Thread.Sleep(1000);
                }
                if (i_ret != 0)
                {
                    MessageBox.Show("ソケットオープンAに失敗しました");
                    this.Close();
                }
                m_cSocketCommunication.evCommandReceive += CommandReceiveAction;
                m_cSocketCommunication.evSocketClose += ClosedServer;


                // 検査用pythonを起動
                // 実行させたいPythonファイルのパス
                string myPythonApp = "PythonFile\\AutoDiscriminate.py";
                // プロセスを起動
                myProcess = new Process
                {
                    StartInfo = new ProcessStartInfo("python.exe")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        Arguments = $"{myPythonApp} {m_strPythonFolderPath}"
                    }
                };
                myProcess.Start();

                // プロセスがIdle状態になるまで待機 --- Windowを持たないためNG
                //myProcess.WaitForInputIdle();
                // プロセスの起動を確認する
                intCount = 0;
                while(Process.GetProcessesByName("Python").Length <= 0)
                {
                    if (intCount > 10)
                    {
                        break;
                    }
                    intCount++;
                    Thread.Sleep(200);
                }
                // 念のため、もう一息(3sec)
                for (int iLoop = 0; iLoop < 3; iLoop++)
                {
                    Thread.Sleep(1000);
                }

                // pythonファイルとのソケット通信を開始
                m_cSocketCommunicationForPython = new SocketCommunication();
                int i_port_number = 50000; // 後で決める
                string str_ip_adress = "192.168.3.10";
                intCount = 0;
                // 10回試してだめならあきらめる
                while (intCount < 10)
                {
                    i_ret = m_cSocketCommunicationForPython.Init(this, str_ip_adress, i_port_number, 0);
                    if (i_ret == 0)
                    {
                        break;
                    }
                    intCount++;
                    Thread.Sleep(1000);
                }
                if (i_ret != 0)
                {
                    MessageBox.Show("ソケットオープンBに失敗しました");
                    this.Close();
                }
                m_cSocketCommunicationForPython.evCommandReceive += CommandReceiveActionByPython;
                m_cSocketCommunicationForPython.evSocketClose += ClosedPython;


                m_cBaseCCSLight = new CPODCommand();

                // ライト通信をはじめに開通しておく
                i_ret = m_cBaseCCSLight.openLight(m_csParameter.LightIPAdress, m_csParameter.LightPortNumber);
                i_ret = m_cBaseCCSLight.closeLight();

            }
            catch (Exception ex)
            {
                m_cLogExecute.outputLog("FormAutoDiscrimation_Load function is catch error. " + ex.Message);
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {

            }
        }
        #endregion

        #region "   フォーム表示処理                                "
        /// <summary>
        /// ダイアログ表示処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAutoDiscrimation_Shown(object sender, EventArgs e)
        {

            label1.Text = $"{CDefine.CCondition.Color[m_csParameter.ConditionColor]}色で" +
                $"{CDefine.CCondition.Size[m_csParameter.ConditionSize]}の球を{m_csParameter.ConditionNumber}個撮ろう！";

            //this.FormBorderStyle = FormBorderStyle.None;
            // 全画面表示する
            this.WindowState = FormWindowState.Maximized;

        }
        #endregion

        #region "   フォーム終了処理                                "
        private void FormAutoDiscrimation_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_cLogExecute.outputLog("Form closing!");
            if (myProcess != null)
            {
                m_cLogExecute.outputLog("Send command 0");
                m_cSocketCommunicationForPython.SendCommand(0);
            }
            CParameterIO.SaveParameter(CDefine.PRMFile, m_csParameter);
            m_cLogExecute.outputLog("SocketA close...");
            m_cSocketCommunication.Close();
            m_cLogExecute.outputLog("SocketB close...");
            m_cSocketCommunicationForPython.Close();
            m_cLogExecute.outputLog("Camera close...");
            m_cCameraControlBase.close();
            m_cLogExecute.outputLog("Light close...");
            if (m_cBaseCCSLight != null)
            {
                m_cBaseCCSLight.openLight(m_csParameter.LightIPAdress, m_csParameter.LightPortNumber);
                ChangeLightState(false);

                m_cBaseCCSLight = null;
            }
        }

        #endregion


        #region "   Logクラス初期化                                 "
        /// <summary>
        /// ログクラス初期化
        /// </summary>
        private void initLog()
        {
            // エラーログ初期化
            string str_temp = LogKind.Error;
            m_cLogError.Enable = m_cParaFormMain.EnableLogError;            // ログを残すか
            m_cLogError.FolderName = "log\\" + str_temp;                    // フォルダ名
            m_cLogError.FileName = str_temp;                                // ファイル名
            // 実行ログ初期化
            str_temp = LogKind.Execute;
            m_cLogExecute.Enable = m_cParaFormMain.EnableLogExecute;        // ログを残すか
            m_cLogExecute.FolderName = "log\\" + str_temp;                  // フォルダ名
            m_cLogExecute.FileName = str_temp;                              // ファイル名
            str_temp = Application.ExecutablePath;
            str_temp = System.IO.Path.GetFileNameWithoutExtension(str_temp);
            m_cLogExecute.outputLog("Start of " + str_temp + ".");
            // カメラ制御用ログ初期化
            str_temp = LogKind.Camera;
            m_cLogCamera.Enable = m_cParaFormMain.EnableLogCamera;          // ログを残すか
            m_cLogCamera.FolderName = "log\\" + str_temp;                   // フォルダ名
            m_cLogCamera.FileName = str_temp;                               // ファイル名
        }

        #endregion

        #region "   判定処理                                        "

        #region "   判定処理-前半                                   "
        /// <summary>
        /// 判定処理_前半部分
        /// </summary>
        private void InoculationBefore()
        {
            m_cLogExecute.outputLog("Analize Start !");

            // 判別中等の表示が行われていない場合のみ実行
            // 判別中である画面を表示
            form_result_picture = new FormResultPicture(null);
            form_result_picture.Show();
            try
            {
                // 排他的処理
                if (m_bInoculationEnable)
                {
                    m_bInoculationEnable = false;
                    m_bJudgementFinish = false;

                    // 解析画像ファイル、結果csvファイルが既に存在している場合は消去する
                    if (File.Exists($@"{m_strPythonFolderPath}\result\color_radius.csv"))
                    {
                        //Console.WriteLine("result_delete");
                        // Pythonからの戻り値がないとき用にこの部分をコメントアウト  2022.08.02
                        //File.Delete($@"{m_strPythonFolderPath}\result\color_radius.csv");
                    }
                    if (File.Exists($@"{m_strPythonFolderPath}\img\img.png"))
                    {
                        //Console.WriteLine("img_delete");
                        File.Delete($@"{m_strPythonFolderPath}\img\img.png");
                    }
                    // 解析画像ファイルを保存する
                    string str_picturefile_name = m_strPythonFolderPath + "\\img\\img.png";
                    m_cCameraControlBase.save_image(str_picturefile_name);
                    //m_cLogExecute.outputLog("Analize folder name is " + m_strPythonFolderPath);
                    m_cLogExecute.outputLog("Save image file. (" + str_picturefile_name + ")");
                    if (File.Exists($@"{m_strPythonFolderPath}\img\img.png"))
                    {
                        //GC.Collect();
                        m_cLogExecute.outputLog("Send to python for get result.");
                        m_cSocketCommunicationForPython.SendCommand(1);

                        // Pythonからの戻りがない原因は、Startコマンド文字列に不用意な「NG」が前後に付加されてしまっていたことであったため、
                        // 下記のスレッド処理は不要とし、NG送信処理をコメントアウトした。 2022.08.02
                        //m_ThreadWatchResultDisp = new Thread(new ThreadStart(this.thread_task));
                        //m_ThreadWatchResultDisp.IsBackground = true;
                        //m_ThreadWatchResultDisp.Start();
                    }
                    else
                    {
                        m_cLogExecute.outputLog("Analize image file is not exist. Return processing.");
                        m_bInoculationEnable = true;
                        form_result_picture.Close();
                        return;
                    }
                }
                else
                {
                    // フラグが立っていなければ何もせずに終了させる
                }
            }
            catch (System.Exception ex)
            {
                m_cLogExecute.outputLog("InoculationBefore function is catch error. " + ex.Message);
                // 例外エラー
                m_bInoculationEnable = true;
                form_result_picture.Close();
                return;
            }
            finally
            {
                m_bInoculationEnable = true;
            }
        }

        #endregion

        #region "   判定処理-後半                                   "
        /// <summary>
        /// 判定処理_後半
        /// </summary>
        private void InoculationAfter()
        {
            m_cLogExecute.outputLog("  Output process start !");

            if (form_result_picture.m_iResultDialogCondition < 0)
            {
                // 判別中(区分 = -1)...が表示中なら実行
                try
                {
                    if (File.Exists($@"{m_strPythonFolderPath}\result\color_radius.csv"))
                    {
                        m_cLogExecute.outputLog("  Read result file color_radius.csv ...");
                        try
                        {
                            // 判定結果詳細内容を表示する
                            UpdataAnalyzeResultAndPicture();
                            Control c_find_control = FindControl(this, $"lblResult_{m_csParameter.ConditionColor}_{m_csParameter.ConditionSize}");

                            // 判定中
                            form_result_picture.Close();

                            if (c_find_control != null)
                            {
                                // 勝利条件を満たしているかを渡す
                                form_result_picture = new FormResultPicture((Int16.Parse(c_find_control.Text) >= m_csParameter.ConditionNumber));
                                form_result_picture.ShowDialog();
                            }
                        }
                        catch (Exception ex2)
                        {
                            m_cLogExecute.outputLog("InoculationAfter function is catch error2. " + ex2.Message);
                            // 例外エラー
                            m_bInoculationEnable = true;
                            form_result_picture.Close();
                            return;
                        }
                    }
                    else
                    {
                        m_cLogExecute.outputLog("  Result csv file is not exist. Return processing.");
                        m_bInoculationEnable = true;
                        form_result_picture.Close();
                        return;
                    }
                    GC.Collect();
                    form_result_picture.Close();
                    // 新規検査可能にする
                    m_bInoculationEnable = true;
                }
                catch (System.Exception ex)
                {
                    m_cLogExecute.outputLog("InoculationAfter function is catch error. " + ex.Message);
                    // 例外エラー
                    m_bInoculationEnable = true;
                    form_result_picture.Close();
                    return;
                }
            }
            else
            {
                m_cLogExecute.outputLog("  No judge.");
            }
        }

        #endregion

        #region "   判定結果詳細内容を表示する                      "
        /// <summary>
        /// 判定結果詳細内容を表示する
        /// </summary>
        public void UpdataAnalyzeResultAndPicture()
        {
            string str_csv_line;
            string[] str_csv_values;
            List<List<int>> int_csv_lists = new List<List<int>>(); // 解析結果を収納するリスト
            int i = 0;
            // 解析結果ファイルの内容を読み込む
            //Console.WriteLine("result_use");
            StreamReader sr_result = new StreamReader($@"{m_strPythonFolderPath}\result\color_radius.csv");
            {
                // 末尾まで繰り返す
                while (!sr_result.EndOfStream)
                {
                    int[] int_csv_values;
                    // CSVファイルの一行を読み込む
                    str_csv_line = sr_result.ReadLine();
                    // 読み込んだ一行をカンマ毎に分けて配列に格納する
                    str_csv_values = str_csv_line.Split(',');
                    int_csv_values = Array.ConvertAll(str_csv_values, int.Parse);
                    // 配列からリストに格納する
                    int_csv_lists.Add(new List<int>());
                    int_csv_lists[i].AddRange(int_csv_values);
                    i++;
                }
            }
            sr_result.Dispose();

            // 色ごとのリストを作成し、全体のリストからデータを代入する
            List<int> int_red = int_csv_lists[0];
            List<int> int_yellow = int_csv_lists[1];
            List<int> int_green = int_csv_lists[3];
            List<int> int_white = int_csv_lists[5];
            // 色ごとのリストから各半径(5mm、8mm、10mm)に近い半径の球それぞれの個数を配列に代入する
            // 赤
            int[] int_red_5mm = new int[5];
            int_red.CopyTo(3, int_red_5mm, 0, 4);
            int[] int_red_8mm = new int[3];
            int_red.CopyTo(7, int_red_8mm, 0, 2);
            int[] int_red_10mm = new int[3];
            int_red.CopyTo(9, int_red_10mm, 0, 3);
            // 黄色
            int[] int_yellow_5mm = new int[5];
            int_yellow.CopyTo(3, int_yellow_5mm, 0, 4);
            int[] int_yellow_8mm = new int[3];
            int_yellow.CopyTo(7, int_yellow_8mm, 0, 2);
            int[] int_yellow_10mm = new int[3];
            int_yellow.CopyTo(9, int_yellow_10mm, 0, 3);
            // 緑
            int[] int_green_5mm = new int[5];
            int_green.CopyTo(3, int_green_5mm, 0, 4);
            int[] int_green_8mm = new int[3];
            int_green.CopyTo(7, int_green_8mm, 0, 2);
            int[] int_green_10mm = new int[3];
            int_green.CopyTo(9, int_green_10mm, 0, 3);
            // 白
            int[] int_white_5mm = new int[5];
            int_white.CopyTo(3, int_white_5mm, 0, 4);
            int[] int_white_8mm = new int[3];
            int_white.CopyTo(7, int_white_8mm, 0, 2);
            int[] int_white_10mm = new int[3];
            int_white.CopyTo(9, int_white_10mm, 0, 3);

            DataTable table = new DataTable("Table");   // グリッドビューに表示するテーブル

            Invoke((Action)(() =>
            {
                lblResult_0_0.Text = int_red_5mm.Sum().ToString();
                lblResult_0_1.Text = int_red_8mm.Sum().ToString();
                lblResult_0_2.Text = int_red_10mm.Sum().ToString();
                lblResult_1_0.Text = int_yellow_5mm.Sum().ToString();
                lblResult_1_1.Text = int_yellow_8mm.Sum().ToString();
                lblResult_1_2.Text = int_yellow_10mm.Sum().ToString();
                lblResult_2_0.Text = int_green_5mm.Sum().ToString();
                lblResult_2_1.Text = int_green_8mm.Sum().ToString();
                lblResult_2_2.Text = int_green_10mm.Sum().ToString();
                lblResult_3_0.Text = int_white_5mm.Sum().ToString();
                lblResult_3_1.Text = int_white_8mm.Sum().ToString();
                lblResult_3_2.Text = int_white_10mm.Sum().ToString();

                //pnlResult.Image = CreateImage($@"{m_strPythonFolderPath}\img\img.png");
                string strParam = $@"{m_strPythonFolderPath}\img\img.png";
                set_picture_box(pnlResult, strParam);
            }));

            m_bJudgementFinish = true;
        }

        private void set_picture_box(PictureBox nPic, string nStr)
        {
            try
            {
                if (nPic.IsDisposed)
                {
                    return;
                }
                if (nPic.InvokeRequired)
                {
                    nPic.Invoke((MethodInvoker)delegate { set_picture_box(nPic, nStr); });
                }
                //nPic.Load(nStr);
                nPic.Image = CreateImage(nStr);
            }
            catch (System.Exception ex)
            {
                m_cLogExecute.outputLog("set_picture_box function is catch error. " + ex.Message);
            }
        }

        private static Image CreateImage(string n_stFileName)
        {
            FileStream st_fs = new FileStream(n_stFileName, FileMode.Open, FileAccess.Read);
            Image img_dst_pic = Image.FromStream(st_fs);
            st_fs.Close();
            return img_dst_pic;
        }

        #endregion

        #region "   コントロール検索                                "
        private Control FindControl(Control hParent, string stName)
        {
            // hParent 内のすべてのコントロールを列挙する
            foreach (Control cControl in hParent.Controls)
            {
                // 列挙したコントロールにコントロールが含まれている場合は再帰呼び出しする
                if (cControl.HasChildren)
                {
                    Control cFindControl = FindControl(cControl, stName);

                    // 再帰呼び出し先でコントロールが見つかった場合はそのまま返す
                    if (cFindControl != null)
                    {
                        return cFindControl;
                    }
                }

                // コントロール名が合致した場合はそのコントロールのインスタンスを返す
                if (cControl.Name == stName)
                {
                    return cControl;
                }
            }

            return null;
        }

        #endregion

        #region "   判定時にPythonからの戻りがないとき用"
        public void thread_task()
        {
            while (true)
            {
                if (form_result_picture.m_iResultDialogCondition == 0)
                {
                    break;
                }
            }

            if (!m_bJudgementFinish)
            {
                RetrySnedPython();
                RetryResultOutput();
            }
        }

        #region "   Retry send処理"
        public void RetrySnedPython()
        {
            m_cLogExecute.outputLog(" Analize reStart !");

            // 判別中等の表示が行われていない場合のみ実行
            // 判別中である画面を表示
            form_result_picture = new FormResultPicture(null);
            form_result_picture.Show();
            try
            {
                m_bInoculationEnable = false;
                m_bJudgementFinish = false;

                m_cLogExecute.outputLog(" Retry Send to python for get result.");
                m_cSocketCommunicationForPython.SendCommand(1);
            }
            catch (System.Exception ex)
            {
                m_cLogExecute.outputLog("RetrySnedPython function is catch error. " + ex.Message);
                // 例外エラー
                m_bInoculationEnable = true;
                form_result_picture.Close();
                return;
            }
            finally
            {
                m_bInoculationEnable = true;
            }
        }
        #endregion

        #region "   Retry output処理"
        public void RetryResultOutput()
        {
            try
            {
                if (File.Exists($@"{m_strPythonFolderPath}\result\color_radius.csv"))
                {
                    m_cLogExecute.outputLog("   Retry Read result file color_radius.csv ...");
                    try
                    {
                        // 判定結果詳細内容を表示する
                        UpdataAnalyzeResultAndPicture();
                        Control c_find_control = FindControl(this, $"lblResult_{m_csParameter.ConditionColor}_{m_csParameter.ConditionSize}");

                        if (c_find_control != null)
                        {
                            // 勝利条件を満たしているかを渡す
                            form_result_picture = new FormResultPicture((Int16.Parse(c_find_control.Text) >= m_csParameter.ConditionNumber));
                            form_result_picture.ShowDialog();
                        }
                    }
                    catch (Exception ex2)
                    {
                        m_cLogExecute.outputLog("CheckResultOutput function is catch error2. " + ex2.Message);
                        // 例外エラー
                        m_bInoculationEnable = true;
                        return;
                    }
                }
                else
                {
                    m_cLogExecute.outputLog("   Retry Result csv file is not exist. Return processing.");
                    m_bInoculationEnable = true;
                    return;
                }
                // 新規検査可能にする
                m_bInoculationEnable = true;
            }
            catch (System.Exception ex)
            {
                m_cLogExecute.outputLog("CheckResultOutput function is catch error. " + ex.Message);
                // 例外エラー
                m_bInoculationEnable = true;
            }
            finally
            {
                m_bJudgementFinish = true;
            }
        }
        #endregion

        #endregion

        #endregion

        #region "   判定条件設定操作                                "
        private void label1_DoubleClick(object sender, EventArgs e)
        {

            // 測定可能=測定を行っていない場合のみ可能
            if (m_bInoculationEnable)
            {
                // 測定不可にする
                m_bInoculationEnable = false;

                // 設定フォームを立ち上げる
                FormConditionSetting formConditionSetting = new FormConditionSetting();
                formConditionSetting.ShowDialog();
                formConditionSetting.Dispose();

                CParameterIO.SaveParameter(CDefine.PRMFile, m_csParameter);

                label1.Text = $"{CDefine.CCondition.Color[m_csParameter.ConditionColor]}色で" +
                    $"{CDefine.CCondition.Size[m_csParameter.ConditionSize]}の球を{m_csParameter.ConditionNumber}個撮ろう！";
                // 測定可能にする
                m_bInoculationEnable = true;

            }
        }

        #endregion

        #region "   Camera通信処理関連                              "

        #region "   カメラオープン                                  "
        /// <summary>
        /// カメラオープン処理
        /// </summary>
        /// <returns>0:正常終了 -1:オープン失敗 </returns>
        private int IniCamera()
        {
            try
            {
                // m_cCameraControlBaseをインスタンス
                if (null == m_cCameraControlBase)
                {
                    m_cCameraControlBase = new CameraControl.CMatrox();
                    // ログクラスのインスタンス
                    m_cCameraControlBase.setLogErrorInstance(LogKind.Error);
                    m_cCameraControlBase.setLogExecuteInstance(LogKind.Execute);
                    m_cCameraControlBase.setLogDeviceInstance(LogKind.Camera);
                    m_cCameraControlBase.set_folder_name(System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\log\\Image");
                }
                // 画像ファイル削除
                m_cCameraControlBase.exec_file_remove(null, 1);
                // open (渡す事が出来るハンドルに制限有り。本アプリではパネルのハンドルを渡す)
                if (m_cCameraControlBase.open(pnlCaptureedPicture.Handle, false) == false)
                {
                    // エラー処理
                    return -1;
                }
            }
            catch (System.Exception ex)
            {
                // エラー処理
                m_cLogExecute.outputLog("IniCamera function is catch error. " + ex.Message);
                return -99;
            }

            return 0;
        }
        #endregion

        #region "   取得画像を保存                                  "
        /// <summary>
        /// 取得画像保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetPicture()
        {
            try
            {
                if (null != m_cCameraControlBase)
                {
                    string str_filename = m_cCameraControlBase.get_file_name();

                    if (true == System.IO.File.Exists(str_filename))
                    {
                        m_cCameraControlBase.save_image(str_filename);
                    }
                }
            }
            catch (System.Exception ex)
            {
                m_cLogExecute.outputLog("GetPicture function is catch error. " + ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region "   画像差分処理の有無を変更                        "

        /// <summary>
        /// 画像差分処理の有無を変更
        /// </summary>
        public void ChangeDiffImageEventEnable()
        {
            m_cCameraControlBase.cImageMatrox.ChangeDiffImageEventEnable();
        }

        #endregion

        #endregion

        #region "   照明通信処理関連                                "

        private int ChangeLightState(bool nbLightState)
        {
            int i_ret;
            if (nbLightState)
            {
                i_ret = m_cBaseCCSLight.openLight(m_csParameter.LightIPAdress, m_csParameter.LightPortNumber);
                i_ret = m_cBaseCCSLight.turnLight(true, CBaseCCSLight.Channel.ch1);
                i_ret = m_cBaseCCSLight.setDimmerValue(m_csParameter.LightValue, CBaseCCSLight.Channel.ch1);
            }
            else
            {
                i_ret = m_cBaseCCSLight.turnLight(false, CBaseCCSLight.Channel.ch1);
                i_ret = m_cBaseCCSLight.setDimmerValue(0, CBaseCCSLight.Channel.ch1);
                i_ret = m_cBaseCCSLight.closeLight();
            }


            return 0;
        }

        #endregion

        #region "   Socket通信処理関連                              "

        private void CommandReceiveAction(int niCommand)
        {
            switch (niCommand)
            {
                case 1:
                    // スタート
                    // ライトを点ける
                    m_cLogExecute.outputLog("Receive socket command. strat....");
                    ChangeLightState(true);
                    break;
                case 2:
                    // 測定開始
                    m_cLogExecute.outputLog("Receive socket command. measurement....");
                    InoculationBefore();
                    break;
                case 0:
                    // 終了
                    // ライトを消す
                    m_cLogExecute.outputLog("Receive socket command. light off....");
                    ChangeLightState(false);
                    break;
            }
        }

        private void ClosedServer()
        {
            m_cLogExecute.outputLog("Receive socket command. close server....");
        }

        private void CommandReceiveActionByPython(int niCommand)
        {
            switch (niCommand)
            {
                case 3:
                    // 測定終了
                    m_cLogExecute.outputLog("Receive socket command. result from python....");
                    InoculationAfter();
                    break;
            }
        }

        private void ClosedPython()
        {
            m_cLogExecute.outputLog("Receive socket command. close python....");
        }

        #endregion


    }
}
