using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;


namespace Manege_of_AutoDiscrimation
{

	//public class ImageOverlay
	//{
	//	public bool OverlayMask(string photofile, string maskfile, string outfile)
	//	{
	//		try
	//		{
	//			Bitmap photo = new Bitmap(photofile);
	//			Bitmap mask = new Bitmap(maskfile);
	//			Bitmap output = this.OverlayMask(photo, mask);
	//			if (output == null)
	//			{
	//				return false;
	//			}
	//			output.Save(outfile);
	//			return true;
	//		}
	//		catch
	//		{
	//			Console.WriteLine("can't make overlay file");
	//			return false;
	//		}
	//	}
	//	public Bitmap OverlayMask(Bitmap bmpPhoto, Bitmap bmpMask)
	//	{
	//		int width = bmpPhoto.Width;
	//		int height = bmpPhoto.Height;
	//		Bitmap bmpOutput = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

	//		for (int y = 0; y < width; y++)
	//		{
	//			for (int x = 0; x < height; x++)
	//			{
	//				Color photo = bmpPhoto.GetPixel(x, y);
	//				byte red = bmpMask.GetPixel(x, y).R;
	//				byte alpha = (byte)(255 - red);
	//				Color mask = Color.FromArgb(alpha, photo);
	//				bmpOutput.SetPixel(x, y, mask);
	//			}
	//		}
	//		return bmpOutput;
	//	}
	//}

	//public class cMaskPicture
	//{
	//	public void CutCircle(string npicturefilepath, string nsavefilepath)
	//	{
	//		//元画像読み込み
	//		Bitmap srcBmp = (Bitmap)Bitmap.FromFile(npicturefilepath);

	//		//元画像を32bppに変換
	//		Bitmap tmpBmp = new Bitmap(srcBmp.Width, srcBmp.Height, PixelFormat.Format32bppArgb);
	//		Graphics tmpG = Graphics.FromImage(tmpBmp);
	//		tmpG.DrawImage(srcBmp, new Rectangle(0, 0, tmpBmp.Width, tmpBmp.Height),
	//			new Rectangle(0, 0, srcBmp.Width, srcBmp.Height), GraphicsUnit.Pixel);

	//		////切り抜き型画像作成
	//		Rectangle rect1 = new Rectangle(0, 0, tmpBmp.Width, tmpBmp.Height);

	//		Bitmap origin = new Bitmap(srcBmp.Width, srcBmp.Height);
	//		Graphics tg = Graphics.FromImage(origin);
	//		tg.FillRectangle(Brushes.White, rect1);
	//		tg.FillPie(Brushes.Black, rect1, 0, 360);


	//		//マスク処理
	//		Mask(tmpBmp, origin, true);

	//		//保存
	//		tmpBmp.Save(nsavefilepath, ImageFormat.Png);
	//	}

	//	public unsafe static void Mask(Bitmap srcBmp, Bitmap mskBmp, bool isNegate)
	//	{
	//		int w = srcBmp.Width;
	//		int h = srcBmp.Height;

	//		if (mskBmp.Width != w || mskBmp.Height != h)
	//		{
	//			throw new ArgumentException("bitmap size unmatch");
	//		}
	//		if (srcBmp.PixelFormat != PixelFormat.Format32bppArgb ||
	//			mskBmp.PixelFormat != PixelFormat.Format32bppArgb)
	//		{
	//			throw new ArgumentException("bitmap must be 32bpp");
	//		}

	//		//ビットマップをロックしてポインタを取得
	//		BitmapData srcData = srcBmp.LockBits(new Rectangle(0, 0, w, h),
	//			ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
	//		BitmapData mskData = mskBmp.LockBits(new Rectangle(0, 0, w, h),
	//			ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
	//		byte* pSrcBase = (byte*)srcData.Scan0.ToPointer();
	//		byte* pMskBase = (byte*)mskData.Scan0.ToPointer();

	//		//ピクセル毎にマスク処理
	//		for (int y = 0; y < h; y++)
	//		{
	//			for (int x = 0; x < w; x++)
	//			{
	//				byte* pSrc = pSrcBase + (y * w + x) * 4;
	//				byte* pMsk = pMskBase + (y * w + x) * 4;

	//				//mask RGB-> src A  罠：Argbといいつつ格納の順番は逆のBGRA
	//				int rgbsum = (*(pMsk + 0) + *(pMsk + 1) + *(pMsk + 2)) / 3;
	//				if (isNegate)
	//				{
	//					rgbsum = 255 - rgbsum;
	//				}
	//				*(pSrc + 3) = (byte)rgbsum;
	//			}
	//		}

	//		//アンロック
	//		srcBmp.UnlockBits(srcData);
	//		mskBmp.UnlockBits(mskData);
	//	}
	//}

	//class MainProcess
	//{
	//	CParaFormMain m_cParaMainProcess = CParaFormMain.getInstance();
	//	private ImageOverlay m_img = new ImageOverlay();
	//	private cMaskPicture m_mask = new cMaskPicture();
	//	private LogBase.CLogBase m_cLogMainProcess = new LogBase.CLogBase(LogKind.MainProcess); // 実行ログ

	//	private Mutex m_mtxPicFiles = new Mutex();                      // 画像ファイルリストアクセス用
	//	private List<string> m_lstPicFiles = new List<string>();        // 画像ファイルリスト

	//	// 表示画像分割処理
	//	private List<string> m_lstDivPicFiles = new List<string>();     // 分割画像ファイルリスト
	//	private List<Rectangle> m_TargetDiv = new List<Rectangle>();    // 分割画像ファイル座標
	//	private List<string> m_TargetPic = new List<string>();          // 分割画像ファイルリスト
	//	private List<clrData> m_TargetData = new List<clrData>();
	//	private int[] m_TargetOrder;

	//	private List<clrColorRGB> m_ResultRGB = new List<clrColorRGB>();
	//	//private List<clrColorRGB> m_StandardRGB = new List<clrColorRGB>();
	//	//private int m_StandardNo;
	//	//private int[] m_StandardCalc;
	//	private double m_HueMax;
	//	private double m_HueMin;
	//	private double m_SaturationMax;
	//	private double m_SaturationMin;
	//	private double m_LightnessMax;
	//	private double m_LightnessMin;
	//	private int m_OrderNo;

	//	private Rectangle imgRect;
	//	private Rectangle dstRect;

