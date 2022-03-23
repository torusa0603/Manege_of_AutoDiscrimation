
namespace Manege_of_AutoDiscrimation
{
    partial class FormConditionSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.nudNumber = new System.Windows.Forms.NumericUpDown();
            this.cmbSize = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnSetting = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.nudDisplayTime = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudNumber)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDisplayTime)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbColor
            // 
            this.cmbColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColor.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.cmbColor.FormattingEnabled = true;
            this.cmbColor.Location = new System.Drawing.Point(223, 21);
            this.cmbColor.Name = "cmbColor";
            this.cmbColor.Size = new System.Drawing.Size(146, 32);
            this.cmbColor.TabIndex = 0;
            // 
            // nudNumber
            // 
            this.nudNumber.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.nudNumber.Location = new System.Drawing.Point(222, 151);
            this.nudNumber.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudNumber.Name = "nudNumber";
            this.nudNumber.ReadOnly = true;
            this.nudNumber.Size = new System.Drawing.Size(147, 31);
            this.nudNumber.TabIndex = 1;
            // 
            // cmbSize
            // 
            this.cmbSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSize.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.cmbSize.FormattingEnabled = true;
            this.cmbSize.Location = new System.Drawing.Point(222, 82);
            this.cmbSize.Name = "cmbSize";
            this.cmbSize.Size = new System.Drawing.Size(147, 32);
            this.cmbSize.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.label1.Location = new System.Drawing.Point(23, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "色";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.label2.Location = new System.Drawing.Point(23, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "サイズ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.label3.Location = new System.Drawing.Point(23, 153);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "個数";
            // 
            // btnSetting
            // 
            this.btnSetting.BackColor = System.Drawing.Color.Chartreuse;
            this.btnSetting.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.btnSetting.Location = new System.Drawing.Point(131, 283);
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(113, 50);
            this.btnSetting.TabIndex = 6;
            this.btnSetting.Text = "設定";
            this.btnSetting.UseVisualStyleBackColor = false;
            this.btnSetting.Click += new System.EventHandler(this.btnSetting_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.label4.Location = new System.Drawing.Point(23, 222);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(154, 24);
            this.label4.TabIndex = 8;
            this.label4.Text = "結果表示時間";
            // 
            // nudDisplayTime
            // 
            this.nudDisplayTime.Font = new System.Drawing.Font("MS UI Gothic", 18F);
            this.nudDisplayTime.Location = new System.Drawing.Point(222, 220);
            this.nudDisplayTime.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudDisplayTime.Name = "nudDisplayTime";
            this.nudDisplayTime.ReadOnly = true;
            this.nudDisplayTime.Size = new System.Drawing.Size(147, 31);
            this.nudDisplayTime.TabIndex = 7;
            // 
            // FormConditionSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(381, 354);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.nudDisplayTime);
            this.Controls.Add(this.btnSetting);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbSize);
            this.Controls.Add(this.nudNumber);
            this.Controls.Add(this.cmbColor);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConditionSetting";
            this.Text = "勝利条件";
            ((System.ComponentModel.ISupportInitialize)(this.nudNumber)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudDisplayTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbColor;
        private System.Windows.Forms.NumericUpDown nudNumber;
        private System.Windows.Forms.ComboBox cmbSize;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSetting;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudDisplayTime;
    }
}