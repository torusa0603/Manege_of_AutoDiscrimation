using System;
using System.Drawing;
using System.Windows.Forms;

namespace Manege_of_AutoDiscrimation
{
    public partial class FormResultPicture : Form
    {
        public bool m_bIsVisiable = false; // 表示状態
        public FormResultPicture(bool? nbResult)
        {
            InitializeComponent();
            if(nbResult == null)
            {
                this.Location = new Point((960-215), (540-100-270));
                picResult.Visible = false;
                this.Size = new Size(430, 200);
                timer2.Start();

                // タイマーを設定
                timer1.Interval = FormAutoDiscrimation.m_csParameter.DiscriminationFormDisplayTime * 1000;
                // タイマースタート
                timer1.Start();
            }
            else
            {
                // 全画面表示する
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
            }
            m_bIsVisiable = true;
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            // 一定時間経過後に閉じる
            this.Close();
        }

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

        private void FormResultPicture_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            m_bIsVisiable = false;
        }
    }
}
