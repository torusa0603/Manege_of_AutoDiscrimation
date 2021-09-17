using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ImageMatrox
{
    public class GlobalStructure
    {
        public struct POINT
        {
            public int x; // x 座標
            public int y; // y 座標

            public POINT(int nlx, int nly)
            {
                this.x = nlx;
                this.y = nly;
            }
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int nlleft, int nltop, int nlright, int nlbottom)
            {
                this.left = nlleft;
                this.top = nltop;
                this.right = nlright;
                this.bottom = nlbottom;
            }
        }

        //public struct SIZE
        //{
        //    public int Width;
        //    public int Height;

        //    public SIZE(int nlWidth, int nlHeight)
        //    {
        //        this.Width = nlWidth;
        //        this.Height = nlHeight;
        //    }
        //}

        public struct SYSTEM_INFO
        {

        }

        public const int MAX_IMAGE_GRAB_NUM = 2; 			//	リングバッファ数					2021/03/12 ノートPCではメモリ消費がひどいため、20 . 2 に変更
        public const int MAX_AVERAGE_IMAGE_GRAB_NUM = 2; 	//	画像を平均化するときの最大画像数	2021/03/12 ノートPCではメモリ消費がひどいため、10 . 2 に変更
        public const int CLEAR_IMAGE_BUFFER = 80;
        public const int FATAL_ERROR_ID = -100;			//	致命的エラーの戻り値

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileStringW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        public enum MTX_TYPE
        {
            MTX_MORPHIS = 0,
            MTX_SOLIOSXCL,
            MTX_SOLIOSXA,
            MTX_METEOR2MC,
            MTX_GIGE,
            MTX_HOST = 100
        };

        //	ボード情報
        public struct BoardParameter
        {

            int iBoardProductCompany;
            int iBoardType;

        }
    ;

        //	カラータイプ
        public enum RGB_TYPE
        {
            RGB = 0,
            R,
            G,
            B
        };
        //	HSI色空間
        public enum HSI_TYPE
        {
            H = 0,
            S,
            I
        };

        //	グラフィックカラー
        public enum GRAPHICCOLOR_TYPE
        {
            GRAPHIC_COLOR_BLACK = 0,
            GRAPHIC_COLOR_WHITE,
            GRAPHIC_COLOR_RED,
            GRAPHIC_COLOR_GREEN,
            GRAPHIC_COLOR_BLUE,
            GRAPHIC_COLOR_YELLOW
        };

        //	パターンマッチング結果情報
        public struct PatternMatchingResult
        {

            double d_pos_x;
            double d_pos_y;
            double d_score;
        }
    ;

        //	ブロブエッジ情報(離型剤による黒光り除去に使う構造体)
        public struct BlobEdgeInfo
        {

            POINT pt_edge_point;
            int i_blob_length;
        };

        //	XYデータ
        public struct XYty
        {

            double dX;
            double dY;
        };

        //	検査結果(NG情報)構造体
        public struct InspectionResultty
        {

            RECT rct_NG_region;
            int i_fake_NG;  //	0:通常欠陥　1:擬似欠陥

        };

        public struct DiffErrorTy
        {

            double d_diff_error;
            int index;
            XYty xy_shift_target_reference; //	基準と検査の位置ずれ量(基準-検査)
        };

        public struct EdgePixelNumty
        {

            double d_edge_pixel_num_rate;
            POINT pt_position;
        };

        //	パターンマッチングパラメーター構造体
        public struct PatternMatchingParametersTy
        {

            int i_max_pattern_find_num;         //	最大パターン検出数
            int i_find_parameter_first_level;   //	検出パラメーターのfirst level
            int i_find_parameter_last_level;    //	検出パラメーターのlast  level
            double d_certainty_val;             //	CertainTy値
            double d_acceptance_val;            //	Accept値

        };

        //	輪郭抽出パラメーター構造体
        public struct EdgeDetectParameterTy
        {

            int i_filter_smoothness;
            int i_second_filter_smoothness;
            int i_threshold_hi;
            int i_threshold_low;
            int i_second_threshold_hi;
            int i_second_threshold_low;

        };


        public struct LineInfo
        {

            int i_start_index;
            int i_end_index;
            double d_average_angle;
            int i_line_ID;

        };

        public struct CrackInfo
        {

            XYty xy_pos;
            double d_crack_length;
            double d_slope_coeff;

        };
    }
}
