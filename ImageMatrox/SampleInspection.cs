using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Matrox.MatroxImagingLibrary;

namespace ImageMatrox
{

    class CSampleInspection : CMatroxCommon
    {
        private Encoding m_Encoding = Encoding.GetEncoding("Shift_JIS");
        //        /*------------------------------------------------------------------------------------------
        //	1.日本語名
        //		現在時刻msec取得

        //	2.パラメタ説明


        //	3.概要
        //		現在時刻msec取得

        //	4.機能説明
        //		現在時刻msec取得

        //	5.戻り値
        //		現在時刻

        //	6.備考
        //		なし
        //------------------------------------------------------------------------------------------*/
        int get_msec()
        {
            DateTime time_now = System.DateTime.Now;
            return time_now.Millisecond;
        }

        //        /*------------------------------------------------------------------------------------------
        //            1.日本語名
        //                bitmap file書き込み

        //            2.パラメタ説明
        //                nstrFilename	(IN)	画像ファイル名
        //                npbyData		(IN)	画像データ
        //                nuiDataSize		(IN)	画像データサイズ

        //            3.概要
        //                bitmap file書き込み

        //            4.機能説明
        //                bitmap file書き込み

        //            5.戻り値
        //                 0 : 正常
        //                -1 : 失敗

        //            6.備考
        //                http://hooktail.org/computer/index.php?Bitmap%A5%D5%A5%A1%A5%A4%A5%EB%A4%F2%C6%FE%BD%D0%CE%CF%A4%B7%A4%C6%A4%DF%A4%EB
        //        ------------------------------------------------------------------------------------------*/
        private int write_bitmapfile(string nstrFilename, byte[] npbyData, int nuiDataSize)
        {
            const int ui_PALETTE = 256;
            const int ui_INFOHEADERSIZE = 40;
            const int ui_HEADERSIZE = 14 + ui_INFOHEADERSIZE;

            StringBuilder uc_header_buf = new StringBuilder((int)ui_HEADERSIZE);
            StringBuilder puc_line_buf = new StringBuilder();
            int ui_file_size = ui_HEADERSIZE + ui_PALETTE * sizeof(int) + nuiDataSize;
            int ui_offset_to_data = ui_HEADERSIZE;
            long ui_info_header_size = ui_INFOHEADERSIZE;
            int ui_planes = 1;
            int ui_color = 8;
            long ul_compress = 0;
            long ul_data_size = nuiDataSize;
            long l_xppm = 1;
            long l_yppm = 1;
            StringBuilder uc_color_palette = new StringBuilder((int)ui_PALETTE * sizeof(int));

            using (var fp = new StreamWriter(nstrFilename, true, m_Encoding))
            {

                // ヘッダを格納する
                uc_header_buf[0] = 'B';
                uc_header_buf[1] = 'M';
                uc_header_buf[2] = (char)ui_file_size;
                //memcpy(uc_header_buf + 2, &ui_file_size, sizeof(ui_file_size));
                uc_header_buf[6] = (char)(0);
                uc_header_buf[7] = (char)(0);
                uc_header_buf[8] = (char)(0);
                uc_header_buf[9] = (char)(0);
                uc_header_buf[10] = (char)ui_offset_to_data;
                //memcpy(uc_header_buf + 10, &ui_offset_to_data, sizeof(ui_offset_to_data));
                uc_header_buf[11] = (char)(0);
                uc_header_buf[12] = (char)(0);
                uc_header_buf[13] = (char)(0);
                uc_header_buf[14] = (char)ui_info_header_size;
                //memcpy(uc_header_buf + 14, &ui_info_header_size, sizeof(ui_info_header_size));
                uc_header_buf[15] = (char)(0);
                uc_header_buf[16] = (char)(0);
                uc_header_buf[17] = (char)(0);
                uc_header_buf[18] = (char)m_szImageSize.cx;
                //memcpy(uc_header_buf + 18, &m_szImageSize.cx, sizeof(int));
                uc_header_buf[19] = (char)(0);
                uc_header_buf[20] = (char)(0);
                uc_header_buf[21] = (char)(0);
                uc_header_buf[22] = (char)m_szImageSize.cy;
                //memcpy(uc_header_buf + 22, &m_szImageSize.cy, sizeof(int));
                uc_header_buf[23] = (char)(0);
                uc_header_buf[24] = (char)(0);
                uc_header_buf[25] = (char)(0);
                uc_header_buf[26] = (char)ui_planes;
                //memcpy(uc_header_buf + 26, &ui_planes, sizeof(ui_planes));
                uc_header_buf[27] = (char)(0);
                uc_header_buf[28] = (char)ui_color;
                //memcpy(uc_header_buf + 28, &ui_color, sizeof(ui_color));
                uc_header_buf[29] = (char)(0);
                uc_header_buf[30] = (char)ul_compress;
                //memcpy(uc_header_buf + 30, &ul_compress, sizeof(ul_compress));
                uc_header_buf[31] = (char)(0);
                uc_header_buf[32] = (char)(0);
                uc_header_buf[33] = (char)(0);
                uc_header_buf[34] = (char)ul_data_size;
                //memcpy(uc_header_buf + 34, &ul_data_size, sizeof(ul_data_size));
                uc_header_buf[35] = (char)(0);
                uc_header_buf[36] = (char)(0);
                uc_header_buf[37] = (char)(0);
                uc_header_buf[38] = (char)l_xppm;
                //memcpy(uc_header_buf + 38, &l_xppm, sizeof(l_xppm));
                uc_header_buf[39] = (char)(0);
                uc_header_buf[40] = (char)(0);
                uc_header_buf[41] = (char)(0);
                uc_header_buf[42] = (char)l_yppm;
                //memcpy(uc_header_buf + 42, &l_yppm, sizeof(l_yppm));
                uc_header_buf[43] = (char)(0);
                uc_header_buf[44] = (char)(0);
                uc_header_buf[45] = (char)(0);
                uc_header_buf[46] = (char)(0);
                uc_header_buf[47] = (char)(0);
                uc_header_buf[48] = (char)(0);
                uc_header_buf[49] = (char)(0);
                uc_header_buf[50] = (char)(0);
                uc_header_buf[51] = (char)(0);
                uc_header_buf[52] = (char)(0);
                uc_header_buf[53] = (char)(0);

                // カラーパレットの設定
                for (uint ui_loop = 0; ui_loop < ui_PALETTE; ui_loop++)
                {
                    uc_color_palette[(int)ui_loop * sizeof(int) + 0] = (char)(ui_loop);
                    uc_color_palette[(int)ui_loop * sizeof(int) + 1] = (char)(ui_loop);
                    uc_color_palette[(int)ui_loop * sizeof(int) + 2] = (char)(ui_loop);
                    uc_color_palette[(int)ui_loop * sizeof(int) + 3] = (char)(0xff);
                }

                // ヘッダの書き込み
                fp.WriteLine(uc_header_buf);
                // カラーパレットの書き込み
                fp.WriteLine(uc_color_palette);
                // RGB情報の書き込み
                puc_line_buf = new StringBuilder(sizeof(char) * m_szImageSize.cx);
                if (puc_line_buf == null)
                {
                    return -1;
                }
                for (int i_loop_y = 0; i_loop_y < m_szImageSize.cy; i_loop_y++)
                {
                    for (int i_loop_x = 0; i_loop_x < m_szImageSize.cx; i_loop_x++)
                    {
                        puc_line_buf[i_loop_x] = (char)npbyData[(m_szImageSize.cy - i_loop_y - 1) * m_szImageSize.cx + i_loop_x];
                    }
                    fp.WriteLine(puc_line_buf);
                }
            }

            //free( puc_line_buf );

            return 0;
        }


