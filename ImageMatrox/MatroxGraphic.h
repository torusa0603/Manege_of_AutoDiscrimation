#pragma once
#include "matroxcommon.h"
#include <vector>

class CMatroxGraphic :public CMatroxCommon
{
public:
	CMatroxGraphic(void);
	~CMatroxGraphic(void);

	//	色を設定する
	void setGraphicColor( int niColorType );
	//	グラフィックを全て消去する関数
	void clearAllGraphic();
	//	線を描画する
	void DrawLine( POINT nptStartPoint, POINT nptEndPoint );
	//	矩形を描画する
	void DrawRectangle( POINT nptLeftTopPoint, POINT nptRightBottomPoint );
	//	平行四辺形を描画する
	void DrawParallelogram( POINT nptFirstPoint, POINT nptSecondPoint, POINT nptThirdPoint );
	//	描画先の変更を行う
	void changeDrawDestination( int niType );


private:
	int m_iTargetDrawType;		//	0:通常のオーバーレイバッファへの描画	1:検査結果画像オーバーレイバッファへの描画  2:グラフィック画像保存用のバッファへの描画
	MIL_ID	m_milLocalOverlay;	//	描画先の画像バッファ

};
