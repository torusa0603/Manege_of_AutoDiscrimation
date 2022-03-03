using System;
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
        private CameraControl.CCameraControlBase m_cCameraControlBase = null;   //カメラ制御クラス
        FormAnalysisResult m_formAnalyzeResult = null;  //解析結果表示フォーム
        public Action evUpdataAnalyzePicture;   //マニュアルモード時解析イベントハンドラ
        string str_python_folder_path = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\PythonFile";   //pythonディレクトリ
        TimerCallback m_timerDelegate;
        System.Threading.Timer m_timer;
        int m_iTimerSleepTime;
        static bool m_bInoculationEnable; // 判定可能かどうかを示す
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

        #region "   フォームロード時処理                                        "
        /// <summary>
        /// フォームロード時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAutoDiscrimation_Load(object sender, EventArgs e)
        {
            //CreateForm2();

            try
            {
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
                // 画像処理コンボボックスの設定
                comboBoxClass.SelectedItem = m_cParaFormMain.ComboBoxClass_SelectedItem;
                // コンボボックスにカメラクラスのインスタンスを示すアイテムを追加
                if (m_cParaFormMain.DebugCameraFlag != true)
                {
                    comboBoxClass.Items.Add("Matrox");
                    comboBoxClass.SelectedItem = "Matrox";
                }
                else
                {
                    comboBoxClass.Items.Add("Virtual");
                    comboBoxClass.SelectedItem = "Virtual";
                }
                // カメラオープンイベント
                btnOpen_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {

            }
        }
        #endregion

        #region "   フォーム表示完了後処理                                 "
        /// <summary>
        /// ダイアログ表示処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAutoDiscrimation_Shown(object sender, EventArgs e)
        {
            try
            {
                FormAutoDiscrimation.m_bInoculationEnable = true;
                m_iTimerSleepTime = 1000;
                m_timerDelegate = new TimerCallback(Inoculation);
                m_timer = new System.Threading.Timer(m_timerDelegate, null, -1, m_iTimerSleepTime);
                m_cCameraControlBase.cImageMatrox.m_evDiffEnable_True += ChangeDiffEnableToTrue;
                m_cCameraControlBase.cImageMatrox.m_evDiffEnable_False += ChangeDiffEnableToFlase;
                // 画像同士の差分を取るモードに変更
                m_cCameraControlBase.cImageMatrox.sifanalyzeDiffImage(20);
                // 解析結果フォームを作成
                m_formAnalyzeResult = new FormAnalysisResult(str_python_folder_path, this, false);
                m_formAnalyzeResult.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            finally
            {
                
            }
        }
        #endregion


        private void ChangeDiffEnableToTrue()
        {
            //Console.WriteLine("timer_start");
            m_timer.Change(0, m_iTimerSleepTime);
        }

        private void ChangeDiffEnableToFlase()
        {
            //Console.WriteLine("timer_stop");
            m_timer.Change(-1, m_iTimerSleepTime);
        }

        /// <summary>
        /// 判定
        /// </summary>
        private void Inoculation(object o)
        {
            // 排他的処理
            if (FormAutoDiscrimation.m_bInoculationEnable)
            {
                FormAutoDiscrimation.m_bInoculationEnable = false;

                // 解析画像ファイル、結果csvファイルが既に存在している場合は消去する
                if (File.Exists($@"{str_python_folder_path}\result\color_radius.csv"))
                {
                    //Console.WriteLine("result_delete");
                    File.Delete($@"{str_python_folder_path}\result\color_radius.csv");
                }
                if (File.Exists($@"{str_python_folder_path}\img\img.png"))
                {
                    //Console.WriteLine("img_delete");
                    File.Delete($@"{str_python_folder_path}\img\img.png");
                }
                // 解析画像ファイルを保存する
                string str_picturefile_name = str_python_folder_path + "\\img\\img.png";
                m_cCameraControlBase.save_image(str_picturefile_name);
                if (File.Exists($@"{str_python_folder_path}\img\img.png"))
                {
                    // 実行させたいpythonファイルのパス
                    string myPythonApp = "PythonFile\\AutoDiscriminate.py";
                    // プロセスを起動
                    Process myProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo("python.exe")
                        {
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = false,
                            Arguments = $"{myPythonApp} {str_python_folder_path}"
                        }
                    };
                    myProcess.Start();
                    // デバック用に残す
                    //StreamReader myStreamReader = myProcess.StandardOutput;
                    //string myString = myStreamReader.ReadLine();
                    // 処理の終了まで待つ
                    myProcess.WaitForExit();
                    // プロセスを閉じて終了
                    myProcess.Close();
                }
                else
                {
                    FormAutoDiscrimation.m_bInoculationEnable = true;
                    return;
                }
                if (File.Exists($@"{str_python_folder_path}\result\color_radius.csv"))
                {
                    // 解析結果フォームが閉じていれば作成
                    if ((m_formAnalyzeResult == null) || m_formAnalyzeResult.IsDisposed)
                    {
                        m_formAnalyzeResult = new FormAnalysisResult(str_python_folder_path, this, true);
                        m_formAnalyzeResult.Show();
                    }
                    else
                    {
                        // 解析結果フォームを更新
                        m_formAnalyzeResult.ShowAnalyzeResultAndPicture();
                        //evUpdataAnalyzePicture();
                    }
                }
                else
                {
                    FormAutoDiscrimation.m_bInoculationEnable = true;
                    return;
                }
                GC.Collect();
                //Console.WriteLine("timermethod_end");
                FormAutoDiscrimation.m_bInoculationEnable = true;
            }
            else
            {
                // フラグが立っていなければ何もせずに終了させる
            }
        }

        /// <summary>
        /// 画像差分処理の有無を変更
        /// </summary>
        public void ChangeDiffImageEventEnable()
        {
            m_cCameraControlBase.cImageMatrox.ChangeDiffImageEventEnable();
        }


        #region "   取得画像を保存                            "
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
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region "   画像処理/Openボタン                             "
        /// <summary>
        /// カメラオープン処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                // m_cCameraControlBaseをインスタンス
                if (null == m_cCameraControlBase)
                {
                    if ("Virtual" == comboBoxClass.SelectedItem.ToString())
                    {
                        m_cCameraControlBase = new CameraControl.CCameraControlVirtual();
                    }
                    else if (true == comboBoxClass.SelectedItem.ToString().Contains("Matrox"))
                    {
                        m_cCameraControlBase = new CameraControl.CMatrox();
                    }
                    else
                    {
                        m_cCameraControlBase = new CameraControl.CCameraControlVirtual();
                    }
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
                    MessageBox.Show("カメラオープンに失敗しました");
                    this.Close();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region "   Logクラス初期化                                   "
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

        /// <summary>
        /// パネルクリック時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pnlCaptureedPicture_Click(object sender, EventArgs e)
        {
            if (!(m_formAnalyzeResult == null))
            {
                // マニュアルモード時、解析を行う
                if (!m_formAnalyzeResult.m_bMode)
                {
                    object ob_o = null;
                    Inoculation(ob_o);
                }
            }
        }

        private void FormAutoDiscrimation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_timer != null)
            {
                // タイマーを破棄
                m_timer.Dispose();
            }
        }
    }
}