        /*------------------------------------------------------------------------------------------
            1.日本語名
                ソフトウェアトリガ実行

            2.パラメタ説明


            3.概要
                ソフトウェアトリガ実行

            4.機能説明


            5.戻り値


            6.備考
                なし
        ------------------------------------------------------------------------------------------*/
        public int executeSoftwareTrigger()
        /*{
            MIL_TEXT_PTR str_vendor_name;
            str_vendor_name = new MIL_TEXT_CHAR[256];
            MdigInquireFeature(m_milDigitizer,M_FEATURE_VALUE,MIL_TEXT("DeviceVendorName"),M_TYPE_STRING ,str_vendor_name);

            //	Basler
            if( strstr(str_vendor_name,"Basler") != NULL )
            {
                MdigControlFeature( m_milDigitizer, M_FEATURE_EXECUTE, MIL_TEXT( "TriggerSoftware" ), M_TYPE_COMMAND, M_NULL );
            }
            // Point grey
            else
            {
                MdigControlFeature( m_milDigitizer, M_FEATURE_EXECUTE, MIL_TEXT( "TriggerSoftware" ), M_TYPE_COMMAND, M_NULL );
            }
            delete [] str_vendor_name;

            return 0;
        }*/
        {
            MIL.MdigControlFeature(m_milDigitizer, MIL.M_FEATURE_EXECUTE, "TriggerSoftware", MIL.M_TYPE_COMMAND, MIL.M_NULL);
            return 0;
        }


