#pragma once

using namespace std;

#define MAX_IMAGE_GRAB_NUM 2 			//	リングバッファ数					2021/03/12 ノートPCではメモリ消費がひどいため、20 -> 2 に変更
#define MAX_AVERAGE_IMAGE_GRAB_NUM 2 	//	画像を平均化するときの最大画像数	2021/03/12 ノートPCではメモリ消費がひどいため、10 -> 2 に変更
#define TRANSPARENT_COLOR	RGB(1,1,1)	//	透過色
#define CLEAR_IMAGE_BUFFER	80
#define FATAL_ERROR_ID	-100			//	致命的エラーの戻り値

//	マクロ
#define MAX(a,b) ( (a) > (b) ? (a) : (b) )
#define MIN(a,b) ( (a) < (b) ? (a) : (b) )

enum PRODUCT_COMPANY
{
	XXX_BOARD = 0,
	MATROX_BOARD
};

//	マトロックス製画像処理ボードの型名
enum MTX_TYPE
{
	MTX_MORPHIS = 0,
	MTX_SOLIOSXCL,
	MTX_SOLIOSXA,
	MTX_METEOR2MC,
	MTX_GIGE,
	MTX_HOST = 100
};

//	ボード情報
typedef struct 
{
	int	iBoardProductCompany;
	int iBoardType;

} BoardParameter;

//	カラータイプ
enum RGB_TYPE
{
	RGB = 0,
	R,
	G,
	B
};
//	HSI色空間
enum HSI_TYPE
{
	H= 0,
	S,
	I
};

//	グラフィックカラー
enum GRAPHICCOLOR_TYPE
{
	GRAPHIC_COLOR_BLACK= 0,
	GRAPHIC_COLOR_WHITE,
	GRAPHIC_COLOR_RED,
	GRAPHIC_COLOR_GREEN, 
	GRAPHIC_COLOR_BLUE,
	GRAPHIC_COLOR_YELLOW
};

//	パターンマッチング結果情報
typedef struct 
{
	double	d_pos_x;
	double	d_pos_y;
	double	d_score;
} PatternMatchingResult;

//	ブロブエッジ情報(離型剤による黒光り除去に使う構造体)
typedef struct 
{
	POINT pt_edge_point;
	int   i_blob_length;
} BlobEdgeInfo;

//	XYデータ
typedef struct 
{
	double dX;
	double dY;
} XYty;

//	検査結果(NG情報)構造体
typedef struct 
{
	RECT rct_NG_region;
	int i_fake_NG;	//	0:通常欠陥　1:擬似欠陥

} InspectionResultty;

typedef struct 
{
	double	d_diff_error;
	int		index;
	XYty	xy_shift_target_reference;	//	基準と検査の位置ずれ量(基準-検査)
}DiffErrorTy;

typedef struct 
{
	double  d_edge_pixel_num_rate;
	POINT pt_position;
}EdgePixelNumty;

//	パターンマッチングパラメーター構造体
typedef struct
{
	int i_max_pattern_find_num;			//	最大パターン検出数
	int i_find_parameter_first_level;	//	検出パラメーターのfirst level
	int i_find_parameter_last_level;	//	検出パラメーターのlast  level
	double d_certainty_val;				//	CertainTy値
	double d_acceptance_val;			//	Accept値

}PatternMatchingParametersTy;

//	輪郭抽出パラメーター構造体
typedef struct
{
	int i_filter_smoothness;
	int i_second_filter_smoothness;
	int	i_threshold_hi;
	int i_threshold_low;
	int	i_second_threshold_hi;
	int i_second_threshold_low;

}EdgeDetectParameterTy;


typedef struct
{
	int i_start_index;
	int i_end_index;
	double d_average_angle;
	int i_line_ID;

}LineInfo;

typedef struct
{
	XYty xy_pos;
	double d_crack_length;
	double d_slope_coeff;

}CrackInfo;