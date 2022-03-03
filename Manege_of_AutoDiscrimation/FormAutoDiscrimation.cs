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
        public Action evAnalyzePicture;   //マニュアルモード時解析イベントハンドラ
        string m_strPythonFolderPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\PythonFile";   //pythonディレクトリ
        Parameter m_Parameter;
        TimerCallback m_timerDelegate;
        System.Threading.Timer m_timer;
        int m_iTimerSleepTime;
        static bool m_bInoculationEnable; // 判定可能かどうかを示す
        SocketCommunication m_cSocketCommunication; // ソケット通信用のクラス
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

                // カメラオープン
                int i_ret=IniCamera();
                if(i_ret != 0)
                {
                    MessageBox.Show("カメラオープンに失敗しました");
                    this.Close();
                }

                // ソケットクラスオープン
                m_cSocketCommunication = new SocketCommunication();
                int i_port_number=0; // 後で決める
                i_ret= m_cSocketCommunication.Init(this,"", i_port_number);
                if (i_ret != 0)
                {
                    MessageBox.Show("ソケットオープンに失敗しました");
                    this.Close();
                }
                m_cSocketCommunication.evCommandReceive += CommandReceiveAction;


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

        /// <summary>
        /// 判定
        /// </summary>
        private void Inoculation()
        {
            // 排他的処理
            if (FormAutoDiscrimation.m_bInoculationEnable)
            {
                FormAutoDiscrimation.m_bInoculationEnable = false;

                // 解析画像ファイル、結果csvファイルが既に存在している場合は消去する
                if (File.Exists($@"{m_strPythonFolderPath}\result\color_radius.csv"))
                {
                    //Console.WriteLine("result_delete");
                    File.Delete($@"{m_strPythonFolderPath}\result\color_radius.csv");
                }
                if (File.Exists($@"{m_strPythonFolderPath}\img\img.png"))
                {
                    //Console.WriteLine("img_delete");
                    File.Delete($@"{m_strPythonFolderPath}\img\img.png");
                }
                // 解析画像ファイルを保存する
                string str_picturefile_name = m_strPythonFolderPath + "\\img\\img.png";
                m_cCameraControlBase.save_image(str_picturefile_name);
                if (File.Exists($@"{m_strPythonFolderPath}\img\img.png"))
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
                            Arguments = $"{myPythonApp} {m_strPythonFolderPath}"
                        }
                    };
                    myProcess.Start();
                    // デバック用に残す
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
                if (File.Exists($@"{m_strPythonFolderPath}\result\color_radius.csv"))
                {
                    UpdataAnalyzeResultAndPicture();
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

        #region "   カメラオープン                             "
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
                return -99;
            }

            return 0;
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

        private void FormAutoDiscrimation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_timer != null)
            {
                // タイマーを破棄
                m_timer.Dispose();
            }
        }

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
            List<int> int_green = int_csv_lists[2];
            List<int> int_white = int_csv_lists[5];
            // 色ごとのリストから各半径(5mm、8mm、10mm)に近い半径の球それぞれの個数を配列に代入する
            // 赤
            int[] int_red_5mm = new int[5];
            int_red.CopyTo(0, int_red_5mm, 0, 5);
            int[] int_red_8mm = new int[3];
            int_red.CopyTo(5, int_red_8mm, 0, 3);
            int[] int_red_10mm = new int[3];
            int_red.CopyTo(8, int_red_10mm, 0, 3);
            // 黄色
            int[] int_yellow_5mm = new int[5];
            int_yellow.CopyTo(0, int_yellow_5mm, 0, 5);
            int[] int_yellow_8mm = new int[3];
            int_yellow.CopyTo(5, int_yellow_8mm, 0, 3);
            int[] int_yellow_10mm = new int[3];
            int_yellow.CopyTo(8, int_yellow_10mm, 0, 3);
            // 緑
            int[] int_green_5mm = new int[5];
            int_green.CopyTo(0, int_green_5mm, 0, 5);
            int[] int_green_8mm = new int[3];
            int_green.CopyTo(5, int_green_8mm, 0, 3);
            int[] int_green_10mm = new int[3];
            int_green.CopyTo(8, int_green_10mm, 0, 3);
            // 白
            int[] int_white_5mm = new int[5];
            int_white.CopyTo(0, int_white_5mm, 0, 5);
            int[] int_white_8mm = new int[3];
            int_white.CopyTo(5, int_white_8mm, 0, 3);
            int[] int_white_10mm = new int[3];
            int_white.CopyTo(8, int_white_10mm, 0, 3);

            DataTable table = new DataTable("Table");   // グリッドビューに表示するテーブル

            Invoke((Action)(() =>
            {
                //Console.WriteLine("Invoke_Start");
                // グリッドビューとチャートに表示されている内容をクリア
                dataGridView1.Rows.Clear();
                // 色-半径毎の個数を合計し、グリッドビューの行に代入する
                dataGridView1.Rows.Add("赤", int_red_5mm.Sum(), int_red_8mm.Sum(), int_red_10mm.Sum(), int_red_5mm.Sum() + int_red_8mm.Sum() + int_red_10mm.Sum());
                dataGridView1.Rows.Add("黄", int_yellow_5mm.Sum(), int_yellow_8mm.Sum(), int_yellow_10mm.Sum(), int_yellow_5mm.Sum() + int_yellow_8mm.Sum() + int_yellow_10mm.Sum());
                dataGridView1.Rows.Add("緑", int_green_5mm.Sum(), int_green_8mm.Sum(), int_green_10mm.Sum(), int_green_5mm.Sum() + int_green_8mm.Sum() + int_green_10mm.Sum());
                dataGridView1.Rows.Add("白", int_white_5mm.Sum(), int_white_8mm.Sum(), int_white_10mm.Sum(), int_white_5mm.Sum() + int_white_8mm.Sum() + int_white_10mm.Sum());
                dataGridView1.Rows.Add("合計", int_red_5mm.Sum() + int_yellow_5mm.Sum() + int_green_5mm.Sum() + int_white_5mm.Sum()
                    , int_red_8mm.Sum() + int_yellow_8mm.Sum() + int_green_8mm.Sum() + int_white_8mm.Sum()
                    , int_red_10mm.Sum() + int_yellow_10mm.Sum() + int_green_10mm.Sum() + int_white_10mm.Sum()
                    , int_red_5mm.Sum() + int_yellow_5mm.Sum() + int_green_5mm.Sum() + int_white_5mm.Sum()
                    + int_red_8mm.Sum() + int_yellow_8mm.Sum() + int_green_8mm.Sum() + int_white_8mm.Sum()
                    + int_red_10mm.Sum() + int_yellow_10mm.Sum() + int_green_10mm.Sum() + int_white_10mm.Sum());
                pnlResult.Image = CreateImage($@"{m_strPythonFolderPath}\img\img.png");
            }));
        }

        private static Image CreateImage(string n_stFileName)
        {
            FileStream st_fs = new FileStream(n_stFileName, FileMode.Open, FileAccess.Read);
            Image img_dst_pic = Image.FromStream(st_fs);
            st_fs.Close();
            return img_dst_pic;
        }

        private void CommandReceiveAction(int niCommand)
        {
            switch (niCommand)
            {
                case 0:
                    // スタート
                    // ライトを点ける
                    break;
                case 1:
                    // 測定開始
                    Inoculation();
                    break;
                case 2:
                    // 終了
                    // ライトを消す
                    break;
            }
        }
    }
    
}
