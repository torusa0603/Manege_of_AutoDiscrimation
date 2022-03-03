namespace Manege_of_AutoDiscrimation
{
    partial class FormAutoDiscrimation
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxClass = new System.Windows.Forms.ComboBox();
            this.pnlCaptureedPicture = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // comboBoxClass
            // 
            this.comboBoxClass.FormattingEnabled = true;
            this.comboBoxClass.Location = new System.Drawing.Point(156, 90);
            this.comboBoxClass.Name = "comboBoxClass";
            this.comboBoxClass.Size = new System.Drawing.Size(121, 20);
            this.comboBoxClass.TabIndex = 2;
            this.comboBoxClass.Visible = false;
            // 
            // pnlCaptureedPicture
            // 
            this.pnlCaptureedPicture.Location = new System.Drawing.Point(0, 0);
            this.pnlCaptureedPicture.Name = "pnlCaptureedPicture";
            this.pnlCaptureedPicture.Size = new System.Drawing.Size(960, 540);
            this.pnlCaptureedPicture.TabIndex = 3;
            this.pnlCaptureedPicture.Click += new System.EventHandler(this.pnlCaptureedPicture_Click);
            // 
            // FormAutoDiscrimation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(960, 540);
            this.Controls.Add(this.pnlCaptureedPicture);
            this.Controls.Add(this.comboBoxClass);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormAutoDiscrimation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AutoDiscrimation";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormAutoDiscrimation_FormClosing);
            this.Load += new System.EventHandler(this.FormAutoDiscrimation_Load);
            this.Shown += new System.EventHandler(this.FormAutoDiscrimation_Shown);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxClass;
        private System.Windows.Forms.Panel pnlCaptureedPicture;
    }
}