	//	// チャート処理
	//	private string[] mColorName;                                    // 色名称
	//	private double[] mColorValue;                                   // 色割合(%)


 //       #region "   初期化処理                                        "
 //       public MainProcess()
	//	{
	//		try
	//		{
	//			// Paramファイル読み込み
	//			string str_log;
	//			m_cParaMainProcess.readXmlFile(out str_log);

	//			// ログ初期化
	//			string str_temp = LogKind.MainProcess;
	//			m_cLogMainProcess.Enable = m_cParaMainProcess.EnableLogExecute;      // ログを残すか
	//			m_cLogMainProcess.FolderName = "log\\" + str_temp;                   // フォルダ名
	//			m_cLogMainProcess.FileName = str_temp;                               // ファイル名

	//			// チャート初期化
	//			InitChartInfomation();

	//		}
	//		catch (Exception e)
	//		{
	//			MessageBox.Show(e.Message);
	//			return;
	//		}
	//	}
 //       #endregion

 //       #region "   画像ファイル名登録                                "
 //       /// <summary>
 //       /// 画像ファイル名をリストへ登録
 //       /// </summary>
 //       /// <param name="nstrFileName">画像ファイル名</param>
 //       public void set_picture_file(string nstrFileName)
	//	{
	//		m_mtxPicFiles.WaitOne();
	//		try
	//		{
	//			m_lstPicFiles.Add(nstrFileName);
	//		}
	//		catch (System.Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//		}
	//		finally
	//		{
	//			m_mtxPicFiles.ReleaseMutex();
	//		}
	//	}
	//	#endregion

	//	#region "   最新の画像ファイル名取得                          "
	//	/// <summary>
	//	/// 最新の画像ファイル名をリストより取得
	//	/// </summary>
	//	/// <returns>最新の画像ファイル名</returns>
	//	/// <remarks>2枚の画像がある時、画像を取得するので実際はひとつ古い画像</returns>
	//	public string get_picture_file()
	//	{
	//		m_mtxPicFiles.WaitOne();

	//		string str_ret = "";
	//		try
	//		{
	//			// ソートすれば古い順に並ぶはず(念の為)
	//			m_lstPicFiles.Sort();
	//			// 最新以外は削除
	//			while (2 < m_lstPicFiles.Count())
	//			{
	//				string str_log = "m_lstPicFiles.Count = " + m_lstPicFiles.Count().ToString() + " " + m_lstPicFiles[0];
	//				System.Diagnostics.Debug.WriteLine(str_log);
	//				m_lstPicFiles.RemoveAt(0);
	//			}
	//			// 最新取得
	//			if (1 <= m_lstPicFiles.Count())
	//			{
	//				str_ret = m_lstPicFiles[0];
	//				m_lstPicFiles.RemoveAt(0);
	//			}
	//		}
	//		catch (System.Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//		}
	//		finally
	//		{
	//			m_mtxPicFiles.ReleaseMutex();
	//		}
	//		return str_ret;
	//	}
	//	#endregion

	//	#region "   画像ファイル有無取得                              "
	//	/// <summary>
	//	/// 画像ファイルの有無取得(2ファイル以上で有とする)
	//	/// </summary>
	//	/// <returns>画像ファイルの有無取得</returns>
	//	public bool is_picture_file()
	//	{
	//		m_mtxPicFiles.WaitOne();

	//		bool b_ret = false;
	//		try
	//		{
	//			// 2ファイル以上?
	//			if (1 < m_lstPicFiles.Count())
	//			{
	//				b_ret = true;
	//			}
	//		}
	//		catch (System.Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//		}
	//		finally
	//		{
	//			m_mtxPicFiles.ReleaseMutex();
	//		}
	//		return b_ret;
	//	}
	//	#endregion



