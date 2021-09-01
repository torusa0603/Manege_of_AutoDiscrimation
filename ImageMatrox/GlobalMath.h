//
//	算術計算用クラス	since 2011.1.26
//

#pragma once

//	複素数
typedef struct{
  double re;
  double im;
} complex;

//#ifdef __cplusplus
//extern "C" {
//#endif

class CGlobalMath
{
public:
	//	逆行列を求める
	int InverseMatrix( double *pdOrgMatrix, double *pdInvMatrix, int niN );
	//	最小二乗法により、近似曲線を求める
	int ApproximationByLeastSquaresMethod( double *pdX, double *pdY, int niNum, int niDimention,
										   double *pdCoeff );
	//	最小二乗法により、近似円を求める
	int CircleApproxByLeastSquaresMethod( double *pdX, double *pdY, int niNum,
										  double *pCenterX, double *pCenterY, double *pRadius );
	//	最小二乗法により、近似平面を求める
	int SurfaceApproxByLeastSquaresMethod( double *pdX, double *pdY, double *pdZ, int niNum,
										   double *pa, double *pb, double *pd );


	//	角度をdegree(°)からrad(π)に変換
	double DegreeToRadian( double dDegree );
	//	角度をrad(π)からdegree(°)に変換
	double RadianToDegree( double dRad );

	//	ロバスト推定、特異点除去など

	//	スプライン補間を行う
	int	Spline( double x[], double y[], int n, double y2[] );
	int Splint( double xa[], double ya[], double y2a[], int n, double x, double *y);

	//	1次元データのFFTを行う(画像のFFTでない)
	int fft(complex *x,int m);
	//	1次元の逆フーリエ変換を行う
	int ifft(complex *x,int m);

	//	1次元データのDFTを行う(画像のDFTでない)
	int dft( complex *x, int nDataNum );
	//	1次元データの逆DFTを行う(画像のDFTでない)
	int idft(complex *x, int nDataNum );

	//	1次元データのFFTを行う(その２）
	int* BitScrollArray(int arraySize);
	void FFT2(double* inputRe, double* inputIm, double* outputRe, double* outputIm, int bitSize);
	void IFFT2(double* inputRe, double* inputIm, double* outputRe, double* outputIm, int bitSize);

	//基数ソートアルゴリズムを実行
	void RadixSort( unsigned short *npiData, unsigned short niDataNum, int niMaxRadix );

	//	データの標準偏差(σ)を求める
	double getStandardDeviation( double *npdData, int niDataNum );

	//*********************************************//
	//											   //
	//	デジタルフィルタ(FIR Filter)に関する機能    //
	//											   //
	//*********************************************//
	//	ローパスフィルタを作成する
	int makeLowPassFilter( double *npdFilterTap, double fs, double f, int niTap, int niWindow );
	//	フィルタリング実行
	void filtering( complex *InputData, int DataNum, double *npdFilterTap, int niTap );
	//	フィルタの周波数特性を求める
	void filterCharacteristic(  double *npdFilterTap, int niTap );
	//	白色干渉計に最適なローパスフィルタのパラメーターを計算する
	void calcLowPassFilterParameter( double ndPitchUM, double *npdFs, double *npdFc, int *npiWindow, int *npiTapNum );	



	//*********************************************//
	//											   //
	//	以下画像データ(2次元データ)に関する機能    //
	//											   //
	//*********************************************//
	//	相関係数を求める関数
	double getCorrelationCoefficient( int niModelImage[], int niImage[], int niSize ); 
	//	画像のフーリエ変換を行う関数(DFT)
	int FourierTransformDFT( int *niInputImage[], int niWidth, int niHeight, double *ndMagnitude[] );
	//	背景画像差分を求める関数
	int DifferenceImageFromBackGround( int niInputImage[], int niBackImage[], int niSize ); 
	//	２次元フィルタリングを行う関数
	int	Filtering2Dim( int *niInputImage[], double ndFilter[][3], int niTap, double *ndOutputImage[],int niWidth, int niHeigh );
	//	アフィン変換を行う関数
	int AffineTranslation( int *niInputImage[], int *niOutputImage[], int niWidthInput, int niHeightInput,
									int niWidthOutput,	int niHeightOutput,	
									double ndA,  double ndB, double ndC, double ndD, double ndE, double ndF );
	//	メディアンフィルターをかける
	void FilterMedian( unsigned short *npImageArray, int niWidth, int niHeight );
	//	相関パターンマッチングを実行
	double PatternMatchingByCorrelation( int niModelImage[], int niImage[], int niModelWidth, int niModelHeight,
										 int niImageWidth, int niImageHeight, int* niPatternPosX, int* niPatternPosY );

};
//#ifdef __cplusplus
//}
//#endif
