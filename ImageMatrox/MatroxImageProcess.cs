using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matrox.MatroxImagingLibrary;
using System.Drawing;
using System.Threading;

namespace ImageMatrox
{
    class CMatroxImageProcess : CMatroxCommon
    {
        public void init()
        {

        }

        public void close()
        {

        }

        /// <summary>
        /// 差分画像を作成しその画像の輝度値の平均が指定の値よりも大きいかを判断する
        /// </summary>
        /// <param name="n_discriminantValue">差分が大きいと判断するスコア値</param>
        /// <returns></returns>
        public int discriminantDiffImage(int n_discriminantValue)
        {
            Size sz_image_size = new Size(0, 0);
            int i_ret = -1;

            if (m_bMainInitialFinished == false)
            {
                return -100;
            }
            //画差分モードに変更
            setDiffMode(2);
            // 画像を確実に取得させるためのスリープ処理
            Thread.Sleep(100);
            //	画像サイズ取得
            sz_image_size.Width = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_X, MIL.M_NULL);
            sz_image_size.Height = (int)MIL.MbufInquire(m_milShowImage, MIL.M_SIZE_Y, MIL.M_NULL);
            RECT rc_show_image = new RECT(0, 0, sz_image_size.Width, sz_image_size.Height);

            //　差分画像の平均輝度値を取得
            int i_pixel_value = getAveragePixelValueOnRegion(rc_show_image);
            // 差分モードを解除
            resetDiffMode();
            if (n_discriminantValue < i_pixel_value)
            {
                //string cs_file_path = "D: \\image" + "\\" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_milOriImage" + ".bmp"; ;
                //RECT rect = new RECT(0, 0, 0, 0);
                //saveImage(rect, true, cs_file_path, false);
                i_ret = 0;
            }
            else
            {
                i_ret = -1;
            }

            return i_ret;
        }
    }
}
