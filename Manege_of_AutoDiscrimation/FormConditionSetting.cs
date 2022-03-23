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
    public partial class FormConditionSetting : Form
    {
        public FormConditionSetting()
        {
            InitializeComponent();
            cmbColor.Items.AddRange(CDefine.CCondition.Color);
            cmbSize.Items.AddRange(CDefine.CCondition.Size);
            nudNumber.Maximum = CDefine.CCondition.NumberMax;
            // 現在の設定値を代入
            this.cmbColor.SelectedIndex= FormAutoDiscrimation.m_csParameter.ConditionColor;
            this.cmbSize.SelectedIndex =FormAutoDiscrimation.m_csParameter.ConditionSize ;
            this.nudNumber.Value       =FormAutoDiscrimation.m_csParameter.ConditionNumber;
            this.nudDisplayTime.Value = FormAutoDiscrimation.m_csParameter.ResultFormDisplayTime;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            // 設定値にコントロールの内容を代入
            FormAutoDiscrimation.m_csParameter.ConditionColor=this.cmbColor.SelectedIndex;
            FormAutoDiscrimation.m_csParameter.ConditionSize = this.cmbSize.SelectedIndex;
            FormAutoDiscrimation.m_csParameter.ConditionNumber = (int)this.nudNumber.Value;
            FormAutoDiscrimation.m_csParameter.ResultFormDisplayTime=(int)this.nudDisplayTime.Value;

            // フォームを閉じる
            this.Close();
        }
    }
}
