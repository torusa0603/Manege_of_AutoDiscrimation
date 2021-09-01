#include "stdafx.h"
#include "ShinEtsuInspection.h"
#include "GlobalMath.h"
#include <direct.h>
#include <fstream>

#define DEBUG_OUTPUT_HEATMAP_FILES				// for heatmap
#pragma warning(disable : 4995)

#define PI (atan(1.0)*4.0)

CShinEtsuInspection::CShinEtsuInspection()
{
}


CShinEtsuInspection::~CShinEtsuInspection()
{
}
/*------------------------------------------------------------------------------------------
	1.���{�ꖼ
		�M�z���w�H�ƌ����A���S���Y��(�����[�X��)

	2.�p�����^����
		nstrInputImage	(IN)	�����摜�t�@�C���p�X
		nstrResultImage (OUT)	���ʉ摜�t�@�C���p�X
		ndMinDefectSize (IN)	�ŏ������T�C�Y(pixel)
		npiNGContent	(OUT)	NG��� 1:���A2:���� 3:��&����
		npiCurve		(OUT)	�J�[�u�L��(0:�J�[�u�����@1:�J�[�u�L)



	3.�T�v
		�I������

	4.�@�\����
		�I������

	5.�߂�l
		0:	OK
		1:	NG
		-1: �������s(�G�b�W��������Ȃ�)
		-2: �������s(�G�b�W�Əd�Ȃ�u���u���Ȃ�)
		-3: �������s(�c���A���k�̍����摜�Əd�Ȃ�G�b�W���Ȃ�)

	6.���l
		�Ȃ�
------------------------------------------------------------------------------------------*/
int CShinEtsuInspection::execInspection(string nstrInputImage, string nstrResultImage, double ndMinDefectSize, int* npiNGContent, int* npiCurve)
{
	const int ci_min_blob_num = 100;	//	�u���u��͎��A���̒l�ȉ��̃u���u�͖�������

	MIL_ID	mil_inspection_image;
	MIL_ID  mil_result_image;
	MIL_ID	mil_inspection_image_binary;
	MIL_ID  mil_edge_image;
	MIL_ID	mil_blob_image;
	MIL_ID	mil_diff_image;
	MIL_ID	mil_temp_image;
	MIL_ID mil_blob_result;
	MIL_ID mil_blob_feature_list;
	MIL_ID mil_edge;
	MIL_ID mil_edge_result;
	MIL_ID	mil_stat_result;
	MIL_INT  l_num_max = 0;
	MIL_INT	i_edge_found_num = 0;
	MIL_INT l_blob_num;
	MIL_ID mil_average_image;
	int		i_loop;
	int		i_loop2;
	int		i_ret = 0;
	SIZE sz_image_size;
	bool	b_crack_detected = false;	//	���������o
	bool	b_cut_detected = false;		//	�������o
	bool	b_curve = false;			//	�Ȑ������o

	vector< int > vec_exclude_point_index;
	vector< XYty > vec_final_edge_data;
	vector< double > vec_angle;
	vector< double > vec_angle_new;

	vector < LineInfo > vec_line_info;
	vector< int > vec_same_line_index;
	vector< vector< CrackInfo > > vec_all_crack_info;
	vector< CrackInfo > vec_crack_info;
	vector< XYty > vec_edge_data_temp;

	MimAllocResult(m_milSys, M_DEFAULT, M_STAT_LIST, &mil_stat_result);

	//	�摜���t�@�C�����烍�[�h����
	MbufRestore(nstrInputImage.c_str(), m_milSys, &mil_inspection_image);
	//	�摜�T�C�Y�擾
	sz_image_size.cx = (long)MbufInquire(mil_inspection_image, M_SIZE_X, M_NULL);
	sz_image_size.cy = (long)MbufInquire(mil_inspection_image, M_SIZE_Y, M_NULL);

	//	��l���p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_inspection_image_binary);
	//	�G�b�W�摜�p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_edge_image);
	//	�u���u�摜�p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_blob_image);
	//	�����Ώۃu���u�̖c���Ək���̍����摜�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_diff_image);
	//	��Ɨp�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_temp_image);
	//	���ω摜�p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);
	//	���ʉ摜�p�̉摜�o�b�t�@���m��
	MbufAllocColor(m_milSys, 3, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC + M_DISP + M_PACKED + M_BGR24, &mil_result_image);
	//	���ʉ摜�o�b�t�@�Ɍ����摜���R�s�[���Ă���
	MbufCopyColor(mil_inspection_image, mil_result_image, M_ALL_BANDS);

	// ************************
	//
	//	���f�B�A���t�B���^�Ńm�C�Y���������Ă���
	//
	// ************************

	MimRank(mil_inspection_image, mil_inspection_image, M_3X3_RECT, M_MEDIAN, M_GRAYSCALE);

	// ************************
	//
	//	��l��
	//
	// ************************

	//	��l�������s�B臒l�͎�����
	MimBinarize(mil_inspection_image, mil_inspection_image_binary, M_BIMODAL + M_GREATER, M_NULL, M_NULL);

	// ************************
	//
	//	�u���u���
	//
	// ************************

	//�@�u���u��͌��ʊi�[�o�b�t�@���m�ۂ���
	MblobAllocResult(m_milSys, &mil_blob_result);
	//�@�u���u��͓����ʃ��X�g�o�b�t�@���m�ۂ���
	MblobAllocFeatureList(m_milSys, &mil_blob_feature_list);

	//	�S�Ă̓�����ݒ肷��
	MblobSelectFeature(mil_blob_feature_list, M_ALL_FEATURES);
	//	�u���u��͂��o�C�i�����[�h�ɂ���(������)
	MblobControl(mil_blob_result, M_IDENTIFIER_TYPE, M_BINARY);
	//	�u���u��͂����s
	MblobCalculate(mil_inspection_image_binary, M_NULL, mil_blob_feature_list, mil_blob_result);
	//	�ŏ������T�C�Y�ɖ����Ȃ��u���u�͍폜����B
	//	������̃u���u�𑪒茋�ʂ���폜
	MblobSelect(mil_blob_result, M_DELETE, M_AREA, M_LESS_OR_EQUAL, ci_min_blob_num, M_NULL);
	//	��͂��ꂽ�u���u�̐����擾����
	MblobGetNumber(mil_blob_result, &l_blob_num);

	// ************************
	//
	//	�G�b�W���o
	//
	// ************************


	//	�G�b�W�t�@�C���_�[�R���e�L�X�g���m��
	MedgeAlloc(m_milSys, M_CONTOUR, M_DEFAULT, &mil_edge);
	MedgeAllocResult(m_milSys, M_DEFAULT, &mil_edge_result);

	//	smoothness��ݒ�
	MedgeControl(mil_edge, M_FILTER_SMOOTHNESS, 50.0);
	//	臒l��very high��
	MedgeControl(mil_edge, M_THRESHOLD_MODE, M_VERY_HIGH);
	//	�G�b�W�Ԃ̃M���b�v�𖄂߂� 20�s�N�Z��
	MedgeControl(mil_edge, M_FILL_GAP_DISTANCE, 20);

	//	�G�b�W���o���s
	MedgeCalculate(mil_edge, mil_inspection_image, M_NULL, M_NULL, M_NULL, mil_edge_result, M_DEFAULT);

	//	�G�b�W�����擾
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_NUMBER_OF_CHAINS + M_TYPE_MIL_INT, &i_edge_found_num, M_NULL);
	//	�G�b�W�摜���쐬����
	if (i_edge_found_num > 0)
	{
		MgraColor(m_milGraphic, M_COLOR_WHITE);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);
	}
	//	�G�b�W������Ȃ���Ό������s
	else
	{
		i_ret = -1;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}

	// ************************
	//
	//	�u���u�̒����猟���Ώۂ̃p�^�[���̃u���u�𒊏o����
	//
	// ************************

	MIL_DOUBLE *d_label;
	MIL_INT  l_num;
	int	i_max_blob_index = -1;

	//	�e�u���u�̃C���f�b�N�X���擾
	d_label = new MIL_DOUBLE[(int)l_blob_num];
	MblobGetResult(mil_blob_result, M_LABEL_VALUE, d_label);

	//	�e�u���u���ƂɃ`�F�b�N���Ă���
	for (i_loop = 0; i_loop < (int)l_blob_num; i_loop++)
	{
		//	���ڂ��Ă���u���u���������o���ău���u�摜�����B
		MbufClear(mil_blob_image, 0);
		MblobSelect(mil_blob_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, d_label[i_loop], M_NULL);
		MblobFill(mil_blob_result, mil_blob_image, M_INCLUDED_BLOBS, 255);

		//	�u���u�������c��������
		MimDilate(mil_blob_image, mil_blob_image, 2, M_GRAYSCALE);

		//	�u���u�ƃG�b�W�摜�̏d�Ȃ����Ƃ��낾���𒊏o
		MimArith(mil_blob_image, mil_edge_image, mil_blob_image, M_AND);
		//	�d�Ȃ����Ƃ���̉�f�����J�E���g
		MimStat(mil_blob_image, mil_stat_result, M_NUMBER, M_GREATER, 1, M_NULL);
		MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_MIL_INT, &l_num);
		//	�ő�̐��ƂȂ�u���u�̃C���f�b�N�X���L��
		if (l_num > l_num_max)
		{
			l_num_max = l_num;
			i_max_blob_index = (int)d_label[i_loop];
		}

	}
	delete[] d_label;

	//	�����G�b�W�Əd�Ȃ���̂��Ȃ���΃G���[�ŕԂ�
	if (i_max_blob_index == -1)
	{
		i_ret = -2;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}
	//	�d�Ȃ��Ă���̂�����΁A���̃u���u�������Ώۃp�^�[���Ƃ���
	else
	{
		MbufClear(mil_blob_image, 0);
		MblobSelect(mil_blob_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_blob_index, M_NULL);
		MblobFill(mil_blob_result, mil_blob_image, M_INCLUDED_BLOBS, 255);
	}

	// ************************
	//
	//	�����Ώۃu���u��c���������̂ƁA�k���������̂̍����摜���쐬����
	//
	// ************************

	int i_boutyou_num = 8;

	//	�c���摜�쐬
	MimDilate(mil_blob_image, mil_diff_image, i_boutyou_num, M_GRAYSCALE);
	//	�k���摜�쐬
	MimErode(mil_blob_image, mil_temp_image, i_boutyou_num, M_GRAYSCALE);
	//	�c��-�����摜�쐬
	MimArith(mil_diff_image, mil_temp_image, mil_diff_image, M_SUB_ABS + M_SATURATION);

	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_0DiffImage.bmp";
		MbufExport(str_file.c_str(), M_BMP, mil_diff_image);
	}


	// ************************
	//
	//	�����摜���Ɋ܂܂��G�b�W(�`�F�[��)�̂����A�܂܂��s�N�Z�������ő�ƂȂ�G�b�W�i�`�F�[��)������o����
	//
	// ************************

	int	i_max_edge_index = -1;
	MIL_DOUBLE *d_edge_label;
	d_edge_label = new MIL_DOUBLE[(int)i_edge_found_num];
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_LABEL_VALUE, d_edge_label, M_NULL);

	l_num_max = 0;
	//	�e�u�G�b�W���ƂɃ`�F�b�N���Ă���
	for (i_loop = 0; i_loop < (int)i_edge_found_num; i_loop++)
	{

		//	���ڂ��Ă���G�b�W���������o���ăG�b�W�摜�����B
		MbufClear(mil_edge_image, 0);
		MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, d_edge_label[i_loop], M_NULL);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);

		//	�����摜�ƃG�b�W�摜�̏d�Ȃ����Ƃ��낾���𒊏o
		MimArith(mil_diff_image, mil_edge_image, mil_edge_image, M_AND);

		//	�d�Ȃ����Ƃ���̉�f�����J�E���g
		MimStat(mil_edge_image, mil_stat_result, M_NUMBER, M_GREATER, 1, M_NULL);
		MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_MIL_INT, &l_num);
		//	�ő�̐��ƂȂ�G�b�W�̃C���f�b�N�X���L��
		if (l_num > l_num_max)
		{
			l_num_max = l_num;
			i_max_edge_index = (int)d_edge_label[i_loop];
		}
	}
	delete[] d_edge_label;

	//	�����G�b�W�Əd�Ȃ���̂��Ȃ���΃G���[�ŕԂ�
	if (i_max_edge_index == -1)
	{
		i_ret = -3;
		outputErrorLog(__func__, i_ret);
		goto Finish;
	}

	if (m_bDebugON == true)
	{
		MbufClear(mil_edge_image, 0);
		MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_edge_index, M_NULL);
		MedgeDraw(M_DEFAULT, mil_edge_result, mil_edge_image, M_DRAW_EDGES, M_DEFAULT, M_DEFAULT);

		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_1EdgeChain.bmp";
		MbufExport(str_file.c_str(), M_BMP, mil_edge_image);
	}


	// ************************
	//
	//	���o�����G�b�W�̓_�Q���擾���A�����摜����͂ݏo���_�Q���L������
	//
	// ************************

	MIL_DOUBLE d_edge_chain_num[1];


	//	���o�����G�b�W��I��
	MedgeSelect(mil_edge_result, M_INCLUDE_ONLY, M_LABEL_VALUE, M_EQUAL, i_max_edge_index, M_NULL);
	//	�G�b�W�̃|�C���g�����擾
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_NUMBER_OF_CHAINED_EDGELS, d_edge_chain_num, M_NULL);

	//	�G�b�W�_�Q���擾
	MIL_DOUBLE *d_edge_chain_X;
	MIL_DOUBLE *d_edge_chain_Y;
	d_edge_chain_X = new MIL_DOUBLE[(int)d_edge_chain_num[0]];
	d_edge_chain_Y = new MIL_DOUBLE[(int)d_edge_chain_num[0]];
	MedgeGetResult(mil_edge_result, M_DEFAULT, M_CHAIN, d_edge_chain_X, d_edge_chain_Y);

	BYTE bt_pixel_value[1];
	//	�G�b�W�_�Q�̒��ō����摜����͂ݏo���_�̃C���f�b�N�X�����߂�
	for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
	{
		//	�����摜��̃G�b�W���W�̋P�x�l���擾���A���ꂪ255�łȂ���΂͂ݏo�Ă���Ɣ��f
		MbufGet2d(mil_diff_image, (MIL_INT)d_edge_chain_X[i_loop], (MIL_INT)d_edge_chain_Y[i_loop], 1, 1, bt_pixel_value);
		if (bt_pixel_value[0] != 255)
		{
			vec_exclude_point_index.push_back(i_loop);
		}
	}

	// ************************
	//
	//	�G�b�W�̓_�Q����A�͂ݏo���_�Q���������āA�V���ȓ_�Q�z����쐬����
	//
	// ************************

	XYty xy_data;

	//	�͂ݏo���_�����݂���
	if (vec_exclude_point_index.size() != 0)
	{
		for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
		{
			for (i_loop2 = 0; i_loop2 < (int)vec_exclude_point_index.size(); i_loop2++)
			{
				//	�͂ݏo���f�[�^�ł���
				if (vec_exclude_point_index[i_loop2] == i_loop)
				{
					break;
				}
			}
			//	�͂ݏo�Ă��Ȃ���Δz��Ɋi�[���Ă���
			if (i_loop2 == (int)vec_exclude_point_index.size())
			{
				xy_data.dX = d_edge_chain_X[i_loop];
				xy_data.dY = d_edge_chain_Y[i_loop];
				vec_final_edge_data.push_back(xy_data);
			}
		}
	}
	//	�͂ݏo���_�����݂��Ȃ�
	else
	{
		for (i_loop = 0; i_loop < (int)d_edge_chain_num[0]; i_loop++)
		{
			xy_data.dX = d_edge_chain_X[i_loop];
			xy_data.dY = d_edge_chain_Y[i_loop];
			vec_final_edge_data.push_back(xy_data);

		}
	}

	//	���̃G�b�W�̓_�Q�z����t���܂ɓ���ւ���B(���������̃G�b�W��X���傫���ق����珇�ɓ����Ă��邩��A���������ɔz��ɓ����悤�ɂ���)
	for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
	{
		vec_edge_data_temp.push_back(vec_final_edge_data[vec_final_edge_data.size() - i_loop - 1]);
	}
	vec_final_edge_data = vec_edge_data_temp;


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_2EdgeChainExclude.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
		{
			sprintf_s(chr_buff, "%f,%f", vec_final_edge_data[i_loop].dX, vec_final_edge_data[i_loop].dY);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}


	// ************************
	//
	//	�G�b�W�_�ɂ����āA�אڂ���G�b�W�_�Ƃ̊p�x�����߂�
	//
	// ************************

	int i_average_num = 3;	//	�O3�_�̕��ρA���3�_�̕��ς̌v2�_����p�x�����߂�
	XYty xy_data2;
	double d_angle;
	for (i_loop = 0; i_loop < (int)vec_final_edge_data.size(); i_loop++)
	{
		if (i_loop >= i_average_num && i_loop + i_average_num < (int)vec_final_edge_data.size())
		{
			xy_data.dX = xy_data.dY = xy_data2.dX = xy_data2.dY = 0.0;
			//	�ON�_�̕���
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				xy_data.dX += vec_final_edge_data[i_loop - i_loop2 - 1].dX;
				xy_data.dY += vec_final_edge_data[i_loop - i_loop2 - 1].dY;
			}
			xy_data.dX /= (double)i_average_num;
			xy_data.dY /= (double)i_average_num;
			//	���N�_�̕���
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				xy_data2.dX += vec_final_edge_data[i_loop + i_loop2 + 1].dX;
				xy_data2.dY += vec_final_edge_data[i_loop + i_loop2 + 1].dY;
			}
			xy_data2.dX /= (double)i_average_num;
			xy_data2.dY /= (double)i_average_num;

			//	�p�x�����߂�(-180�`180��)�ƂȂ�
			d_angle = atan2(xy_data.dY - xy_data2.dY, xy_data.dX - xy_data2.dX);
			vec_angle.push_back(d_angle * 180.0 / PI);
		}
	}
	//	���̂܂܂��ƁA�p�x�̔z��(vector)�ɂ͑O��3�_���̃f�[�^�������ĂȂ��̂ŁA�ׂ̊p�x�Ŗ��߂Ă��܂�
	{
		d_angle = vec_angle[0];
		auto it = vec_angle.begin();
		for (i_loop = 0; i_loop < i_average_num; i_loop++)
		{
			it = vec_angle.insert(it, d_angle);
		}
		d_angle = vec_angle.back();
		for (i_loop = 0; i_loop < i_average_num; i_loop++)
		{
			vec_angle.push_back(d_angle);
		}
	}

	//	180����-180���̋��E���t�߂͎��ۂ�1,2����������ĂȂ��Ă�179�A-179�Ƃ�358���Ƃ������̂�
	//	�A��������
	//	�אڂ���p�x��180���ȏ㗣��Ă���360����]���ĕ␳����
	for (i_loop = 0; i_loop < (int)vec_angle.size() - 1; i_loop++)
	{
		//	���̓_�Ǝ��̓_�ŁA�����Z���s���A���̌��ʂ�180���ȏ�ł����(�O�̓_��+�A���̓_��-)�A���̓_��360�����Z
		//	���ʂ�-180���ȉ��ł����(�O�̓_��-�A���̓_��+)�A���̓_����360�����Z
		if (vec_angle[i_loop] - vec_angle[i_loop + 1] > 180.0)
		{
			vec_angle[i_loop + 1] = vec_angle[i_loop + 1] + 360.0;
		}
		else if (vec_angle[i_loop] - vec_angle[i_loop + 1] < -180.0)
		{
			vec_angle[i_loop + 1] = vec_angle[i_loop + 1] - 360.0;
		}
	}

	// ************************
	//
	//	���߂��G�b�W�p�x�z��𕽋ω����m�C�Y�����
	//
	// ************************

	i_average_num = 5;		//	�������g�̃f�[�^�𒆐S�ɕ��ς������̂Ŋ�ɂ���

	for (i_loop = 0; i_loop < (int)vec_angle.size(); i_loop++)
	{
		//	�f�[�^�[�̕��ω��o���Ȃ��Ƃ���͂��̂܂܂ɂ��Ă���

		if (i_loop >= i_average_num / 2 && i_loop + (i_average_num / 2) < (int)vec_angle.size())
		{
			d_angle = 0.0;
			//	�f�[�^�[�łȂ��̂ŕ���
			for (i_loop2 = 0; i_loop2 < i_average_num; i_loop2++)
			{
				d_angle = d_angle + vec_angle[i_loop - (i_average_num / 2) + i_loop2];
			}
			vec_angle_new.push_back(d_angle / (double)i_average_num);
		}
		else
		{
			vec_angle_new.push_back(vec_angle[i_loop]);
		}
	}
	vec_angle = vec_angle_new;

	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_3Angle.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_angle.size(); i_loop++)
		{
			sprintf_s(chr_buff, "%f", vec_angle[i_loop]);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}

	// ************************
	//
	//	�擪���珇�Ɋp�x�����Ă����A�����p�x��100�A���ő��������̂𒼐��Ƃ���
	//
	// ************************
	bool b_line_start = false;
	LineInfo line_info;
	const double cd_same_line_angle_threshold = 5.0;	//�O��̊p�x���ꂪ5���ȓ��Ȃ璼���Ƃ݂Ȃ�
	const int ci_line_length_threshold = 100;
	double d_line_start_angle = 0.0;

	line_info.i_start_index = 0;

	for (i_loop = 0; i_loop < (int)vec_angle.size() - 1; i_loop++)
	{
		//	�����̎n�_����܂��Ă�ꍇ
		if (b_line_start == true)
		{
			//	�J�n�p�x�ƌ��݂̊p�x�̍���������
			d_angle = fabs(vec_angle[i_loop] - d_line_start_angle);
		}
		//	�����̎n�_���܂���܂��Ă��Ȃ��ꍇ
		else
		{
			//	�אڂ���2�_�̊p�x�̍������߂�
			d_angle = vec_angle[i_loop + 1] - vec_angle[i_loop];
		}



		//	�����ł���
		if (fabs(d_angle) < cd_same_line_angle_threshold)
		{
			//	�܂������̎n�_���߂Ă��Ȃ���΂����Œ�߂�
			if (b_line_start == false)
			{
				b_line_start = true;
				line_info.i_start_index = i_loop;
				d_line_start_angle = (vec_angle[i_loop + 1] + vec_angle[i_loop]) / 2.0;
			}
		}
		//	�p�x�̍����傫��(�����łȂ�)
		else
		{
			//	�����̎n�_�͐ݒ肳�ꂽ��ԂŁA�Ȃ����\���ɒ�����������΁A�����Œ����̏I�_���m�肳���A������o�^����
			if (b_line_start == true && i_loop - line_info.i_start_index > ci_line_length_threshold)
			{
				line_info.i_end_index = i_loop;
				//	���̒����̕��ϊp�x�����߂�
				d_angle = 0.0;
				for (i_loop2 = line_info.i_start_index; i_loop2 <= line_info.i_end_index; i_loop2++)
				{
					d_angle += vec_angle[i_loop2];
				}
				line_info.d_average_angle = d_angle / (double)(line_info.i_end_index - line_info.i_start_index);
				line_info.i_line_ID = (int)vec_line_info.size();
				//	�����̓o�^
				vec_line_info.push_back(line_info);
			}
			//	�n�_����t���O��������
			b_line_start = false;
		}

	}
	//	�Ō�̒����𔻒f����
	if (b_line_start == true && i_loop - line_info.i_start_index > ci_line_length_threshold)
	{
		line_info.i_end_index = i_loop;
		//	���̒����̕��ϊp�x�����߂�
		d_angle = 0.0;
		for (i_loop2 = line_info.i_start_index; i_loop2 <= line_info.i_end_index; i_loop2++)
		{
			d_angle += vec_angle[i_loop2];
		}
		line_info.d_average_angle = d_angle / (double)(line_info.i_end_index - line_info.i_start_index);
		line_info.i_line_ID = (int)vec_line_info.size();
		//	�����̓o�^
		vec_line_info.push_back(line_info);
	}

	//	����������Ȃ��A��������1�����Ȃ����̒������G�b�W�����̔����ȉ��ł���΁A�傫�Ȍ��������݂���Ƃ��Ă��܂�
	if (vec_line_info.size() == 0)
	{
		//	�������o�t���O�𗧂Ă�
		b_crack_detected = true;
	}
	else if (vec_line_info.size() == 1)
	{
		if ((vec_line_info[0].i_end_index - vec_line_info[0].i_start_index) < (int)(vec_angle.size() * 0.5))
		{
			//	�������o�t���O�𗧂Ă�
			b_crack_detected = true;
		}
	}


	// ************************
	//
	//	���߂������̂����A�אڂ��钼���œ��꒼����ɂ�����̂�����΂����A������
	//
	// ************************

	int i_line_length;
	double *d_x;
	double *d_y;
	double d_coeff[2];
	double d_distance;

	CGlobalMath c_global_math;
	const double cd_same_angle_threshold = 2.0;			//	��
	const double cd_same_line_point_threshold = 2.0;  //	�s�N�Z�����W

	//	������2�{�ȏ゠��΍s��
	if ((int)vec_line_info.size() >= 2)
	{
		for (i_loop = 0; i_loop < (int)vec_line_info.size() - 1; i_loop++)
		{
			//	N�{�ڂ̒�����N+1�{�ڂ̒������p�x���قȂ�΁A���꒼����ɂ��邱�Ƃ͂��肦�Ȃ��̂�
			//	N�{�ڂ͂��̂܂ܓo�^
			if (fabs(vec_line_info[i_loop].d_average_angle - vec_line_info[i_loop + 1].d_average_angle) > cd_same_angle_threshold)
			{
				continue;
			}

			//	����p�x�Ȃ瓯�꒼����ɂ���\��������̂Ń`�F�b�N

#if 0
			XYty one_data;
			//	N�{�ڂ̒�����xy���擾
			i_line_length = vec_line_info[i_loop].i_end_index - vec_line_info[i_loop].i_start_index + 1;
			d_x = new double[i_line_length];
			d_y = new double[i_line_length];

			for (i_loop2 = 0; i_loop2 < i_line_length; i_loop2++)
			{
				d_x[i_loop2] = vec_final_edge_data[vec_line_info[i_loop].i_start_index + i_loop2].dX;
				d_y[i_loop2] = vec_final_edge_data[vec_line_info[i_loop].i_start_index + i_loop2].dY;
			}
			//	���̒�����̓_�Q�����A���������߂�
			c_global_math.ApproximationByLeastSquaresMethod(d_x, d_y, i_line_length, 1, d_coeff);

			//	N+1�{�ڂ̒�����1�_�ڂƁA���̒����Ƃ̋��������߂�

			one_data.dX = vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index].dX;
			one_data.dY = vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index].dY;
			//	�_�ƒ����̋����̌���
			d_distance = fabs(d_coeff[0] * one_data.dX - one_data.dY + d_coeff[1]) / sqrt(d_coeff[0] * d_coeff[0] + 1);
			//	�_��������ɂ���΁AN+1�{�ڂ̒�����N�{�ڂ̒����ł���A���̊Ԃɂ͌���������\��������Ƃ������Ƃ�������
			//	N+1�{�ڂ̒�����ID��N�{�ڂ�ID�Ɠ���ɂ���
			if (d_distance < cd_same_line_point_threshold)
			{
				vec_line_info[i_loop + 1].i_line_ID = vec_line_info[i_loop].i_line_ID;
			}
			delete[] d_x;
			delete[] d_y;
