#include "stdafx.h"
#include "FilconInspection.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>

#define DEBUG_OUTPUT_HEATMAP_FILES				// for heatmap
#pragma warning(disable : 4995)

#define PI (atan(1.0)*4.0)

CFilconInspection::CFilconInspection()
{
}


CFilconInspection::~CFilconInspection()
{
}

/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		�t�B���R���}�X�N�����A���S���Y��(�f���ȈՔ�)

	2.�p�����^����
		nstrInputImage	(IN)	�����摜�t�@�C���p�X
		nstrResultImage (OUT)	���ʉ摜�t�@�C���p�X

	3.�T�v
		�t�B���R���}�X�N�����A���S���Y��(

	4.�@�\����
		�t�B���R���}�X�N�����A���S���Y��(

	5.�߂�l
		0:	OK
		1:	NG

	6.���l
		�Ȃ�
------------------------------------------------------------------------------------------*/
int CFilconInspection::execInspection(string nstrInputImage, string nstrResultImage)
{
	MIL_ID mil_inspection_image;
	MIL_ID mil_average_image;
	MIL_ID mil_diff_image;
	MIL_ID mil_result_image;
	MIL_ID mil_stat_result;
	SIZE sz_image_size;
	bool b_cut_detected;

	MimAllocResult(m_milSys, M_DEFAULT, M_STAT_LIST, &mil_stat_result);

	//	�摜���t�@�C�����烍�[�h����
	MbufRestore(nstrInputImage.c_str(), m_milSys, &mil_inspection_image);
	//	�摜�T�C�Y�擾
	sz_image_size.cx = (long)MbufInquire(mil_inspection_image, M_SIZE_X, M_NULL);
	sz_image_size.cy = (long)MbufInquire(mil_inspection_image, M_SIZE_Y, M_NULL);

	//	�����Ώۃu���u�̖c���Ək���̍����摜�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_diff_image);
	//	���ω摜�p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);
	//	���ʉ摜�p�̉摜�o�b�t�@���m��
	MbufAllocColor(m_milSys, 3, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_result_image);
	//	���ʉ摜�o�b�t�@�Ɍ����摜���R�s�[���Ă���
	MbufCopyColor(mil_inspection_image, mil_result_image, M_ALL_BANDS);


	//	�摜���R�s�[
	MbufCopy(mil_inspection_image, mil_average_image);

	//	�N���[�W���O�ƃI�[�v�j���O�Ń����Y�̉��ꓙ�̃S�~����������
	MimClose(mil_average_image, mil_average_image, 5, M_GRAYSCALE);
	MimOpen(mil_average_image, mil_average_image, 5, M_GRAYSCALE);

	//	���������
	MimArith(mil_inspection_image, mil_average_image, mil_diff_image, M_SUB_ABS);

	//	�����摜�̓�l��
	MimBinarize(mil_diff_image, mil_diff_image, M_GREATER_OR_EQUAL, 10, M_NULL);

	//	�I�[�v�j���O�Ŕ����ȍ���������
	//MimOpen(mil_diff_image, mil_diff_image, 1, M_GRAYSCALE);
	MimRank(mil_diff_image, mil_diff_image, M_3X3_RECT, M_MEDIAN, M_GRAYSCALE);

	//	���������������Ȃ����OK
	//	�������o���ꂽ���ǂ����́A���摜�̉�f�l��255�ƂȂ��f�����邩�ǂ����Ŕ��f����
	MIL_DOUBLE d_count;
	MimStat(mil_diff_image, mil_stat_result, M_NUMBER, M_EQUAL, 255, M_NULL);
	MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_DOUBLE, &d_count);
	if (d_count > 10)
	{
		b_cut_detected = true;
	}
	else
	{
		b_cut_detected = false;
	}



	//	����������摜�ƍ�������
	MimArith(mil_result_image, mil_diff_image, mil_result_image, M_ADD + M_SATURATION);

	//	�t�@�C���o��
	MbufExport(nstrResultImage.c_str(), M_BMP, mil_result_image);

	MbufFree(mil_inspection_image);
	MbufFree(mil_average_image);
	MbufFree(mil_diff_image);
	MbufFree(mil_result_image);

	MimFree(mil_stat_result);

	if (b_cut_detected == true)
	{
		return 1;
	}
	else
	{
		return 0;
	}

}
