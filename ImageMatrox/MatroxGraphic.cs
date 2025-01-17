﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;

namespace ImageMatrox
{
    class CMatroxGraphic: CMatroxCommon
    {
        int m_iTargetDrawType;      //	0:通常のオーバーレイバッファへの描画	1:検査結果画像オーバーレイバッファへの描画  2:グラフィック画像保存用のバッファへの描画
        MIL_ID m_milLocalOverlay;	//	描画先の画像バッファ

        public CMatroxGraphic()
        {
        	m_iTargetDrawType = 0;
        	m_milLocalOverlay = MIL.M_NULL;
        }

        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		グラフィックカラーを設定する関数
        
        	2.パラメタ説明
        		(IN)  niColorType	色タイプ
        
        	3.概要
        		グラフィックカラーを設定する
        
        	4.機能説明
        		グラフィックカラーを設定する
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void setGraphicColor(int niColorType)
        {
            int l_red = 0;
            int l_green = 0;
            int l_blue = 0;
        
            if (m_bMainInitialFinished == false)
            {
                return;
            }
        
            switch (niColorType)
            {
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_BLACK:
                    l_red = 0;
                    l_green = 0;
                    l_blue = 0;
                    break;
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_WHITE:
                    l_red = 255;
                    l_green = 255;
                    l_blue = 255;
                    break;
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_RED:
                    l_red = 255;
                    l_green = 0;
                    l_blue = 0;
                    break;
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_GREEN:
                    l_red = 0;
                    l_green = 255;
                    l_blue = 0;
                    break;
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_BLUE:
                    l_red = 0;
                    l_green = 0;
                    l_blue = 255;
                    break;
                case (int)GRAPHICCOLOR_TYPE.GRAPHIC_COLOR_YELLOW:
                    l_red = 255;
                    l_green = 255;
                    l_blue = 0;
                    break;
            }
            MIL.MgraColor(m_milGraphic, MIL.M_RGB888(l_red, l_green, l_blue));
        
        }
        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		指定のディスプレイのグラフィックを消去する関数
        
        	2.パラメタ説明
        		(IN)  nmilOverlay		: オーバーレイバッファ
        
        	3.概要
        		グラフィックを消去する
        
        	4.機能説明
        		グラフィックを消去する
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void clearAllGraphic()
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }
        
