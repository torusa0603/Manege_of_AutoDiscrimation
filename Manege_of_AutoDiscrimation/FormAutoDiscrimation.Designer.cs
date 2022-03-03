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
            this.panel1 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column_Color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_5mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_8mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_10mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.合計 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
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
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(959, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(960, 540);
            this.panel1.TabIndex = 4;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column_Color,
            this.Column_5mm,
            this.Column_8mm,
            this.Column_10mm,
            this.合計});
            this.dataGridView1.Location = new System.Drawing.Point(977, 551);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(931, 478);
            this.dataGridView1.TabIndex = 5;
            // 
            // Column_Color
            // 
            this.Column_Color.HeaderText = "色";
            this.Column_Color.Name = "Column_Color";
            this.Column_Color.Width = 50;
            // 
            // Column_5mm
            // 
            this.Column_5mm.HeaderText = "5mm";
            this.Column_5mm.Name = "Column_5mm";
            this.Column_5mm.Width = 70;
            // 
            // Column_8mm
            // 
            this.Column_8mm.HeaderText = "8mm";
            this.Column_8mm.Name = "Column_8mm";
            this.Column_8mm.Width = 70;
            // 
            // Column_10mm
            // 
            this.Column_10mm.HeaderText = "10mm";
            this.Column_10mm.Name = "Column_10mm";
            this.Column_10mm.Width = 70;
            // 
            // 合計
            // 
            this.合計.HeaderText = "合計";
            this.合計.Name = "合計";
            this.合計.Width = 70;
            // 
            // FormAutoDiscrimation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1920, 1041);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxClass;
        private System.Windows.Forms.Panel pnlCaptureedPicture;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Color;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_5mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_8mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_10mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn 合計;
    }
}

