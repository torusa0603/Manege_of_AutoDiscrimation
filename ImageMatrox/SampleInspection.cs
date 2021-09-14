using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMatrox
{
    class CSampleInspection
    {
        //        int CSampleInspection::execInspection(string nstrFolderName)
        //        {
        //            int i_ret = -1;
        //            unsigned long ul_start, ul_end, ul_width, ul_trg, ul_data, ul_save;
        //            BYTE* pbt_pixel_value_buff = NULL;

        //            // �t�@�C�����p(�Ƃ肠����msec���͕��u)
        //            // ���ݎ����̎擾
        //            time_t tm_now = time(NULL);
        //    // �������\���̂Ɋi�[
        //        struct tm tm_st;
        //	localtime_s( &tm_st, &tm_now );

        //	while( true )
        //	{
        //		// �摜�f�[�^�z��m��
        //		unsigned int ui_total_pixel_num = m_szImageSize.cx * m_szImageSize.cy;
        //        pbt_pixel_value_buff				= new BYTE[ui_total_pixel_num];

        //		// �J�n�����ݒ�
        //		ul_start		= get_msec();

        //		// 100����s
        //		for(int i_loop = 0; i_loop< 100; i_loop++ )
        //		{
        //			// �\�t�g�g���K���s
        //			executeSoftwareTrigger();
        //        ul_trg			= get_msec();

        //        // �f�[�^�擾
        //        getMonoBitmapData(ui_total_pixel_num, pbt_pixel_value_buff );
        //        ul_data			= get_msec();

        //        // �t�@�C���ۑ�
        //        const int i_NAMESIZE = 256;
        //        char sz_filename[i_NAMESIZE];
        //        sprintf_s(sz_filename, i_NAMESIZE, "%s\\%04d%02d%02d_%02d%02d%02d.%03d.bmp",
        //                    nstrFolderName.c_str(),
        //                    tm_st.tm_year + 1900, tm_st.tm_mon + 1, tm_st.tm_mday,
        //                    tm_st.tm_hour, tm_st.tm_min, tm_st.tm_sec,
        //                    i_loop );
        //        write_bitmapfile(sz_filename, pbt_pixel_value_buff, ui_total_pixel_num );
        //        ul_save			= get_msec();

        //			// 30fps
        //			while( true )
        //			{
        //				ul_end		= get_msec();
        //        ul_width	= ul_end - ul_start;
        //				if(ul_width >= 33 )
        //				{
        //					break;
        //				}
        //			}
        //    // ���O�o��
        //    ul_save			-= ul_data;
        //			ul_data			-= ul_trg;
        //			ul_trg			-= ul_start;
        //			const int i_SIZE = 100;
        //    char sz_log[i_SIZE];
        //    sprintf_s(sz_log, i_SIZE, "%03d : time = %04d : triggertime = %04d : datatime = %04d : savetime = %04d\n", i_loop, ul_width, ul_trg, ul_data, ul_save );
        //    OutputDebugString(sz_log );

        //    // �����ݒ�
        //    ul_start		= ul_end;
        //		}

        //i_ret	= 0;
        //		break;
        //	}

        //	if(NULL != pbt_pixel_value_buff )
        //	{
        //		delete[] pbt_pixel_value_buff;
        //pbt_pixel_value_buff	= NULL;
        //	}

        //	return	i_ret;
        //}
    }
}