        /*------------------------------------------------------------------------------------------
            1.日本語名
                サンプル動作 ソフトトリガ連続画像取得処理

            2.パラメタ説明
                nstrFolderName	(IN)	画像ファイル保存先

            3.概要
                サンプル動作 ソフトトリガ連続画像取得処理

            4.機能説明
                サンプル動作 ソフトトリガ連続画像取得処理

            5.戻り値
                 0:		OK
                -1:		NG

            6.備考
                なし
        ------------------------------------------------------------------------------------------*/
        public int execInspection(string nstrFolderName)
        {
            int i_ret = -1;
            int ul_start, ul_end, ul_width, ul_trg, ul_data, ul_save;
            byte[] pbt_pixel_value_buff;

            DateTime time_now = System.DateTime.Now;

            while (true)
            {
                // 画像データ配列確保
                int ui_total_pixel_num = m_szImageSize.cx * m_szImageSize.cy;
                pbt_pixel_value_buff = new byte[ui_total_pixel_num];

                // 開始時刻設定
                ul_start = get_msec();

                // 100回実行
                for (int i_loop = 0; i_loop < 100; i_loop++)
                {
                    // ソフトトリガ実行
                    executeSoftwareTrigger();
                    ul_trg = get_msec();

                    // データ取得
                    getMonoBitmapData(ui_total_pixel_num, ref pbt_pixel_value_buff);
                    ul_data = get_msec();

                    // ファイル保存
                    string sz_filename = $"{nstrFolderName}\\{time_now.ToString("yyyyMMdd")}{time_now.ToString("HHmm")}{time_now.ToString("ssfff")}.bmp";
                    write_bitmapfile(sz_filename, pbt_pixel_value_buff, ui_total_pixel_num);
                    ul_save = get_msec();

                    // 30fps
                    while (true)
                    {
                        ul_end = get_msec();
                        ul_width = ul_end - ul_start;
                        if (ul_width >= 33)
                        {
                            break;
                        }
                    }
                    // ログ出力
                    ul_save -= ul_data;
                    ul_data -= ul_trg;
                    ul_trg -= ul_start;
                    OutputDebugString($"loop{i_loop.ToString()}_time = {ul_width.ToString()}_triggertime = {ul_trg.ToString()}_datatime = {ul_data.ToString()}_savetime = {ul_save.ToString()}");

                    // 時刻設定
                    ul_start = ul_end;
                }

                i_ret = 0;
                break;
            }

            if (pbt_pixel_value_buff != null)
            {
                pbt_pixel_value_buff = null;
            }

            return i_ret;
        }
    }
}