#else
			double d_x_first_ave = 0.0;
			double d_y_first_ave = 0.0;
			double d_x_second_ave = 0.0;
			double d_y_second_ave = 0.0;
			const double cd_same_angle_range = 10.0;

			//N�{�ڂ̏I�_��3�_�̕��ύ��W�����߂�
			for (i_loop2 = 0; i_loop2 < 3; i_loop2++)
			{
				d_x_first_ave += vec_final_edge_data[vec_line_info[i_loop].i_end_index + i_loop2 - 3].dX;
				d_y_first_ave += vec_final_edge_data[vec_line_info[i_loop].i_end_index + i_loop2 - 3].dY;
			}
			d_x_first_ave /= 3.0;
			d_y_first_ave /= 3.0;
			//	N+1�{�ڂ̎n�_��3�_�̕��ύ��W�����߂�
			for (i_loop2 = 0; i_loop2 < 3; i_loop2++)
			{
				d_x_second_ave += vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index + i_loop2].dX;
				d_y_second_ave += vec_final_edge_data[vec_line_info[i_loop + 1].i_start_index + i_loop2].dY;
			}
			d_x_second_ave /= 3.0;
			d_y_second_ave /= 3.0;

			//	����2�_���Ȃ��p�x��N��������N+1�̊p�x�Ɠ����悤�ł���Γ��꒼����Ƃ��A���̊ԂɌ���������\��������
			d_angle = atan2(d_y_first_ave - d_y_second_ave, d_x_first_ave - d_x_second_ave);
			d_angle = d_angle * 180.0 / PI;

			//	�p�x��A��������
			if (d_angle - vec_line_info[i_loop].d_average_angle > 180.0)
			{
				d_angle = d_angle - 360.0;
			}
			else if (d_angle - vec_line_info[i_loop].d_average_angle < -180.0)
			{
				d_angle = d_angle + 360.0;
			}

			if (fabs(d_angle - vec_line_info[i_loop].d_average_angle) < cd_same_angle_range)
			{
				vec_line_info[i_loop + 1].i_line_ID = vec_line_info[i_loop].i_line_ID;
			}
