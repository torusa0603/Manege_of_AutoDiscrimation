using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manege_of_AutoDiscrimation
{
    public class Parameter
    {
        public int PortNumber { get; set; } = 0;
        public int ImageDisplayTime { get; set; } = 5;
        public int ConditionSize { get; set; } = 0;
        public int ConditionColor { get; set; } = 0;
        public int ConditionNumber { get; set; } = 3;
        public int LightValue { get; set; } = 150;
        public string LightIPAdress { get; set; } = "";
        public int LightPortNumber { get; set; } = 0;
        public int ResultFormDisplayTime { get; set; } = 5;
    }
}
