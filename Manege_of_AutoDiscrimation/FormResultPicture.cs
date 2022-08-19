using System;
using System.Drawing;
using System.Windows.Forms;

namespace Manege_of_AutoDiscrimation
{
    public partial class FormResultPicture : Form
    {
        public int m_iResultDialogCondition = 0;   // ダイアログ表示状態(-1:判定中, 0:表示なし, 1:結果表示中)
        private LogBase.CLogBase m_cLogExecute = new LogBase.CLogBase(LogKind.Execute); // 実行ログ

        /// <summary>
        /// ダイアログ表示処理
        /// </summary>
        /// <param name="nbResult">null:判定中を表示, 0:失敗表示, 1:成功表示</param>
        public FormResultPicture(bool? nbResult)
        {
            InitializeComponent();
            if(nbResult == null)
            {
                // 判定中．．．表示
                //m_cLogExecute.outputLog("FormResultPicture control ... Show analyzing dialog.");
                this.Location = new Point((960-215), (540-100-270));
                picResult.Visible = false;
                this.Size = new Size(430, 200);
                timer2.Start();

                // タイマーを設定
                timer1.Interval = FormAutoDiscrimation.m_csParameter.DiscriminationFormDisplayTime * 1000;
                // タイマースタート
                timer1.Start();

                m_iResultDialogCondition = -1;      // ダイアログ表示状態 : 判定中
            }
            else
            {
                // 全画面表示する
                //m_cLogExecute.outputLog("FormResultPicture control ... Show result picture. result = [" + nbResult + "].");
                this.Size = new Size(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 );
                picResult.Size = new Size(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2);
                this.Location = new Point(0, 540);
                if ((bool)nbResult)
                {
                    // 成功画像表示
                    picResult.Image = Properties.Resources.Good;
                }
                else
                {
                    // 失敗画像表示
                    picResult.Image = Properties.Resources.NextChallenge;
                }

                // タイマーを設定
                timer1.Interval = FormAutoDiscrimation.m_csParameter.ResultFormDisplayTime * 1000;
                // タイマースタート
                timer1.Start();

                m_iResultDialogCondition = 1;       // ダイアログ表示状態 : 結果表示中
            }
        }

        /// <summary>
        /// ダイアログ終了処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormResultPicture_FormClosed(object sender, FormClosedEventArgs e)
        {
            //m_cLogExecute.outputLog("FormResultPicture control ... Close dialog.");
            timer1.Stop();
            timer2.Stop();
            m_iResultDialogCondition = 0;           // ダイアログ表示状態 : 表示なし
        }

        /// <summary>
        /// 一定時間経過後に閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            // 一定時間経過後に閉じる
            this.Close();
        }

        /// <summary>
        /// 判別中... 描画切り替え
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Tick(object sender, EventArgs e)
        {
            Invoke((Action)(() =>
            {
                if (lblCheck.Text == "判別中")
                {
                    lblCheck.Text = "判別中.";
                }
                else if (lblCheck.Text == "判別中.")
                {
                    lblCheck.Text = "判別中..";
                }
                else if (lblCheck.Text == "判別中..")
                {
                    lblCheck.Text = "判別中...";
                }
                else if (lblCheck.Text == "判別中...")
                {
                    lblCheck.Text = "判別中";
                }

                // 描画させる
                lblCheck.Update();
            }));
        }

    }
}