#endif

		}
	}


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_4LineInfo.csv";
		char chr_buff[MAX_PATH];


		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			sprintf_s(chr_buff, "Line%d\nStartIndex,%d\nEndIndex,%d\nAverageAngle,%f\nLineID,%d", i_loop, vec_line_info[i_loop].i_start_index, vec_line_info[i_loop].i_end_index,
				vec_line_info[i_loop].d_average_angle, vec_line_info[i_loop].i_line_ID);
			write_file << chr_buff << endl;
		}
		write_file.close();
	}

	// ************************
	//
	//	�Ȑ������݂��邩���肷��
	//
	// ************************

	//	������1�{�����Ȃ��ꍇ�͋Ȑ��͑��݂��Ȃ�
	if ((int)vec_line_info.size() < 2)
	{
		b_curve = false;
	}
	else
	{
		//	1�{�ڂ̒�����ID�ƈقȂ���̂����������_�ŋȐ������݂���Ɣ��f����
		int i_ID = vec_line_info[0].i_line_ID;
		for (i_loop = 1; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			if (vec_line_info[i_loop].i_line_ID != i_ID)
			{
				b_curve = true;
				break;
			}
		}
	}

	// ************************
	//
	//	�e�����Ō��������o����(����͂܂����Ȃ�)
	//
	// ************************

	CrackInfo crack_info;

	const double cd_min_angle = 80.0;		//	Y���ɕ��s�Ȓ����Ƃ݂Ȃ��p�x1
	const double cd_max_angle = 100.0;		//	Y���ɕ��s�Ȓ����Ƃ݂Ȃ��p�x2    �p�x1 < abs(��) < �p�x2  �ł����Y���ɕ��s�Ȓ����Ƃ���B

	//	�����͘A�����ꂽ�����̊Ԃɂ���̂ŁA�܂��A�����ꂽ���������߂�

	//	������2�{�ȏ゠��΍s���B����1�{�ȉ��Ȃ猇���͂Ȃ�
	if ((int)vec_line_info.size() >= 2)
	{
		//	�ŏ��ɋ��߂������̖{���Ń��[�v�����邪�AID�����[�v�����Ă���Ӗ��BID�ԍ��͒����̖{�����傫�������ɂ͂Ȃ�Ȃ�
		for (i_loop = 0; i_loop < (int)vec_line_info.size(); i_loop++)
		{
			vec_same_line_index.clear();
			//	ID�����������C���f�b�N�X�����߂�
			for (i_loop2 = 0; i_loop2 < (int)vec_line_info.size(); i_loop2++)
			{
				if (vec_line_info[i_loop2].i_line_ID == i_loop)
				{
					vec_same_line_index.push_back(i_loop2);
				}
			}
			//	��������ID�̒�����2�{�ȏ゠��΁A���̒�����Ɍ���������
			if ((int)vec_same_line_index.size() >= 2)
			{
				//	��������ID�̒��������A���������߂�
				//	�{���͓�������ID�̑S�Ă̒������������A���������߂�̂��x�X�g�����A�ʓ|�������̂ŁA��ڂ̒���ID�̒������狁�߂�

				//	������xy���擾
				i_line_length = vec_line_info[vec_same_line_index[0]].i_end_index - vec_line_info[vec_same_line_index[0]].i_start_index + 1;
				d_x = new double[i_line_length];
				d_y = new double[i_line_length];

				for (i_loop2 = 0; i_loop2 < i_line_length; i_loop2++)
				{
					//	�p�x��80<��<100�̂Ƃ��A������Y���ɕ��s�ɋ߂��Ȃ邽�ߋߎ����������܂��Ђ��Ȃ��̂Ŕ��]������
					if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
					{
						//xy���]
						d_y[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dX;
						d_x[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dY;
					}
					else
					{
						d_x[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dX;
						d_y[i_loop2] = vec_final_edge_data[vec_line_info[vec_same_line_index[0]].i_start_index + i_loop2].dY;
					}

				}
				//	���̒�����̓_�Q�����A���������߂�
				c_global_math.ApproximationByLeastSquaresMethod(d_x, d_y, i_line_length, 1, d_coeff);

				//	���������߂�͈͂́A��������ID�ŁA�����ƒ����̊Ԃ̋�ԁB������܂����߂�
				for (i_loop2 = 0; i_loop2 < (int)vec_same_line_index.size() - 1; i_loop2++)
				{
					vec_crack_info.clear();

					i_line_length = vec_line_info[vec_same_line_index[i_loop2 + 1]].i_start_index - vec_line_info[vec_same_line_index[i_loop2]].i_end_index + 1;
					for (int i_loop3 = 0; i_loop3 < i_line_length; i_loop3++)
					{
						//	�p�x��80<��<100�̂Ƃ��A������Y���ɕ��s�ɋ߂��Ȃ邽�ߋߎ����������܂��Ђ��Ȃ��̂Ŕ��]������
						if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
						{
							//	XY���]
							crack_info.xy_pos.dY = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dX;
							crack_info.xy_pos.dX = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dY;
						}
						else
						{
							crack_info.xy_pos.dX = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dX;
							crack_info.xy_pos.dY = vec_final_edge_data[vec_line_info[vec_same_line_index[i_loop2]].i_end_index + i_loop3].dY;
						}

						vec_crack_info.push_back(crack_info);
					}
					//	���������߂�B��A�����ƁA�������̓_�Ƃ̋��������߂�
					for (int i_loop3 = 0; i_loop3 < (int)vec_crack_info.size(); i_loop3++)
					{
						//	�_�ƒ����̋����̌���
						d_distance = fabs(d_coeff[0] * vec_crack_info[i_loop3].xy_pos.dX - vec_crack_info[i_loop3].xy_pos.dY + d_coeff[1]) / sqrt(d_coeff[0] * d_coeff[0] + 1);
						vec_crack_info[i_loop3].d_crack_length = d_distance;
						vec_crack_info[i_loop3].d_slope_coeff = d_coeff[0];

					}
					//	�p�x��80<��<100�̂Ƃ��A������Y���ɕ��s�ɋ߂��Ȃ邽�ߋߎ����������܂��Ђ��Ȃ��̂Ŕ��]������
					if (cd_min_angle < fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) && fabs(vec_line_info[vec_same_line_index[0]].d_average_angle) < cd_max_angle)
					{
						//	���������߂��̂�XY���W�����ɖ߂�
						for (int i_loop3 = 0; i_loop3 < (int)vec_crack_info.size(); i_loop3++)
						{
							XYty xy_temp;
							xy_temp.dX = vec_crack_info[i_loop3].xy_pos.dX;
							xy_temp.dY = vec_crack_info[i_loop3].xy_pos.dY;
							vec_crack_info[i_loop3].xy_pos.dX = xy_temp.dY;
							vec_crack_info[i_loop3].xy_pos.dY = xy_temp.dX;
						}
					}

					vec_all_crack_info.push_back(vec_crack_info);
				}
				delete[] d_x;
				delete[] d_y;
			}
		}
	}


	if (m_bDebugON == true)
	{
		string str_file = m_strDebugFolder + m_strDebugFileIdentifiedName + "_5CrackInfo.csv";
		char chr_buff[MAX_PATH];

		ofstream write_file;
		write_file.open(str_file, ios::out);
		for (i_loop = 0; i_loop < (int)vec_all_crack_info.size(); i_loop++)
		{
			sprintf_s(chr_buff, "Crack%d\nX,Y,Length", i_loop);
			write_file << chr_buff << endl;
			for (i_loop2 = 0; i_loop2 < (int)vec_all_crack_info[i_loop].size(); i_loop2++)
			{
				sprintf_s(chr_buff, "%f,%f,%f", vec_all_crack_info[i_loop][i_loop2].xy_pos.dX, vec_all_crack_info[i_loop][i_loop2].xy_pos.dY, vec_all_crack_info[i_loop][i_loop2].d_crack_length);
				write_file << chr_buff << endl;
			}
		}
		write_file.close();
	}


	// ************************
	//
	//	������臒l�ȏ�̌������𔻒肷��B�����A�������͂ދ�`���쐬����
	//
	// ************************

//	const double cd_crack_threshold = 5.0;		//	�s�N�Z�����W
	double d_max_crack_length;
	CrackInfo max_crack;
	XYty crack_box[4];							//	��`���͂ޕ��s�l�ӌ`
	XYty minimum_crack_rect[4];					//	���s�l�ӌ`�̍ŏ��O�ڋ�`

	double d_max_x = -9999.9;
	double d_max_y = -9999.9;
	double d_min_x = 9999.9;
	double d_min_y = 9999.9;;

	max_crack.d_crack_length = max_crack.d_slope_coeff = max_crack.xy_pos.dX = max_crack.xy_pos.dY = 0.0;

	for (i_loop = 0; i_loop < (int)vec_all_crack_info.size(); i_loop++)
	{
		d_max_crack_length = -9999.999;
		for (i_loop2 = 0; i_loop2 < (int)vec_all_crack_info[i_loop].size(); i_loop2++)
		{
			//	�����̒��ōő�̒��������߂�
			if (vec_all_crack_info[i_loop][i_loop2].d_crack_length > d_max_crack_length)
			{
				d_max_crack_length = vec_all_crack_info[i_loop][i_loop2].d_crack_length;
				max_crack = vec_all_crack_info[i_loop][i_loop2];
			}
		}
		//	臒l�ȏ�̌����ł���Ό��ׂƂ��A��`���쐬����
		if (d_max_crack_length > ndMinDefectSize)
		{
			//	�������o�t���O�𗧂Ă�
			b_crack_detected = true;

			//	�n�_�A�I�_�A�ő匇�ג����ƂȂ�_�A��3�_���͂��ŏ��O�ڋ�`�����߂�
			crack_box[0] = vec_all_crack_info[i_loop].front().xy_pos;	//	�n�_
			crack_box[1] = vec_all_crack_info[i_loop].back().xy_pos;	//	�I�_

			//MinX
			d_min_x = min(crack_box[0].dX, crack_box[1].dX);
			d_min_x = min(d_min_x, max_crack.xy_pos.dX);

			//MinY
			d_min_y = min(crack_box[0].dY, crack_box[1].dY);
			d_min_y = min(d_min_y, max_crack.xy_pos.dY);

			//MaxX
			d_max_x = max(crack_box[0].dX, crack_box[1].dX);
			d_max_x = max(d_max_x, max_crack.xy_pos.dX);

			//MaxY
			d_max_y = max(crack_box[0].dY, crack_box[1].dY);
			d_max_y = max(d_max_y, max_crack.xy_pos.dY);

			minimum_crack_rect[0].dX = d_min_x;
			minimum_crack_rect[0].dY = d_min_y;
			minimum_crack_rect[1].dX = d_max_x;
			minimum_crack_rect[1].dY = d_min_y;
			minimum_crack_rect[2].dX = d_min_x;
			minimum_crack_rect[2].dY = d_max_y;
			minimum_crack_rect[3].dX = d_max_x;
			minimum_crack_rect[3].dY = d_max_y;

			// ************************
			//
			//	��`���摜�ɕ`�悷��
			//
			// ************************

			MgraColor(m_milGraphic, M_RGB888(255, 0, 0));
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[0].dX, (int)minimum_crack_rect[0].dY, (int)minimum_crack_rect[1].dX, (int)minimum_crack_rect[1].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[1].dX, (int)minimum_crack_rect[1].dY, (int)minimum_crack_rect[3].dX, (int)minimum_crack_rect[3].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[2].dX, (int)minimum_crack_rect[2].dY, (int)minimum_crack_rect[3].dX, (int)minimum_crack_rect[3].dY);
			MgraLine(m_milGraphic, mil_result_image, (int)minimum_crack_rect[0].dX, (int)minimum_crack_rect[0].dY, (int)minimum_crack_rect[2].dX, (int)minimum_crack_rect[2].dY);
		}
	}

	//////////////////////////////////////////
	//	�����܂ł��������o�A���S���Y��
	//////////////////////////////////////////

	//////////////////////////////////////////
	//	��������͊O�ό���(������)
	//////////////////////////////////////////

#if 0

	//	���ω摜�p�̉摜�o�b�t�@���m��
	MbufAlloc2d(m_milSys, (long)sz_image_size.cx, (long)sz_image_size.cy,
		8 + M_UNSIGNED, M_IMAGE + M_PROC, &mil_average_image);

	//	�摜���R�s�[
	MbufCopy(mil_inspection_image, mil_average_image);

	//	�N���[�W���O�ƃI�[�v�j���O�Ń����Y�̉��ꓙ�̃S�~����������B�T�C�Y��10�ɂ��邱�Ƃňȉ��̕��ϋP�x�l�Ƃ̒u����������߂�
	MimClose(mil_average_image, mil_average_image, 10, M_GRAYSCALE);
	MimOpen(mil_average_image, mil_average_image, 10, M_GRAYSCALE);


#if 0
	//	�����Ώۗ̈�̕��ϋP�x�l�����߂�
	MimStat(mil_average_image, mil_stat_result, M_MEAN, M_MASK, mil_blob_image, M_NULL);
	MimGetResult(mil_stat_result, M_MEAN + M_TYPE_DOUBLE, &d_average_value);
	//	�u���u��c������
	MimDilate(mil_blob_image, mil_temp_image, 30, M_GRAYSCALE);
	//	�c���u���u�摜�ƃu���u�摜�̍��������
	MimArith(mil_temp_image, mil_blob_image, mil_diff_image, M_SUB_ABS);
	//	�����c������
	MimDilate(mil_diff_image, mil_diff_image, 20, M_GRAYSCALE);
	//	���̕����𕽋ϋP�x�l�ɒu������
	MimClip(mil_diff_image, mil_temp_image, M_IN_RANGE, 255, 255, d_average_value, M_NULL);
	//	��������̉摜�̖c�������ɒu��������
	MimArith(mil_average_image, mil_diff_image, mil_diff_image, M_SUB);
	MimArith(mil_diff_image, mil_temp_image, mil_average_image, M_MAX);

	//	50�񕽋ς���
	for (i_loop = 0; i_loop < 50; i_loop++)
	{
		MimConvolve(mil_average_image, mil_average_image, M_SMOOTH);
	}
#endif

	//	���������
	MimArith(mil_inspection_image, mil_average_image, mil_diff_image, M_SUB_ABS);

	//	�����摜�̓�l��
	MimBinarize(mil_diff_image, mil_temp_image, M_GREATER_OR_EQUAL, 7, M_NULL);

	//	�I�[�v�j���O�Ŕ����ȍ���������
	MimOpen(mil_temp_image, mil_temp_image, 1, M_GRAYSCALE);

	//	���̉摜�ƁA�����Ώۂ̃u���u�̉摜�ŏd�Ȃ����Ƃ��낾���𒊏o����
	//	�G�b�W�̂Ƃ���͔����Ȃ̂Ńu���u�����k�����āA�G�b�W�t�߂̏��͌��Ȃ��悤�ɂ���
	MimErode(mil_blob_image, mil_diff_image, 10, M_GRAYSCALE);
	MimArith(mil_temp_image, mil_diff_image, mil_temp_image, M_AND);

	//	�������o���ꂽ���ǂ����́A���摜�̉�f�l��255�ƂȂ��f�����邩�ǂ����Ŕ��f����
	MIL_DOUBLE d_count;
	MimStat(mil_temp_image, mil_stat_result, M_NUMBER, M_EQUAL, 255, M_NULL);
	MimGetResult(mil_stat_result, M_NUMBER + M_TYPE_DOUBLE, &d_count);
	if (d_count > 0)
	{
		b_cut_detected = true;
	}
	else
	{
		b_cut_detected = false;
	}

	//	����������摜�ƍ�������
	MimArith(mil_result_image, mil_temp_image, mil_result_image, M_ADD + M_SATURATION);

#endif

	// ************************
	//
	//	�������ʂ̍쐬
	//
	// ************************

	//	�t�@�C���o��
	MbufExport(nstrResultImage.c_str(), M_BMP, mil_result_image);

	//	���������͌��������o���ꂽ��NG��Ԃ�
	if (b_crack_detected == true || b_cut_detected == true)
	{
		i_ret = 1;
	}
	else
	{
		i_ret = 0;
	}

	//	�Ȑ��̗L����Ԃ�
	if (b_curve == true)
	{
		*npiCurve = 1;
	}
	else
	{
		*npiCurve = 0;
	}

	//	���׎�ʂ�Ԃ�
	if (b_cut_detected == true && b_crack_detected == false)
	{
		*npiNGContent = 1;
	}
	else if (b_cut_detected == false && b_crack_detected == true)
	{
		*npiNGContent = 2;
	}
	else if (b_cut_detected == true && b_crack_detected == true)
	{
		*npiNGContent = 3;
	}
	else
	{
		*npiNGContent = 0;
	}


	// ************************
	//
	//	�I������
	//
	// ************************

Finish:
	//	�������J��
	MbufFree(mil_inspection_image);
	MbufFree(mil_inspection_image_binary);
	MbufFree(mil_edge_image);
	MbufFree(mil_blob_image);
	MbufFree(mil_diff_image);
	MbufFree(mil_temp_image);
	MbufFree(mil_result_image);
	MbufFree(mil_average_image);

	MblobFree(mil_blob_result);
	MblobFree(mil_blob_feature_list);

	//	�G�b�W�t�@�C���_�[�R���e�L�X�g�����
	MedgeFree(mil_edge);
	MedgeFree(mil_edge_result);

	MimFree(mil_stat_result);

	return i_ret;
}
