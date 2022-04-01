using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Manege_of_AutoDiscrimation
{
    public partial class FormResultPicture : Form
    {
        public FormResultPicture(bool? nbResult)
        {
            InitializeComponent();
            if(nbResult == null)
            {
                picResult.Visible = false;
                this.Size = new Size(430, 200);
                timer2.Start();
            }
            else
            {
                // 全画面表示する
                this.WindowState = FormWindowState.Maximized;
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
                if (lblCheck.Text == "検査中")
                {
                    lblCheck.Text = "検査中.";
                }
                else if (lblCheck.Text == "検査中.")
                {
                    lblCheck.Text = "検査中..";
                }
                else if (lblCheck.Text == "検査中..")
                {
                    lblCheck.Text = "検査中...";
                }
                else if (lblCheck.Text == "検査中...")
                {
                    lblCheck.Text = "検査中";
                }

                // 描画させる
                lblCheck.Update();
            }));
        }

        private void FormResultPicture_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
        }
    }
}
