namespace Manege_of_AutoDiscrimation
{
    partial class FormAnalysisResult
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column_Color = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_5mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_8mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column_10mm = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.合計 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btn_mode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(960, 540);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
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
            this.dataGridView1.Location = new System.Drawing.Point(562, 592);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(333, 127);
            this.dataGridView1.TabIndex = 2;
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
            // chart1
            // 
            chartArea1.AxisX.Interval = 1D;
            chartArea1.AxisX.Maximum = 11D;
            chartArea1.AxisX.Minimum = 4D;
            chartArea1.AxisX.Title = "球半径";
            chartArea1.AxisY.Interval = 1D;
            chartArea1.AxisY.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.Maximum = 7D;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY.Title = "個数";
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(12, 592);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.Red;
            series1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            series1.Legend = "Legend1";
            series1.LegendText = "赤";
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series1.Name = "red";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.Yellow;
            series2.Legend = "Legend1";
            series2.LegendText = "黄";
            series2.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series2.Name = "yellow";
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Color = System.Drawing.Color.Lime;
            series3.Legend = "Legend1";
            series3.LegendText = "緑";
            series3.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series3.Name = "green";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Color = System.Drawing.Color.Black;
            series4.Legend = "Legend1";
            series4.LegendText = "白";
            series4.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series4.Name = "white";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Series.Add(series3);
            this.chart1.Series.Add(series4);
            this.chart1.Size = new System.Drawing.Size(487, 325);
            this.chart1.TabIndex = 3;
            this.chart1.Text = "chart1";
            // 
            // btn_mode
            // 
            this.btn_mode.BackColor = System.Drawing.Color.Orange;
            this.btn_mode.Font = new System.Drawing.Font("MS UI Gothic", 40F);
            this.btn_mode.Location = new System.Drawing.Point(562, 794);
            this.btn_mode.Name = "btn_mode";
            this.btn_mode.Size = new System.Drawing.Size(333, 110);
            this.btn_mode.TabIndex = 4;
            this.btn_mode.Text = "Auto";
            this.btn_mode.UseVisualStyleBackColor = false;
            this.btn_mode.Click += new System.EventHandler(this.btn_mode_Click);
            // 
            // FormAnalysisResult
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(961, 961);
            this.Controls.Add(this.btn_mode);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.pictureBox1);
            this.Location = new System.Drawing.Point(960, 0);
            this.Name = "FormAnalysisResult";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "AnalysisResult";
            this.Load += new System.EventHandler(this.FormAnalysisResult_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button btn_mode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_Color;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_5mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_8mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column_10mm;
        private System.Windows.Forms.DataGridViewTextBoxColumn 合計;
    }
}