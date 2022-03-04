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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.pnlCaptureedPicture = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column_Color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_5mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_8mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_10mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.合計 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlResult = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlResult)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlCaptureedPicture
            // 
            this.pnlCaptureedPicture.Location = new System.Drawing.Point(0, 0);
            this.pnlCaptureedPicture.Name = "pnlCaptureedPicture";
            this.pnlCaptureedPicture.Size = new System.Drawing.Size(960, 540);
            this.pnlCaptureedPicture.TabIndex = 3;
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
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("MS UI Gothic", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.Location = new System.Drawing.Point(977, 551);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("MS UI Gothic", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
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
            // pnlResult
            // 
            this.pnlResult.Location = new System.Drawing.Point(960, 0);
            this.pnlResult.Name = "pnlResult";
            this.pnlResult.Size = new System.Drawing.Size(960, 540);
            this.pnlResult.TabIndex = 6;
            this.pnlResult.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(99, 720);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(761, 124);
            this.textBox1.TabIndex = 7;
            this.textBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox1_MouseUp);
            // 
            // FormAutoDiscrimation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1920, 1041);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.pnlResult);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.pnlCaptureedPicture);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormAutoDiscrimation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AutoDiscrimation";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormAutoDiscrimation_FormClosing);
            this.Load += new System.EventHandler(this.FormAutoDiscrimation_Load);
            this.Shown += new System.EventHandler(this.FormAutoDiscrimation_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pnlResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel pnlCaptureedPicture;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Color;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_5mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_8mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_10mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn 合計;
        private System.Windows.Forms.PictureBox pnlResult;
        private System.Windows.Forms.TextBox textBox1;
    }
}

