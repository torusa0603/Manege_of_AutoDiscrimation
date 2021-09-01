#include "stdafx.h"
#include "SampleInspection.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>
#include <chrono>

#pragma warning(disable : 4995)

CSampleInspection::CSampleInspection()
{
}


CSampleInspection::~CSampleInspection()
{
}

/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		���ݎ���msec�擾

	2.�p�����^����
		

	3.�T�v
		���ݎ���msec�擾

	4.�@�\����
		���ݎ���msec�擾

	5.�߂�l
		���ݎ���

	6.���l
		�Ȃ�
------------------------------------------------------------------------------------------*/
unsigned long CSampleInspection::get_msec( void )
{
	std::chrono::system_clock::time_point p = std::chrono::system_clock::now();
	std::chrono::milliseconds ms = std::chrono::duration_cast< std::chrono::milliseconds >( p.time_since_epoch() );
	return	static_cast< unsigned long >( ms.count() );
}

/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		bitmap file��������

	2.�p�����^����
		nstrFilename	(IN)	�摜�t�@�C����
		npbyData		(IN)	�摜�f�[�^
		nuiDataSize		(IN)	�摜�f�[�^�T�C�Y

	3.�T�v
		bitmap file��������

	4.�@�\����
		bitmap file��������

	5.�߂�l
		 0 : ����
		-1 : ���s

	6.���l
		http://hooktail.org/computer/index.php?Bitmap%A5%D5%A5%A1%A5%A4%A5%EB%A4%F2%C6%FE%BD%D0%CE%CF%A4%B7%A4%C6%A4%DF%A4%EB
------------------------------------------------------------------------------------------*/
int CSampleInspection::write_bitmapfile( string nstrFilename, BYTE *npbyData, unsigned int nuiDataSize )
{
	const unsigned int	ui_PALETTE			= 256;
	const unsigned int	ui_INFOHEADERSIZE	= 40;
	const unsigned int	ui_HEADERSIZE	= 14 + ui_INFOHEADERSIZE;

	FILE			*fp;
	unsigned char	uc_header_buf[ ui_HEADERSIZE ];
	unsigned char	*puc_line_buf		= NULL;
	unsigned int	ui_file_size		= ui_HEADERSIZE + ui_PALETTE * sizeof( int ) + nuiDataSize;
	unsigned int	ui_offset_to_data	= ui_HEADERSIZE;
	unsigned long	ui_info_header_size	= ui_INFOHEADERSIZE;
	unsigned int	ui_planes			= 1;
	unsigned int	ui_color			= 8;
	unsigned long	ul_compress			= 0;
	unsigned long	ul_data_size		= nuiDataSize;
	long			l_xppm				= 1;
	long			l_yppm				= 1;
	unsigned char	uc_color_palette[ ui_PALETTE * sizeof( int ) ];

	fopen_s( &fp, nstrFilename.c_str(), "wb" );
	if( NULL == fp )
	{
		return	-1;
	}

	// �w�b�_���i�[����
	uc_header_buf[ 0 ] = 'B';
	uc_header_buf[ 1 ] = 'M';
	memcpy( uc_header_buf + 2, &ui_file_size, sizeof( ui_file_size ) );
	uc_header_buf[ 6 ] = 0;
	uc_header_buf[ 7 ] = 0;
	uc_header_buf[ 8 ] = 0;
	uc_header_buf[ 9 ] = 0;
	memcpy( uc_header_buf + 10, &ui_offset_to_data, sizeof( ui_offset_to_data ) );
	uc_header_buf[ 11 ] = 0;
	uc_header_buf[ 12 ] = 0;
	uc_header_buf[ 13 ] = 0;
	memcpy( uc_header_buf + 14, &ui_info_header_size, sizeof( ui_info_header_size ) );
	uc_header_buf[ 15 ] = 0;
	uc_header_buf[ 16 ] = 0;
	uc_header_buf[ 17 ] = 0;
	memcpy( uc_header_buf + 18, &m_szImageSize.cx, sizeof( int ) );
	memcpy( uc_header_buf + 22, &m_szImageSize.cy, sizeof( int ) );
	memcpy( uc_header_buf + 26, &ui_planes, sizeof( ui_planes ) );
	memcpy( uc_header_buf + 28, &ui_color, sizeof( ui_color ) );
	memcpy( uc_header_buf + 30, &ul_compress, sizeof( ul_compress ) );
	memcpy( uc_header_buf + 34, &ul_data_size, sizeof( ul_data_size ) );
	memcpy( uc_header_buf + 38, &l_xppm, sizeof( l_xppm ) );
	memcpy( uc_header_buf + 42, &l_yppm, sizeof( l_yppm ) );
	uc_header_buf[ 46 ] = 0;
	uc_header_buf[ 47 ] = 0;
	uc_header_buf[ 48 ] = 0;
	uc_header_buf[ 49 ] = 0;
	uc_header_buf[ 50 ] = 0;
	uc_header_buf[ 51 ] = 0;
	uc_header_buf[ 52 ] = 0;
	uc_header_buf[ 53 ] = 0;

	// �J���[�p���b�g�̐ݒ�
	for( unsigned int ui_loop = 0; ui_loop < ui_PALETTE; ui_loop++ )
	{
		uc_color_palette[ ui_loop * sizeof( int ) + 0 ]		= static_cast< unsigned char >( ui_loop );
		uc_color_palette[ ui_loop * sizeof( int ) + 1 ]		= static_cast< unsigned char >( ui_loop );
		uc_color_palette[ ui_loop * sizeof( int ) + 2 ]		= static_cast< unsigned char >( ui_loop );
		uc_color_palette[ ui_loop * sizeof( int ) + 3 ]		= 0xff;
	}

	// �w�b�_�̏�������
	fwrite( uc_header_buf, sizeof( unsigned char ), ui_HEADERSIZE, fp );
	// �J���[�p���b�g�̏�������
	fwrite( uc_color_palette, sizeof( unsigned char ), ui_PALETTE * sizeof( int ), fp );
	// RGB���̏�������
	//if( NULL == ( puc_line_buf = ( unsigned char * )malloc( sizeof( unsigned char ) * m_szImageSize.cx ) ) )
	puc_line_buf	= new unsigned char[ sizeof( unsigned char ) * m_szImageSize.cx ];
	if( NULL == puc_line_buf )
	{
		return	-1;
	}
	for( int i_loop_y = 0; i_loop_y < m_szImageSize.cy; i_loop_y++ )
	{
		for( int i_loop_x = 0; i_loop_x < m_szImageSize.cx; i_loop_x++ )
		{
			puc_line_buf[ i_loop_x ]	= npbyData[ ( m_szImageSize.cy - i_loop_y - 1 ) * m_szImageSize.cx + i_loop_x ];
		}
		fwrite( puc_line_buf, sizeof( unsigned char ), m_szImageSize.cx, fp );
	}

	fclose( fp );
	//free( puc_line_buf );
	delete	[]puc_line_buf;

	return 0;
}


/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		�\�t�g�E�F�A�g���K���s

	2.�p�����^����


	3.�T�v
		�\�t�g�E�F�A�g���K���s

	4.�@�\����


	5.�߂�l


	6.���l
		�Ȃ�
------------------------------------------------------------------------------------------*/
int CSampleInspection::executeSoftwareTrigger( void )
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
	MdigControlFeature( m_milDigitizer, M_FEATURE_EXECUTE, MIL_TEXT( "TriggerSoftware" ), M_TYPE_COMMAND, M_NULL );
	return 0;
}


/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		�T���v������ �\�t�g�g���K�A���摜�擾����

	2.�p�����^����
		nstrFolderName	(IN)	�摜�t�@�C���ۑ���

	3.�T�v
		�T���v������ �\�t�g�g���K�A���摜�擾����

	4.�@�\����
		�T���v������ �\�t�g�g���K�A���摜�擾����

	5.�߂�l
		 0:		OK
		-1:		NG

	6.���l
		�Ȃ�
------------------------------------------------------------------------------------------*/
int CSampleInspection::execInspection( string nstrFolderName )
{
	int				i_ret	= -1;
	unsigned long	ul_start, ul_end, ul_width, ul_trg, ul_data, ul_save;
	BYTE			*pbt_pixel_value_buff	= NULL;

	// �t�@�C�����p(�Ƃ肠����msec���͕��u)
	// ���ݎ����̎擾
	time_t tm_now = time( NULL );
	// �������\���̂Ɋi�[
	struct tm tm_st;
	localtime_s( &tm_st, &tm_now );

	while( true )
	{
		// �摜�f�[�^�z��m��
		unsigned int	ui_total_pixel_num	= m_szImageSize.cx * m_szImageSize.cy;
		pbt_pixel_value_buff				= new BYTE[ ui_total_pixel_num ];

		// �J�n�����ݒ�
		ul_start		= get_msec();

		// 100����s
		for( int i_loop = 0; i_loop < 100; i_loop++ )
		{
			// �\�t�g�g���K���s
			executeSoftwareTrigger();
			ul_trg			= get_msec();

			// �f�[�^�擾
			getMonoBitmapData( ui_total_pixel_num, pbt_pixel_value_buff );
			ul_data			= get_msec();

			// �t�@�C���ۑ�
			const int i_NAMESIZE = 256;
			char	sz_filename[ i_NAMESIZE ];
			sprintf_s( sz_filename, i_NAMESIZE, "%s\\%04d%02d%02d_%02d%02d%02d.%03d.bmp", 
						nstrFolderName.c_str(),
						tm_st.tm_year + 1900, tm_st.tm_mon + 1, tm_st.tm_mday,
						tm_st.tm_hour, tm_st.tm_min, tm_st.tm_sec,
						i_loop );
			write_bitmapfile( sz_filename, pbt_pixel_value_buff, ui_total_pixel_num );
			ul_save			= get_msec();

			// 30fps
			while( true )
			{
				ul_end		= get_msec();
				ul_width	= ul_end - ul_start;
				if( ul_width >= 33 )
				{
					break;
				}
			}
			// ���O�o��
			ul_save			-= ul_data;
			ul_data			-= ul_trg;
			ul_trg			-= ul_start;
			const int i_SIZE = 100;
			char	sz_log[ i_SIZE ];
			sprintf_s( sz_log, i_SIZE, "%03d : time = %04d : triggertime = %04d : datatime = %04d : savetime = %04d\n", i_loop, ul_width, ul_trg, ul_data, ul_save );
			OutputDebugString( sz_log );

			// �����ݒ�
			ul_start		= ul_end;
		}

		i_ret	= 0;
		break;
	}

	if( NULL != pbt_pixel_value_buff )
	{
		delete	[]pbt_pixel_value_buff;
		pbt_pixel_value_buff	= NULL;
	}

	return	i_ret;
}