	//	#region "   分割処理変数の初期化                              "
	//	public void getTargetPosition()
	//	{
	//		m_TargetDiv.Clear();

	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget1);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget2);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget3);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget4);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget5);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget6);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget7);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget8);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget9);
	//		m_TargetDiv.Add(m_cParaMainProcess.GraphicTarget10);
	//	}
 //       #endregion

 //       #region "   分割処理時の並び替え要素設定                      "
	//	public void setTargetOrder(int nOrderNo)
 //       {
	//		m_OrderNo = nOrderNo;

	//		if(m_OrderNo > 3)
 //           {
	//			m_OrderNo = 2;
 //           }
 //       }
 //       #endregion

 //       #region "   分割処理結果情報取得処理                          "
 //       /// <summary>
 //       /// 分割処理結果情報取得処理
 //       /// </summary>
 //       /// <param name="data_order">順番</param>
 //       /// <param name="data_rect">表示情報</param>
 //       /// <param name="data_result">測定結果情報</param>
 //       /// <param name="data_resultRGB">RGB結果情報</param>
 //       public void GetResultDataOrder(ref int[] data_order, ref Rectangle[] data_rect, ref clrData[] data_result, ref clrColorRGB[] data_resultRGB)
	//	{
	//		data_order = m_TargetOrder;
	//		data_rect = m_TargetDiv.ToArray();
	//		data_result = m_TargetData.ToArray();
	//		data_resultRGB = m_ResultRGB.ToArray();
	//	}
	//	public void GetTargetPoints(ref  Rectangle[] data_rect)
	//	{
	//		data_rect = m_TargetDiv.ToArray();
	//	}

	//	#endregion

	//	#region "   分割処理結果順番取得処理                          "
	//	/// <summary>
	//	/// 分割処理結果順番取得処理
	//	/// </summary>
	//	/// <param name="data_order">順番</param>
	//	/// <param name="data_result">測定結果情報</param>
	//	public void GetTargetOrder(ref int[] target_order, ref clrData[] data_result)
	//	{
	//		target_order = m_TargetOrder.ToArray();
	//		data_result = m_TargetData.ToArray();
	//	}

	//	#endregion

	//	#region "   分割処理 / 取り込んだ画像を分割して順番を決定する "
	//	public void setDividedPicture(string nImgFileName)
	//	{
	//		try
	//		{
	//			if (m_cParaMainProcess.DebugCameraFlag != true)
	//			{
	//				nImgFileName = get_picture_file();
	//			}

	//			if (nImgFileName == "")
	//			{
	//				return;
	//			}
	//			else
	//			{
	//				// 初期化
	//				m_TargetPic.Clear();
	//				m_TargetData.Clear();
	//				int intLoopEnd = int.Parse(m_cParaMainProcess.GraphicTargetNumber);
	//				m_TargetOrder = new int[intLoopEnd];
	//				m_ResultRGB.Clear();

	//				string strLog = "order[" + m_OrderNo + "] process strat. ...........";
	//				m_cLogMainProcess.outputLog(strLog);

	//				// 切り出し実行
	//				for (int intLoop = 1; intLoop <= intLoopEnd; intLoop++)
	//				{
	//					Console.WriteLine("切り出しNo.{0}", intLoop);
	//					getDividedImage(nImgFileName, intLoop - 1);
	//				}

	//				if (m_TargetData.Count > 0)
	//				{
	//					// 色相の濃い順に並べる
	//					if(m_OrderNo == 1)
	//						m_TargetData.Sort(clrData.CompareHueAsc);
	//					else if(m_OrderNo == 2)
	//						m_TargetData.Sort(clrData.CompareSaDesc);
	//					else if (m_OrderNo == 3)
	//						m_TargetData.Sort(clrData.CompareLigDesc);
	//					else
	//						m_TargetData.Sort(clrData.CompareHueMedianAsc);

	//					for (int intLoop = 1; intLoop <= intLoopEnd; intLoop++)
 //                       {
	//						m_TargetOrder[intLoop-1] = m_TargetData[intLoop-1].ID;
	//					}
	//					// 並び順をもとに戻しておく
	//					m_TargetData.Sort(clrData.CompareIDAsc);
	//				}
	//			}
	//		}
	//		catch (System.Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//		}
	//	}
	//	#endregion

	//	#region "   分割処理 / 画像切り出し処理                       "
	//	/// <summary>
	//	/// Bitmapの一部を切り出したBitmapオブジェクトを返す
	//	/// </summary>
	//	/// <param name="nSrc">元の画像ファイルパス</param>
	//	/// <param name="nNo">切り出しNo</param>
	//	/// <returns></returns>
	//	private void getDividedImage(string nSrc, int nNo)
	//	{
	//		try
	//		{
	//			//画像ファイルのImageオブジェクトを作成する
	//			using (var bmp = new Bitmap(nSrc))
	//			{
	//				//画像の一部を切り取って（トリミングして）表示する

	//				//描画先とするImageオブジェクトを作成する
	//				using (Bitmap bmpDV = new Bitmap(m_TargetDiv[nNo].Width, m_TargetDiv[nNo].Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb)) // bmp.PixelFormat))
	//				{
	//					//ImageオブジェクトのGraphicsオブジェクトを作成する
	//					using (Graphics g = Graphics.FromImage(bmpDV))
	//					{
	//						//切り取る部分の範囲を決定する。
	//						imgRect = new Rectangle(m_TargetDiv[nNo].X, m_TargetDiv[nNo].Y, m_TargetDiv[nNo].Width, m_TargetDiv[nNo].Height);
	//						//描画する部分の範囲を決定する。
	//						dstRect = new Rectangle(0, 0, imgRect.Width, imgRect.Height);
	//						//画像の一部を描画する
	//						g.DrawImage(bmp, dstRect, imgRect, GraphicsUnit.Pixel);

	//						string sTargetTempName = "\\TargetDivTemp" + (nNo + 1).ToString() + ".bmp";
	//						string sTargetDivFileName = "\\TargetDiv" + (nNo + 1).ToString() + ".bmp";
	//						string sTargetTempPath = m_cParaMainProcess.GraphicTargetSavePath + sTargetTempName;
	//						string sTargetDvFilePath = m_cParaMainProcess.GraphicTargetSavePath + sTargetDivFileName;
	//						//画像をファイルに保存する
	//						//bmpDV.Save(sTargetDvFilePath, System.Drawing.Imaging.ImageFormat.Bmp);
	//						bmpDV.Save(sTargetTempPath, System.Drawing.Imaging.ImageFormat.Bmp);
							
	//						//マスク用画像を使用して〇に切り抜く
	//						//string sTargetMaskName = "\\TargetMask.bmp";
	//						//string sTargetMaskPath = m_cParaMainProcess.GraphicTargetSavePath + sTargetMaskName;
	//						//m_img.OverlayMask(sTargetTempPath, sTargetMaskPath, sTargetDvFilePath);

	//						//比良プロセスを使用して□画像を自動で〇に切り抜く
	//						m_mask.CutCircle(sTargetTempPath, sTargetDvFilePath);

	//						// 分割画像ファイルリストに追加する
	//						m_TargetPic.Add(sTargetDvFilePath);

	//						// 同時にHSL値を取得する
	//						getHslFromImage(bmpDV, nNo, 0);

	//						//Graphicsオブジェクトのリソースを解放する
	//						g.Dispose();

	//					}

	//					bmpDV.Dispose();
	//				}

	//				bmp.Dispose();
	//			}

	//			return;
	//		}
	//		catch (Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//			return;
	//		}

	//	}

	//	#endregion

	//	#region "   分割処理 / HSL値を取得する                        "
	//	/// <summary>
	//	/// LockBits、UnlockBitsで画像データのポインタを取得し、配列を介して処理を行う
	//	/// </summary>
	//	/// <param name="bmp"></param>
	//	/// <param name="nNo"></param>
	//	/// <param name="nRGBOnlyFlag">RGBのみで判定する(1)か否か(0)</param>
	//	private void getHslFromImage(Bitmap bmp, int nNo, int nRGBOnlyFlag)
	//	{
	//		try
	//		{
	//			int pixcelCount;
	//			int lineIndex;
	//			double hue;
	//			double saturation;
	//			double lightness;
	//			double addH = 0;
	//			double addS = 0;
	//			double addL = 0;
	//			double aveH = 0;
	//			double aveS = 0;
	//			double aveL = 0;
	//			double medH = 0;
	//			List<double> hList = new List<double>();
	//			Color col;
	//			int medR = 0;
	//			int medG = 0;
	//			int medB = 0;
	//			List<int> rList = new List<int>();
	//			List<int> gList = new List<int>();
	//			List<int> bList = new List<int>();
	//			string strLog = "";

	//			var width = bmp.Width;
	//			var height = bmp.Height;

	//			// Bitmapをロック
	//			var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

	//			// メモリの幅のバイト数を取得
	//			var stride = Math.Abs(bmpData.Stride);

	//			// 画像データ格納用配列
	//			var data = new byte[stride * bmpData.Height];

	//			// Bitmapデータを配列へコピー
	//			System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, stride * bmpData.Height);

	//			byte r, g, b;
	//			int intGrayFlag;

 //               pixcelCount = 0;
 //               lineIndex = 0;

	//			// 最初にRGB値からHue値範囲を求める					
	//			for (int y = 0; y < height; y++)
 //               {
 //                   for (int x = 0; x < width * 3; x += 3)
	//				{
	//					// RGB値の取得
	//					r = data[lineIndex + x + 2];
 //                       g = data[lineIndex + x + 1];
 //                       b = data[lineIndex + x];
	//					intGrayFlag = checkGrayRGB(r, g, b);
	//					if (intGrayFlag == 0)
	//					{
	//						// 白黒灰以外を対象に輝度値を取得する
	//						rList.Add(r);
	//						gList.Add(g);
	//						bList.Add(b);
	//					}
	//				}
 //                   lineIndex += stride;
 //               }

	//			if(rList.Count < 1)
 //               {
	//				strLog = "No." + nNo + " is no data (RGB).";
	//				m_cLogMainProcess.outputLog(strLog);
	//				m_TargetData.Add(new clrData(nNo, 0, 0, 0, 0, 0));
	//				setResultRGB(nNo, medR, medG, medB);

	//				return;
	//			}

	//			// Hue取得範囲を決定するとき、RGBのメディアン値を使用する
	//			medR = rList.Median();
	//			medG = gList.Median();
	//			medB = bList.Median();
	//			col = Color.FromArgb(medR, medG, medB);
	//			hue = 0;
	//			saturation = 0;
	//			lightness = 0;
	//			getRgbToHsl(col, ref hue, ref saturation, ref lightness);

	//			strLog = "No." + nNo + " median RGB : hue = " + hue.ToString("0.00000") + "( 0 ) saturation = " + saturation.ToString("0.00000") + " lightness = " + lightness.ToString("0.00000");
	//			m_cLogMainProcess.outputLog(strLog);

	//			if (nRGBOnlyFlag == 1)
 //               {
	//				// RGBのみで判定する場合はここで終了
	//				strLog = "No." + nNo + " is only RGB judge.";
	//				m_cLogMainProcess.outputLog(strLog);
	//				Console.WriteLine(strLog);

	//				// 分割画像の各値にデータをセットする
	//				m_TargetData.Add(new clrData(nNo, rList.Count, hue, saturation, lightness, hue));
	//				setResultRGB(nNo, medR, medG, medB);										
	//			}
	//			else
	//			{
	//				double dLimitMax = 0;
	//				double dLimitMin = 0;
	//				double dDataValue = 0;

	//				// Hue取得範囲を決定する
	//				setHslRange(hue, saturation, lightness);

	//				if (m_OrderNo == 2)
	//				{
	//					// 彩度
	//					dLimitMax = m_SaturationMax;
	//					dLimitMin = m_SaturationMin;
	//				}
	//				else if (m_OrderNo == 3)
	//				{
	//					// 明度
	//					dLimitMax = m_LightnessMax;
	//					dLimitMin = m_LightnessMin;
	//				}
	//				else
	//				{
	//					// 色相
	//					dLimitMax = m_HueMax;
	//					dLimitMin = m_HueMin;
	//				}


	//				rList.Clear();
	//				gList.Clear();
	//				bList.Clear();
	//				pixcelCount = 0;
	//				lineIndex = 0;
	//				//string strLog;
	//				//int intDiff;

	//				for (int y = 0; y < height; y++)
	//				{
	//					for (int x = 0; x < width * 3; x += 3)
	//					{
	//						// RGB値の取得
	//						r = data[lineIndex + x + 2];
	//						g = data[lineIndex + x + 1];
	//						b = data[lineIndex + x];

	//						//strLog = ",No." + Convert.ToString(nNo + 1) + "," + x.ToString() + "," + y.ToString() + "," + r.ToString() + "," + g.ToString() + "," + b.ToString();

	//						//intDiff = checkDifferenceRGB(r, g, b);
	//						intGrayFlag = checkGrayRGB(r, g, b);
	//						if (intGrayFlag == 0)
	//						{
	//							// 白黒灰以外を対象に輝度値を取得する
	//							col = Color.FromArgb(r, g, b);
	//							hue = 0;
	//							saturation = 0;
	//							lightness = 0;
	//							getRgbToHsl(col, ref hue, ref saturation, ref lightness);

	//							// Redのhue値は-330°～30°となるため、ここでは-値としておく。
	//							if (hue > 330)
	//								hue = hue - 360;

	//							#region "   TEST   "
	//							//if (pixcelCount == 0)
	//							//	Console.WriteLine("             : hue=" + hue + " saturation=" + saturation + " lightness=" + lightness);
	//							//if (pixcelCount == 0)
	//							//	Console.WriteLine("             : hue=" + col.GetHue() + " saturation=" + col.GetSaturation() + " lightness=" + col.GetBrightness());

	//							//strLog = "No." + Convert.ToString(nNo + 1) + ", diff=" + intDiff.ToString() + ",HSL, x[" + x.ToString() + "], y[" + y.ToString() + "], H=" + hue.ToString("0.000") + ", S=" + saturation.ToString("0.000") + ", L=" + lightness.ToString("0.000");
	//							//strLog = strLog + ",,"  + hue.ToString("0.000") + "," + saturation.ToString("0.000") + "," + lightness.ToString("0.000");
	//							//m_cLogMainProcess.outputLog(strLog);
	//							#endregion

	//							if (m_OrderNo == 2)
	//							{
	//								// 彩度
	//								dDataValue = saturation;
	//							}
	//							else if (m_OrderNo == 3)
	//							{
	//								// 明度
	//								dDataValue = lightness;
	//							}
	//							else
 //                               {
	//								// 色相
	//								dDataValue = hue;
	//							}

	//							if (dDataValue > dLimitMin && dDataValue < dLimitMax)
	//							{
	//								// 設定範囲内のもののみを対象にデータをセットする
	//								pixcelCount = pixcelCount + 1;
	//								addH += hue;
	//								addS += saturation;
	//								addL += lightness;
	//								hList.Add(hue);

	//								rList.Add(r);
	//								gList.Add(g);
	//								bList.Add(b);
	//							}
	//							else
	//							{
	//								//strLog = strLog + ",,";
	//								//m_cLogMainProcess.outputLog(strLog);
	//							}
	//						}
	//						else
	//						{
	//							//strLog = strLog + ",gray";
	//							//m_cLogMainProcess.outputLog(strLog);
	//							//Console.WriteLine(strLog);
	//						}
	//					}
	//					lineIndex += stride;
	//				}

	//				if(pixcelCount > 0)
 //                   {
	//					aveH = addH / pixcelCount;
	//					aveS = addS / pixcelCount;
	//					aveL = addL / pixcelCount;
	//					medH = hList.Median();

	//					strLog = "No." + nNo + " result HSL : hue=" + aveH.ToString("0.00000") + " (" + medH.ToString("0.00000") + ") saturation=" + aveS.ToString("0.00000") + " lightness=" + aveL.ToString("0.00000");
	//					m_cLogMainProcess.outputLog(strLog);
	//					Console.WriteLine(strLog);
	//				}
 //                   else
 //                   {
	//					strLog = "No." + nNo + " result is no data.";
	//					m_cLogMainProcess.outputLog(strLog);
	//					Console.WriteLine(strLog);

	//				}
										
	//				// 分割画像の各値にデータをセットする
	//				m_TargetData.Add(new clrData(nNo, pixcelCount, aveH, aveS, aveL, medH));

	//				if (pixcelCount > 0)
 //                   {
	//					medR = rList.Median();
	//					medG = gList.Median();
	//					medB = bList.Median();
	//					setResultRGB(nNo, medR, medG, medB);
	//				}
	//				else
 //                   {
	//					setResultRGB(nNo, medR, medG, medB);
	//				}
	//			}

	//			// アンロック
	//			bmp.UnlockBits(bmpData);
	//		}
	//		catch (Exception ex)
	//		{
	//			System.Diagnostics.Debug.WriteLine(ex.Message);
	//			MessageBox.Show(ex.Message);
	//		}
	//	}
 //       #endregion

 //       #region "   RGB成分の基準色情報処理                           "
	//	//private void initDataRGB()
	//	//{
	//	//	m_StandardRGB.Clear();

	//	//	setStandardRGB(0, "White", 255, 255, 255);			// 白
	//	//	setStandardRGB(1, "Red", 255, 0, 0);				// 赤
	//	//	setStandardRGB(2, "Yellowgreen", 0, 255, 0);        // 黄緑
	//	//	setStandardRGB(3, "Blue", 0, 0, 255);				// 青
	//	//	setStandardRGB(4, "Yellow", 255, 255, 0);           // 黄色
	//	//	setStandardRGB(5, "Pink", 255, 0, 255);				// 桃色
	//	//	setStandardRGB(6, "Lightblue", 0, 255, 255);        // 水色
	//	//	setStandardRGB(7, "Gray", 128, 128, 128);           // 灰色
	//	//	setStandardRGB(8, "Brown", 128, 0, 0);				// 茶色
	//	//	setStandardRGB(9, "Green", 0, 128, 0);				// 緑
	//	//	setStandardRGB(10, "Darkblue", 0, 0, 128);          // 紺
	//	//	setStandardRGB(11, "Ocher", 128, 128, 0);           // 黄土色
	//	//	setStandardRGB(12, "Perpule", 128, 0, 128);         // 紫
	//	//	setStandardRGB(13, "Bluegreen", 0, 128, 128);       // 青緑
	//	//	setStandardRGB(14, "Black", 0, 0, 0);               // 黒

	//	//	m_StandardCalc = new int[14];
	//	//	m_StandardNo = 0;

	//	//	return;
	//	//}

	//	//public void setStandardRGB(int id, string name, int r, int g, int b)
	//	//{
	//	//	m_StandardRGB.Add(new clrColorRGB());
	//	//	m_StandardRGB[id].name = name;
	//	//	m_StandardRGB[id].R = r;
	//	//	m_StandardRGB[id].G = g;
	//	//	m_StandardRGB[id].B = b;
	//	//}

	//	//private void setDataRGBstandard(int nR, int nG, int nB)
	//	//{
	//	//	int temp = 0;

	//	//  if (nR > 200 && nG > 200 && nB > 200)
	//	//		// 255, 255, 255,	:白
	//	//		temp = 0;
	//	//	else if (nR < 50 && nG < 50 && nB < 50)
	//	//		//   0,   0,   0,	:黒
	//	//		temp = 9;
	//	//	else if (nR > 200 && nG < 200 && nB < 200)
	//	//		// 255,   0,   0,	:赤
	//	//		temp = 1;
	//	//	else if (nR > 200 && nG > 200 && nB < 200)
	//	//		// 255, 255,   0,	:黄色
	//	//		temp = 2;
	//	//	else if (nR < 200 && nG > 200 && nB < 200)
	//	//		//   0, 255,   0,	:緑
	//	//		temp = 3;
	//	//	else if (nR < 200 && nG > 200 && nB > 200)
	//	//		//   0, 255, 255,	:水色
	//	//		temp = 4;
	//	//	else if (nR < 200 && nG < 200 && nB > 200)
	//	//		//   0,   0, 255,	:青
	//	//		temp = 5;
	//	//	else if (nR > 200 && nG < 200 && nB > 200)
	//	//		// 255,   0, 255,	:紫
	//	//		temp = 6;
	//	//	else if (nR > 100 && nR < 150 && nG > 100 && nG < 150 && nB > 100 && nB < 150)
	//	//		// 128, 128, 128,	:灰色
	//	//		temp = 7;

	//	//	return;
	//	//}

	//	#endregion

	//	#region "   RGB成分から白・黒・灰に該当するか求める           "
	//	private int checkGrayRGB(int nR, int nG, int nB)
	//	{
	//		int intRet = 0;

	//		int intAv = ((nR + nG + nB) / 3); 
	//		int intRv = Math.Abs(nR - intAv);
	//		int intGv = Math.Abs(nG - intAv);
	//		int intBv = Math.Abs(nB - intAv);

	//		// 白・黒・灰の場合は判定対象外とするため、戻りは0以外
	//		if (nR > 200 && nG > 200 && nB > 200)
	//			// 白
	//			intRet = 255;
	//		else if (nR < 50 && nG < 50 && nB < 50)
	//			// 黒
	//			intRet = 255;
	//		else if(intRv < 20 && intGv < 20 && intBv < 20)
	//			// 灰
	//			intRet = 1;

	//		return intRet;
	//	}
	//	#endregion

	//	#region "   HSL成分の色合い範囲情報をセットする               "
	//	private void setHslRange(double nH, double nS, double nL)
	//	{
	//		int iHueValue = 0;
	//		double dRange;

	//		if (nH >= -30 && nH < 30)
	//			// 赤
	//			iHueValue = 0;
	//		else if (nH >= 30 && nH < 90)
	//			// 黄
	//			iHueValue = 1;
	//		else if (nH >= 90 && nH < 150)
	//			// 緑
	//			iHueValue = 2;
	//		else if (nH >= 150 && nH < 210)
	//			// 水色
	//			iHueValue = 3;
	//		else if (nH >= 210 && nH < 270)
	//			// 青
	//			iHueValue = 4;
	//		else if (nH >= 270 && nH < 330)
	//			// 紫
	//			iHueValue = 5;
	//		else if (nH >= 330 && nH < 360)
	//		{
	//			// 赤
	//			iHueValue = 0;
	//			nH = nH - 360;
	//		}

	//		// 色合い取得時のHue値範囲を決定する
	//		//m_HueMax = 60 * iHueValue + ciDiff;
	//		//m_HueMin = 60 * iHueValue - ciDiff;
	//		// 各対象のHue値前後で設定するようにしてみる
	//		dRange = double.Parse(m_cParaMainProcess.GraphicRangeHue) / 2;
	//		m_HueMax = nH + dRange;
	//		m_HueMin = nH - dRange;

	//		string strLog = "Hue[" + iHueValue + "] range : min=" + m_HueMin + " max=" + m_HueMax;
	//		//m_cLogMainProcess.outputLog(strLog);
	//		//Console.WriteLine(strLog);

	//		// 彩度
	//		dRange = double.Parse(m_cParaMainProcess.GraphicRangeSaturation) / 2;
	//		m_SaturationMax = nS + dRange;
	//		m_SaturationMin = nS - dRange;

	//		// 明度
	//		dRange = double.Parse(m_cParaMainProcess.GraphicRangeLightness) / 2;
	//		m_LightnessMax = nL + dRange;
	//		m_LightnessMin = nL - dRange;

	//		return;
	//	}

	//	#endregion

	//	#region "   Color/RGB結果情報をセットする                     "
	//	private void setResultRGB(int id, int r, int g, int b)
	//	{
	//		m_ResultRGB.Add(new clrColorRGB());
	//		m_ResultRGB[id].name = "No" + id.ToString();
	//		m_ResultRGB[id].R = r;
	//		m_ResultRGB[id].G = g;
	//		m_ResultRGB[id].B = b;
	//	}
 //       #endregion

 //       #region "   Color -> HSL 変換                                 "
 //       /// <summary>
 //       /// Color 型から HSL 値に変換する
 //       /// </summary>
 //       /// <param name="color"></param>
 //       /// <param name="hue"></param>
 //       /// <param name="saturation"></param>
 //       /// <param name="lightness"></param>
 //       static public void getRgbToHsl(Color color, ref double hue, ref double saturation, ref double lightness)
	//	{
	//		double r, g, b, h, s, l;

	//		r = color.R / 255.0;
	//		g = color.G / 255.0;
	//		b = color.B / 255.0;

	//		double maxColor = Math.Max(r, Math.Max(g, b));
	//		double minColor = Math.Min(r, Math.Min(g, b));

	//		// lightness
	//		l = (minColor + maxColor) / 2;

	//		if (maxColor == minColor)
	//		{
	//			h = 0.0;
	//			s = 0.0;
	//		}
	//		else
	//		{
	//			// saturation
	//			if (l < 0.5)
	//				s = (maxColor - minColor) / (maxColor + minColor);
	//			else
	//				s = (maxColor - minColor) / (2.0 - maxColor - minColor);

	//			// hue
	//			if (r == maxColor)
	//				h = (g - b) / (maxColor - minColor);
	//			else if (g == maxColor)
	//				h = 2.0 + (b - r) / (maxColor - minColor);
	//			else
	//				h = 4.0 + (r - g) / (maxColor - minColor);

	//			h /= 6;

	//			if (h < 0)
	//				++h;
	//		}

	//		// 0 ～ 1 の範囲内に制限する
	//		if (h < 0) h = 0;
	//		if (h > 1) h = 1;
	//		if (s < 0) s = 0;
	//		if (s > 1) s = 1;
	//		if (l < 0) l = 0;
	//		if (l > 1) l = 1;

	//		hue = h * 360;
	//		saturation = s;
	//		lightness = l;
	//	}

	//	#endregion

	//	#region "   HSL -> Color 変換                                 "
	//	/// <summary>
	//	/// HSL 値から Color 型に変換する
	//	/// </summary>
	//	/// <param name="hue">0-360</param>
	//	/// <param name="saturation">0-1</param>
	//	/// <param name="lightness">0-1</param>
	//	/// <returns></returns>
	//	public static Color HslToRgb(double hue, double saturation, double lightness)
	//	{
	//		double r, g, b, h, s, l, s1, s2, r1, g1, b1;

	//		h = hue / 360.0;
	//		s = saturation;
	//		l = lightness;

	//		if (s == 0)
	//		{
	//			r = g = b = l;
	//		}
	//		else
	//		{
	//			if (l < 0.5)
	//			{
	//				s2 = l * (1 + s);
	//			}
	//			else
	//			{
	//				s2 = (l + s) - (l * s);
	//			}

	//			s1 = 2 * l - s2;
	//			r1 = h + 1.0 / 3.0;

	//			if (r1 > 1)
	//			{
	//				--r1;
	//			}

	//			g1 = h;
	//			b1 = h - 1.0 / 3.0;

	//			if (b1 < 0)
	//				++b1;

	//			// R
	//			if (r1 < 1.0 / 6.0)
	//				r = s1 + (s2 - s1) * 6.0 * r1;
	//			else if (r1 < 0.5)
	//				r = s2;
	//			else if (r1 < 2.0 / 3.0)
	//				r = s1 + (s2 - s1) * ((2.0 / 3.0) - r1) * 6.0;
	//			else
	//				r = s1;

	//			// G
	//			if (g1 < 1.0 / 6.0)
	//				g = s1 + (s2 - s1) * 6.0 * g1;
	//			else if (g1 < 0.5)
	//				g = s2;
	//			else if (g1 < 2.0 / 3.0)
	//				g = s1 + (s2 - s1) * ((2.0 / 3.0) - g1) * 6.0;
	//			else g = s1;

	//			// B
	//			if (b1 < 1.0 / 6.0)
	//				b = s1 + (s2 - s1) * 6.0 * b1;
	//			else if (b1 < 0.5)
	//				b = s2;
	//			else if (b1 < 2.0 / 3.0)
	//				b = s1 + (s2 - s1) * ((2.0 / 3.0) - b1) * 6.0;
	//			else
	//				b = s1;
	//		}

	//		// 0 ～ 1 の範囲内に制限する
	//		if (h < 0) h = 0;
	//		if (h > 1) h = 1;
	//		if (s < 0) s = 0;
	//		if (s > 1) s = 1;
	//		if (l < 0) l = 0;
	//		if (l > 1) l = 1;

	//		return Color.FromArgb(Convert.ToByte(r * 255), Convert.ToByte(g * 255), Convert.ToByte(b * 255));
	//	}

	//	#endregion



	//	#region "   チャート情報初期化処理                            "
	//	/// <summary>
	//	/// チャート情報初期化処理
	//	/// </summary>
	//	public void InitChartInfomation()
	//	{
	//		m_TargetOrder = new int[10];
	//		mColorValue = new double[10];
	//		mColorName = new string[10];
	//		mColorName[0] = "Red";
	//		mColorName[1] = "Orange";
	//		mColorName[2] = "Yellow";
	//		mColorName[3] = "Green";
	//		mColorName[4] = "Blue";
	//		mColorName[5] = "Purple";
	//		mColorName[6] = "Pink";
	//		mColorName[7] = "Brown";
	//		mColorName[8] = "YellowGreen";
	//		mColorName[9] = "Black";
	//	}

	//	#endregion

	//	#region "   チャート情報取得処理                              "
	//	/// <summary>
	//	/// チャート情報取得処理
	//	/// </summary>
	//	/// <param name="color_name">色名称</param>
	//	/// <param name="color_value">色分析値</param>
	//	/// <param name="color_order">操作順位</param>
	//	public void GetChartInfomation(ref List<clrColorRGB> color_name, ref double[] color_value, ref int[] color_order)
	//	{
	//		color_name = m_ResultRGB;
	//		color_value = mColorValue;
	//		// 表示用に操作順位を並べ替える
	//		int intNum;
	//		for(int intLoop = 0; intLoop < m_TargetOrder.Count(); intLoop++)
 //           {
	//			intNum = m_TargetOrder[intLoop];
	//			color_order[intNum] = intLoop;
	//		}
	//	}

	//	#endregion

	//	#region "   チャート情報設定処理                              "
	//	/// <summary>
	//	/// チャート情報設定処理
	//	/// </summary>
	//	/// <param name="nParam">並べ替え要素No</param>
	//	public void SetChartInfomation()
	//	{
	//		for (int intLoop = 0; intLoop < m_TargetData.Count; intLoop++)
 //           {
	//			if(m_OrderNo == 2)
	//				// 彩度
	//				mColorValue[intLoop] = m_TargetData[intLoop].saturation;
	//			else if (m_OrderNo == 3)
	//				// 明度
	//				mColorValue[intLoop] = m_TargetData[intLoop].lightness;
	//			else
	//				// 色相
	//				mColorValue[intLoop] = m_TargetData[intLoop].hue;
	//		}
	//	}

	//	public void SetChartDebugInfomation(int nParam)
	//	{
	//		mColorValue[0] = 0.9 * nParam;
	//		mColorValue[1] = 1.1 * nParam;
	//		mColorValue[2] = 1.2 * nParam;
	//		mColorValue[3] = 1.4 * nParam;
	//		mColorValue[4] = 1.0 * nParam;
	//		mColorValue[5] = 0.8 * nParam;
	//		mColorValue[6] = 1.3 * nParam;
	//		mColorValue[7] = 1.1 * nParam;
	//		mColorValue[8] = 0.7 * nParam;
	//		mColorValue[9] = 0.5 * nParam;
	//	}

	//	#endregion
	//}


	//#region "   色分析結果データ関連Class                             "

	//public class clrColorRGB
	//{
	//	public string name;
	//	public int R;
	//	public int G;
	//	public int B;
	//}

	//public class clrData
 //   {
	//	public int ID;					// 分割ID
	//	public double hue;              // 分割画像色相ave
	//	public double hueMedian;        // 分割画像色相median
	//	public double saturation;       // 分割画像彩度ave
	//	public double lightness;        // 分割画像明度ave
	//	public int count;

	//	public clrData(int ID, int count, double hue, double saturation, double lightness, double hueMedian)
	//	{
	//		this.ID = ID;
	//		this.count = count;
	//		this.hue = hue;
	//		this.hueMedian = hueMedian;
	//		this.saturation = saturation;
	//		this.lightness = lightness;
	//	}

 //       #region "   並べ替え処理                                      "
 //       // ID昇順
 //       public static int CompareIDAsc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return -1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return 1;
	//			}

	//			// aとbの比較
	//			return a.ID.CompareTo(b.ID);
	//		}
	//	}

	//	// ID降順
	//	public static int CompareIDDesc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return 1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return -1;
	//			}

	//			// bとaの比較
	//			return b.ID.CompareTo(a.ID);
	//		}
	//	}

	//	// hue昇順
	//	public static int CompareHueAsc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return -1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return 1;
	//			}

	//			// aとbの比較
	//			return a.hue.CompareTo(b.hue);
	//		}
	//	}

	//	// hue降順
	//	public static int CompareHueDesc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return 1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return -1;
	//			}

	//			// bとaの比較
	//			return b.hue.CompareTo(a.hue);
	//		}
	//	}

	//	// saturation昇順
	//	public static int CompareSatAsc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return -1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return 1;
	//			}

	//			// aとbの比較
	//			return a.saturation.CompareTo(b.saturation);
	//		}
	//	}

	//	// saturation降順
	//	public static int CompareSaDesc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return 1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return -1;
	//			}

	//			// bとaの比較
	//			return b.saturation.CompareTo(a.saturation);
	//		}
	//	}

	//	// lightness昇順
	//	public static int CompareLigAsc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return -1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return 1;
	//			}

	//			// aとbの比較
	//			return a.lightness.CompareTo(b.lightness);
	//		}
	//	}

	//	// lightness降順
	//	public static int CompareLigDesc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return 1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return -1;
	//			}

	//			// bとaの比較
	//			return b.lightness.CompareTo(a.lightness);
	//		}
	//	}

	//	// hueMedian昇順
	//	public static int CompareHueMedianAsc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return -1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return 1;
	//			}

	//			// aとbの比較
	//			return a.hueMedian.CompareTo(b.hueMedian);
	//		}
	//	}

	//	// hueMedian降順
	//	public static int CompareHueMedianDesc(clrData a, clrData b)
	//	{
	//		// nullチェック
	//		if (a == null)
	//		{
	//			if (b == null)
	//			{
	//				return 0;
	//			}
	//			return 1;
	//		}
	//		else
	//		{
	//			if (b == null)
	//			{
	//				return -1;
	//			}

	//			// bとaの比較
	//			return b.hueMedian.CompareTo(a.hueMedian);
	//		}
	//	}

	//	#endregion

	//}
 //   #endregion

 //   #region "   Median処理関連Class                                   "

 //   public static class LinQCustomMethods
	//{
	//	// メディアン算出メソッド（Generics）
	//	public static T Median<T>(this IEnumerable<T> src)
	//	{
	//		//ジェネリックの四則演算用クラス
	//		var ao = new ArithmeticOperation<T>();
	//		//昇順ソート
	//		var sorted = src.OrderBy(a => a).ToArray();
	//		if (!sorted.Any())
	//		{
	//			throw new InvalidOperationException("Cannot compute median for an empty set.");
	//		}
	//		int medianIndex = sorted.Length / 2;
	//		//要素数が偶数のとき、真ん中の2要素の平均を出力
	//		if (sorted.Length % 2 == 0)
	//		{
	//			//四則演算可能な時のみ算出
	//			if (ao.ArithmeticOperatable(typeof(T)))
	//			{
	//				//return ao.Divide(ao.Add(sorted[medianIndex], sorted[medianIndex - 1]), (T)(object)2.0);
	//				return sorted[medianIndex+1];
	//			}
	//			else throw new InvalidOperationException("Cannot compute arithmetic operation");
	//		}
	//		//奇数のときは、真ん中の値を出力
	//		else
	//		{
	//			return sorted[medianIndex];
	//		}
	//	}

	//	// メディアン算出（DateTime型のみ別メソッド）
	//	public static DateTime Median(this IEnumerable<DateTime> src)
	//	{
	//		//昇順ソート
	//		var sorted = src.OrderBy(a => a).ToArray();
	//		if (!sorted.Any())
	//		{
	//			throw new InvalidOperationException("Cannot compute median for an empty set.");
	//		}
	//		int medianIndex = sorted.Length / 2;
	//		//要素数が偶数のとき、真ん中の2要素の平均を出力
	//		if (sorted.Length % 2 == 0)
	//		{
	//			return sorted[medianIndex] + new TimeSpan((sorted[medianIndex - 1] - sorted[medianIndex]).Ticks / 2);
	//		}
	//		//奇数のときは、真ん中の値を出力
	//		else
	//		{
	//			return sorted[medianIndex];
	//		}
	//	}
	//}
	////ジェネリック四則演算用クラス
	//public class ArithmeticOperation<T>
	//{
	//	/// <summary>
	//	/// 四則演算適用可能かを判定
	//	/// </summary>
	//	/// <param name="src">判定したいタイプ</param>
	//	/// <returns></returns>
	//	public bool ArithmeticOperatable(Type srcType)
	//	{
	//		//四則演算可能な型の一覧
	//		var availableT = new Type[]
	//		{
	//		typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(byte),
	//		typeof(decimal), typeof(double)
	//		};
	//		if (availableT.Contains(srcType)) return true;
	//		else return false;
	//	}

	//	/// <summary>
	//	/// 四則演算可能なクラスに対しての処理
	//	/// </summary>
	//	public ArithmeticOperation()
	//	{
	//		var availableT = new Type[]
	//		{
	//		typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(byte),
	//		typeof(decimal), typeof(double)
	//		};
	//		if (!availableT.Contains(typeof(T)))
	//		{
	//			throw new NotSupportedException();
	//		}
	//		var p1 = Expression.Parameter(typeof(T));
	//		var p2 = Expression.Parameter(typeof(T));
	//		Add = Expression.Lambda<Func<T, T, T>>(Expression.Add(p1, p2), p1, p2).Compile();
	//		Subtract = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(p1, p2), p1, p2).Compile();
	//		Multiply = Expression.Lambda<Func<T, T, T>>(Expression.Multiply(p1, p2), p1, p2).Compile();
	//		Divide = Expression.Lambda<Func<T, T, T>>(Expression.Divide(p1, p2), p1, p2).Compile();
	//		Modulo = Expression.Lambda<Func<T, T, T>>(Expression.Modulo(p1, p2), p1, p2).Compile();
	//		Equal = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(p1, p2), p1, p2).Compile();
	//		GreaterThan = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThan(p1, p2), p1, p2).Compile();
	//		GreaterThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.GreaterThanOrEqual(p1, p2), p1, p2).Compile();
	//		LessThan = Expression.Lambda<Func<T, T, bool>>(Expression.LessThan(p1, p2), p1, p2).Compile();
	//		LessThanOrEqual = Expression.Lambda<Func<T, T, bool>>(Expression.LessThanOrEqual(p1, p2), p1, p2).Compile();
	//	}
	//	public Func<T, T, T> Add { get; private set; }
	//	public Func<T, T, T> Subtract { get; private set; }
	//	public Func<T, T, T> Multiply { get; private set; }
	//	public Func<T, T, T> Divide { get; private set; }
	//	public Func<T, T, T> Modulo { get; private set; }
	//	public Func<T, T, bool> Equal { get; private set; }
	//	public Func<T, T, bool> GreaterThan { get; private set; }
	//	public Func<T, T, bool> GreaterThanOrEqual { get; private set; }
	//	public Func<T, T, bool> LessThan { get; private set; }
	//	public Func<T, T, bool> LessThanOrEqual { get; private set; }
	//}

 //   #endregion

}