            //	グラフィックをクリアする
            if (m_iTargetDrawType == 0)
            {
                MIL.MdispControl(m_milDisp, MIL.M_OVERLAY_CLEAR, MIL.M_TRANSPARENT_COLOR);
            }
            else if (m_iTargetDrawType == 1)
            {
                MIL.MdispControl(m_milDispForInspectionResult, MIL.M_OVERLAY_CLEAR, MIL.M_TRANSPARENT_COLOR);
            }
            else
            {
                MIL.MbufClear(m_milDraphicSaveImage, MIL.M_RGB888(1, 1, 1));
            }
        }
        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		オーバーレイバッファに線を描画する関数
        
        	2.パラメタ説明
        		(IN)  npptStartPoint	: 線の始点座標
        		(IN)  npptEndPoint		: 線の終点座標
        		(IN)  nmilOverlay		: オーバーレイバッファ
        		
        	3.概要
        		オーバーレイバッファに線を描画する
        
        	4.機能説明
        		オーバーレイバッファに線を描画する
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void DrawLine(POINT nptStartPoint, POINT nptEndPoint)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }
        
            MIL.MgraLine(m_milGraphic, m_milLocalOverlay, nptStartPoint.x, nptStartPoint.y,
                          nptEndPoint.x, nptEndPoint.y);
        }

        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		オーバーレイバッファに矩形を描画する関数
        
        	2.パラメタ説明
        		(IN)  npptStartPoint	: 線の始点座標
        		(IN)  npptEndPoint		: 線の終点座標
        		(IN)  nmilOverlay		: オーバーレイバッファ
        		
        	3.概要
        		オーバーレイバッファに矩形を描画する
        
        	4.機能説明
        		オーバーレイバッファに矩形を描画する
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void DrawRectangle(POINT nptLeftTopPoint, POINT nptRightBottomPoint)
        {
            if (m_bMainInitialFinished == false)
            {
                return;
            }
        
            clearAllGraphic();
        
            //	実際には引数の二点が左上、右下となっていないことがあるので、ここで左上、右下にする
            int i_max_x = -65325;
            int i_max_y = -65325;
            int i_min_x = 65325;
            int i_min_y = 65325;
        
            if (nptLeftTopPoint.x > i_max_x)
            {
                i_max_x = nptLeftTopPoint.x;
            }
            if (nptRightBottomPoint.x > i_max_x)
            {
                i_max_x = nptRightBottomPoint.x;
            }
        
            if (nptLeftTopPoint.x < i_min_x)
            {
                i_min_x = nptLeftTopPoint.x;
            }
            if (nptRightBottomPoint.x < i_min_x)
            {
                i_min_x = nptRightBottomPoint.x;
            }
        
            if (nptLeftTopPoint.y > i_max_y)
            {
                i_max_y = nptLeftTopPoint.y;
            }
            if (nptRightBottomPoint.y > i_max_y)
            {
                i_max_y = nptRightBottomPoint.y;
            }
        
            if (nptLeftTopPoint.y < i_min_y)
            {
                i_min_y = nptLeftTopPoint.y;
            }
            if (nptRightBottomPoint.y < i_min_y)
            {
                i_min_y = nptRightBottomPoint.y;
            }
        
            nptLeftTopPoint.x = i_min_x;
            nptLeftTopPoint.y = i_min_y;
            nptRightBottomPoint.x = i_max_x;
            nptRightBottomPoint.y = i_max_y;
        
                    MIL.MgraRect(m_milGraphic, m_milLocalOverlay,
                      nptLeftTopPoint.x, nptLeftTopPoint.y, nptRightBottomPoint.x, nptRightBottomPoint.y);
        
            nptLeftTopPoint.x = i_min_x + 1;
            nptLeftTopPoint.y = i_min_y + 1;
            nptRightBottomPoint.x = i_max_x - 1;
            nptRightBottomPoint.y = i_max_y - 1;
        
                    MIL.MgraRect(m_milGraphic, m_milLocalOverlay,
                      nptLeftTopPoint.x, nptLeftTopPoint.y, nptRightBottomPoint.x, nptRightBottomPoint.y);
        }

        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		オーバーレイバッファに平行四辺形を描画する関数
        
        	2.パラメタ説明
        		(IN)  npptStartPoint	: 線の始点座標
        		(IN)  npptEndPoint		: 線の終点座標
        		(IN)  nmilOverlay		: オーバーレイバッファ
        		
        	3.概要
        		オーバーレイバッファに平行四辺形を描画する
        
        	4.機能説明
        		オーバーレイバッファに平行四辺形を描画する
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void DrawParallelogram(POINT nptFirstPoint, POINT nptSecondPoint, POINT nptThirdPoint)
        {
            POINT pt_fourth_point;          //　平行四辺形の第４点
        
            //　４番目の点を計算する
            pt_fourth_point.x = nptThirdPoint.x - nptSecondPoint.x + nptFirstPoint.x;
            pt_fourth_point.y = nptThirdPoint.y - nptSecondPoint.y + nptFirstPoint.y;
        
            //	平行四辺形は4本の直線から求める
            DrawLine(nptFirstPoint, nptSecondPoint);        //	1→2
            DrawLine(nptSecondPoint, nptThirdPoint);        //	2→3
            DrawLine(nptThirdPoint, pt_fourth_point);   //	3→4
            DrawLine(pt_fourth_point, nptFirstPoint);	//	4→1
        }

        /*------------------------------------------------------------------------------------------
        	1.日本語名
        		描画先の変更を行う
        
        	2.パラメタ説明
        		
        	3.概要
        		描画先の変更を行う
        
        	4.機能説明
        		描画先の変更を行う
        
        	5.戻り値
        		なし
        
        	6.備考
        		なし
        ------------------------------------------------------------------------------------------*/
        public void changeDrawDestination(int niType)
        {
            if (niType == 0)
            {
                m_iTargetDrawType = 0;
                m_milLocalOverlay = m_milOverLay;
            }
            else if (niType == 1)
            {
                m_iTargetDrawType = 1;
                m_milLocalOverlay = m_milOverLayForInspectionResult;
            }
            else
            {
                m_iTargetDrawType = 2;
                m_milLocalOverlay = m_milDraphicSaveImage;
            }
        }
    }
}
