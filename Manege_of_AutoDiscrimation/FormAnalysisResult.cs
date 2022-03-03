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
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace Manege_of_AutoDiscrimation
{
    public partial class FormAnalysisResult : Form
    {
        // 親フォームのインスタンス
        FormAutoDiscrimation m_formAutoDiscrimation;
        // pythonフォルダのパス
        string m_strPythonFolderPath;
        public bool m_bMode = true;
        bool m_bUpdata = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="n_strPythonFolderPath">pythonフォルダのパス</param>
        /// <param name="n_formAutoDiscrimation">親フォームのインスタンス</param>
        public FormAnalysisResult(string n_strPythonFolderPath, FormAutoDiscrimation n_formAutoDiscrimation, bool n_bUpdata)
        {
            InitializeComponent();

            // 各親フォームから渡されたパラメータを代入
            m_strPythonFolderPath = n_strPythonFolderPath;
            m_formAutoDiscrimation = n_formAutoDiscrimation;
            // イベントハンドラの追加
            m_formAutoDiscrimation.evUpdataAnalyzePicture += UpdataAnalyzePictureHandler;
            // 初回作成時はアップデート不要なために作成したフラグ
            m_bUpdata = n_bUpdata;
        }

        /// <summary>
        /// フォームロード処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAnalysisResult_Load(object sender, EventArgs e)
        {
            // 初回以外でフォーム作成時に表示内容をアップデート
            if (m_bUpdata)
            {
                // フォームに解析結果と画像を表示
                ShowAnalyzeResultAndPicture();
            }
        }
        /// <summary>
        /// 画像に差分がある場合のイベントハンドラ
        /// </summary>
        private void UpdataAnalyzePictureHandler()
        {
            // フォームに解析結果と画像を表示
            ShowAnalyzeResultAndPicture();
        }

        /// <summary>
        /// 解析結果と元となった画像をフォームに表示
        /// </summary>
        public void ShowAnalyzeResultAndPicture()
        {
            string str_csv_line;
            string[] str_csv_values;
            List<List<int>> int_csv_lists = new List<List<int>>(); // 解析結果を収納するリスト
            //StreamReader sr_img = new StreamReader($@"{m_strPythonFolderPath}\img\img.png");
            //Image img_dst_pic = Image.FromFile($@"{m_strPythonFolderPath}\img\img.png");
            //File.Copy($@"{m_strPythonFolderPath}\img\img.png", $@"{m_strPythonFolderPath}\img\img_temp.png", true);
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
                chart1.Series["red"].Points.Clear();
                chart1.Series["yellow"].Points.Clear();
                chart1.Series["green"].Points.Clear();
                chart1.Series["white"].Points.Clear();
                // 色-半径毎の個数を合計し、グリッドビューの行に代入する
                dataGridView1.Rows.Add("赤", int_red_5mm.Sum(), int_red_8mm.Sum(), int_red_10mm.Sum(), int_red_5mm.Sum() + int_red_8mm.Sum() + int_red_10mm.Sum());
                dataGridView1.Rows.Add("黄", int_yellow_5mm.Sum(), int_yellow_8mm.Sum(), int_yellow_10mm.Sum(), int_yellow_5mm.Sum() + int_yellow_8mm.Sum() + int_yellow_10mm.Sum());
                dataGridView1.Rows.Add("緑", int_green_5mm.Sum(), int_green_8mm.Sum(), int_green_10mm.Sum(), int_green_5mm.Sum() + int_green_8mm.Sum() + int_green_10mm.Sum());
                dataGridView1.Rows.Add("白", int_white_5mm.Sum(), int_white_8mm.Sum(), int_white_10mm.Sum(), int_white_5mm.Sum() + int_white_8mm.Sum() + int_white_10mm.Sum());
                dataGridView1.Rows.Add("合計", int_red_5mm.Sum() + int_yellow_5mm.Sum()+ int_green_5mm.Sum()+ int_white_5mm.Sum()
                    , int_red_8mm.Sum() + int_yellow_8mm.Sum()+ int_green_8mm.Sum()+ int_white_8mm.Sum()
                    , int_red_10mm.Sum() + int_yellow_10mm.Sum() + int_green_10mm.Sum() + int_white_10mm.Sum()
                    , int_red_5mm.Sum() + int_yellow_5mm.Sum() + int_green_5mm.Sum() + int_white_5mm.Sum()
                    + int_red_8mm.Sum() + int_yellow_8mm.Sum() + int_green_8mm.Sum() + int_white_8mm.Sum()
                    + int_red_10mm.Sum() + int_yellow_10mm.Sum() + int_green_10mm.Sum() + int_white_10mm.Sum());
                // チャートに(半径, 個数)のデータをプロットする
                // 同色ごとに同じシリーズのデータとする
                // 赤シリーズプロット追加
                chart1.Series["red"].Points.AddXY(5, int_red_5mm.Sum());
                chart1.Series["red"].Points.AddXY(8, int_red_8mm.Sum());
                chart1.Series["red"].Points.AddXY(10, int_red_10mm.Sum());
                // 黄色シリーズプロット追加
                chart1.Series["yellow"].Points.AddXY(5, int_yellow_5mm.Sum());
                chart1.Series["yellow"].Points.AddXY(8, int_yellow_8mm.Sum());
                chart1.Series["yellow"].Points.AddXY(10, int_yellow_10mm.Sum());
                // 緑シリーズプロット追加
                chart1.Series["green"].Points.AddXY(5, int_green_5mm.Sum());
                chart1.Series["green"].Points.AddXY(8, int_green_8mm.Sum());
                chart1.Series["green"].Points.AddXY(10, int_green_10mm.Sum());
                // 白シリーズプロット追加
                chart1.Series["white"].Points.AddXY(5, int_white_5mm.Sum());
                chart1.Series["white"].Points.AddXY(8, int_white_8mm.Sum());
                chart1.Series["white"].Points.AddXY(10, int_white_10mm.Sum());
                // ピクチャーボックスに解析に使用した画像を挿入
                //pictureBox1.ImageLocation = $@"{m_strPythonFolderPath}\img\img_temp.png";
                pictureBox1.Image = CreateImage($@"{m_strPythonFolderPath}\img\img.png");
                //img_dst_pic.Dispose();
                //Console.WriteLine("Invoke_End");
            }));
        }

        private static Image CreateImage(string n_stFileName)
        {
            FileStream st_fs = new FileStream(n_stFileName, FileMode.Open, FileAccess.Read);
            Image img_dst_pic = Image.FromStream(st_fs);
            st_fs.Close();
            return img_dst_pic;
        }

        private void btn_mode_Click(object sender, EventArgs e)
        {
            m_bMode = !m_bMode;
            if (m_bMode)
            {
                btn_mode.Text = "Auto";
                btn_mode.BackColor = Color.Orange;
                m_formAutoDiscrimation.ChangeDiffImageEventEnable();
            }
            else
            {
                btn_mode.Text = "Manual";
                btn_mode.BackColor = Color.Lime;
                m_formAutoDiscrimation.ChangeDiffImageEventEnable();
            }
        }
    }
}
