//
//	算術計算用クラス	since 2011.1.26
//

#include ".\GlobalMath.h"
#include <math.h>
#include <malloc.h>
#include <windows.h>

#include <stdio.h>

#define PI (atan(1.0)*4.0)


/*------------------------------------------------------------------------------------------
	1.日本語名
		逆行列を求める関数

	2.パラメタ説明
		pdOrgMatrix	(IN)	オリジナル行列
		pdInvMatrix	(OUT)	逆行列
		niSize		(IN)	N×N行列のN


	3.概要
		Gauss-Jordan法により、逆行列を求める。
		入力する行列は正方行列であることが原則である

	4.機能説明
		入力、出力される行列は１次元配列。
		例えば 
		    2,3,2
			1,2,2
			4,5,3
		という行列は、a[9] = { 2,3,2,1,2,2,4,5,3 }
		と表される

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::InverseMatrix( double *pdOrgMatrix, double *pdInvMatrix, int niN )
{
    int i_loop, i_loop2, i_loop_k;
    double d_temp;

    //単位行列で初期化
    for (i_loop2 = 0; i_loop2 < niN; i_loop2++)
	{
        for (i_loop = 0; i_loop < niN; i_loop++)
		{
            if (i_loop == i_loop2)
			{
                pdInvMatrix[i_loop + i_loop2 * niN] = 1;
            }
			else
			{
                pdInvMatrix[i_loop + i_loop2 * niN] = 0;
            }
        }
    }

    for (i_loop_k = 0; i_loop_k < niN; i_loop_k++)
	{
        //k行k列目の要素を１にする
        d_temp = pdOrgMatrix[i_loop_k + i_loop_k * niN];
        if (d_temp == 0)
		{
			return -1;    //エラー
		}
        for (i_loop = 0; i_loop < niN; i_loop++)
		{
            pdOrgMatrix[i_loop + i_loop_k * niN] /= d_temp;
            pdInvMatrix[i_loop + i_loop_k * niN] /= d_temp;
        }

        for (i_loop2 = 0; i_loop2 < niN; i_loop2++)
		{
            if (i_loop2 != i_loop_k)
			{
                d_temp = pdOrgMatrix[i_loop_k + i_loop2 * niN] / pdOrgMatrix[i_loop_k + i_loop_k * niN];
                for (i_loop = 0; i_loop < niN; i_loop++)
				{
                    pdOrgMatrix[i_loop + i_loop2 * niN] -= pdOrgMatrix[i_loop + i_loop_k * niN] * d_temp;
                    pdInvMatrix[i_loop + i_loop2 * niN] -= pdInvMatrix[i_loop + i_loop_k * niN] * d_temp;
                }            
            }
        }
    }
    //正常終了
    return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		最小二乗法により、近似曲線を求める

	2.パラメタ説明
		pdX			(IN)	入力するX座標の配列
		pdY			(IN)	入力するY座標の配列
		niNum		(IN)	X,Yのデータ数
		niDimention	(IN)	何次式に近似するか (1 < niDimention )
		pdCoeff		(OUT)	近似式の係数

	3.概要
		近似式は y = a0 * x^2 + a1 * x + a2
		のように、次数の高い係数の順に0,1,2としていく。
		つまり、２次式近似ならば
		a[0], a[1], a[2]がそれぞれ x^2, x^1, x^0 の係数となる

		係数の数は近似式の次数+1となる

	4.機能説明
		http://atsugi5761455.fc2web.com/calking16.html
		など参照

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::ApproximationByLeastSquaresMethod( double *pdX, double *pdY, int niNum, int niDimention,
													double *pdCoeff )
{
	double *d_matrixA, *d_matrixB;
	double *d_invMatrixA;
	double d_sum;
	double d_temp;
	int i_loop, i_loop2;
	int i_row, i_column;
	int i_dim;

	//	データが3つ以下ならばエラーとする
	if( niNum < 3 )
	{
		return -1;
	}
	//	次元はこの関数では上限8とする。下限は1とする
	if( niDimention < 1 || niDimention > 8 )
	{
		return -1;
	}

	//	まず行列の作成を行う
	//                  
	//  1次式近似ならA  |    n    Σx    |     と　　B　|  Σy  |
 	//				    |	Σx  Σx^2  |			    |  Σxy |
	//
	//  2次式なら　　A　| n    Σx    Σx^2 |		  B |  Σy   |
	//				    | Σx  Σx^2  Σx^3 |  と	    |  Σxy  |
	//				    | Σx^2 Σx^3 Σx^4 |		    |  Σx^2y|
	//


	//	まずAの行列から作成する
	//	配列を確保
	d_matrixA = new double[ (niDimention + 1) * (niDimention + 1) ];
	d_invMatrixA = new double[ (niDimention + 1) * (niDimention + 1) ];

	for( i_loop = 0; i_loop < (niDimention + 1) * (niDimention + 1); i_loop++ )
	{
		//	現在の行数(0～)
		i_column = i_loop / (niDimention + 1);
		//	現在の列数(0～)
		i_row = i_loop % (niDimention + 1);
		//	Σのxの次数を求める
		i_dim = i_column + i_row;

		//	次数が0のときだけ特別
		if( i_dim == 0 )
		{
			//	n
			d_matrixA[i_loop] = ( double )niNum;
		}
		else
		{
			// Σx^???
			d_sum = 0.0;
			for( i_loop2 = 0; i_loop2 < niNum; i_loop2++ )
			{
				d_temp = pow( pdX[i_loop2], ( double )i_dim );
				d_sum += d_temp;
			}
			d_matrixA[i_loop] = d_sum;
		}
	}

	//	次にBの行列を求める
	d_matrixB = new double[ (niDimention + 1) ];

	for( i_loop = 0; i_loop < (niDimention + 1); i_loop++ )
	{
		d_sum = 0.0;
		for( i_loop2 = 0; i_loop2 < niNum; i_loop2++ )
		{
			d_temp = pow( pdX[i_loop2], ( double )i_loop );
			d_temp = d_temp * pdY[i_loop2];
			d_sum += d_temp;
		}
		d_matrixB[i_loop] = d_sum;
	}

	//	Aの逆行列を求める
	InverseMatrix( d_matrixA, d_invMatrixA, niDimention + 1 );

	//	Aの逆行列とBを掛ける
	//	**近似式の係数の配列の添え字に注意する
	for( i_loop = 0; i_loop < (niDimention + 1); i_loop++ )
	{
		d_sum = 0.0;
		for( i_loop2 = 0; i_loop2 < (niDimention + 1); i_loop2++ )
		{
			d_temp = d_invMatrixA[ i_loop * (niDimention + 1) + i_loop2 ] * d_matrixB[i_loop2];
			d_sum += d_temp;
		}
		pdCoeff[niDimention - i_loop] = d_sum;
	}
	//	メモリ解放
	delete [] d_matrixA;
	delete [] d_matrixB;
	delete [] d_invMatrixA;

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		最小二乗法により、近似円を求める

	2.パラメタ説明
		pdX			(IN)	入力するX座標の配列
		pdY			(IN)	入力するY座標の配列
		niNum		(IN)	X,Yのデータ数
		pCenterX	(OUT)	円の中心座標X
		pCenterY	(OUT)	円の中心座標Y
		pRadius		(OUT)	円の半径

	3.概要
		| Σx^2	Σxy  Σx |   | A |   | -Σ(x^3 + xy^2)  |
		| Σxy  Σy^2 Σy | * | B | = | -Σ(x^2*y + y^3) |
		| Σx	Σy   Σ1 |   | C |   | -Σ(x^2 + y^2)   |

		A = -2a
		B = -2b
		C = a^2 + b^2 - r^2

		中心(a,b)半径rの円


	4.機能説明
		http://imagingsolution.blog107.fc2.com/blog-entry-16.html
		など参照

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::CircleApproxByLeastSquaresMethod( double *pdX, double *pdY, int niNum,
												   double *pCenterX, double *pCenterY, double *pRadius )
{
	double d_matrixA[9], d_matrixB[3];
	double d_invMatrixA[9];
	double d_sum;
	double d_temp;
	double d_coeff[3];
	int i_loop, i_loop2;

	//	データが3つ以下ならばエラーとする
	if( niNum < 3 )
	{
		return -1;
	}

	//	行列Aを求める。綺麗な計算方法が思いつかないので１つ１つ計算する
	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pow( pdX[i_loop], 2.0 );
		d_sum += d_temp;
	}
	d_matrixA[0] = d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdX[i_loop] * pdY[i_loop];
		d_sum += d_temp;
	}
	d_matrixA[1] = d_sum;
	d_matrixA[3] = d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdX[i_loop];
		d_sum += d_temp;
	}
	d_matrixA[2] = d_sum;
	d_matrixA[6] = d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pow( pdY[i_loop], 2.0 );
		d_sum += d_temp;
	}
	d_matrixA[4] = d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdY[i_loop];
		d_sum += d_temp;
	}
	d_matrixA[5] = d_sum;
	d_matrixA[7] = d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = 1;
		d_sum += d_temp;
	}
	d_matrixA[8] = d_sum;

	//	行列Bを求める
	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdX[i_loop] * pdX[i_loop] * pdX[i_loop] + pdX[i_loop] * pdY[i_loop] * pdY[i_loop];
		d_sum += d_temp;
	}
	d_matrixB[0] = -d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdX[i_loop] * pdX[i_loop] * pdY[i_loop] + pdY[i_loop] * pdY[i_loop] * pdY[i_loop];
		d_sum += d_temp;
	}
	d_matrixB[1] = -d_sum;

	d_sum = 0.0;
	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_temp = pdX[i_loop] * pdX[i_loop] + pdY[i_loop] * pdY[i_loop];
		d_sum += d_temp;
	}
	d_matrixB[2] = -d_sum;

	//	Aの逆行列を求める
	InverseMatrix( d_matrixA, d_invMatrixA, 3 );

	//	Aの逆行列とBを掛ける
	for( i_loop = 0; i_loop < 3; i_loop++ )
	{
		d_sum = 0.0;
		for( i_loop2 = 0; i_loop2 < 3; i_loop2++ )
		{
			d_temp = d_invMatrixA[ i_loop * 3 + i_loop2 ] * d_matrixB[i_loop2];
			d_sum += d_temp;
		}
		d_coeff[i_loop] = d_sum;
	}
	//	A = -2a からaを求める
	*pCenterX = -0.5 * d_coeff[0];
	//	B = -2b からbを求める
	*pCenterY = -0.5 * d_coeff[1];
	//	C = a^2 + b^2 - r^2 からrを求める
	*pRadius = *pCenterX * *pCenterX + *pCenterY * *pCenterY - d_coeff[2];
	*pRadius = sqrt( *pRadius );

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		角度をdegree(°)からrad(π)に変換

	2.パラメタ説明
		dDegree		(IN)	角度(°)

	3.概要
		角度をdegree(°)からrad(π)に変換

	4.機能説明
		なし

	5.戻り値
		rad(π)
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CGlobalMath::DegreeToRadian( double dDegree )
{
	return dDegree * ( PI / 180.0 );
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		角度をrad(π)からdegree(°)に変換

	2.パラメタ説明
		dRad		(IN)	rad(π)

	3.概要
		角度をrad(π)からdegree(°)に変換

	4.機能説明
		なし

	5.戻り値
		degree(°)
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CGlobalMath::RadianToDegree( double dRad )
{
	return dRad * ( 180.0 / PI );
}


/*------------------------------------------------------------------------------------------
	1.日本語名
		スプライン補間を行う

	2.パラメタ説明
		x[1]～x[n]、y[1]～y[n]、n個のデータからy2を作成する。添え字が0～n-1ではなく
		1～nであることに注意する

	3.概要
		スプライン補間を行う

	4.機能説明
		実際にスプライン補間を行う場合、まずこの関数で補間に必要となる配列y2を作成し、
		実際の補間処理は、Splint()にて行う。
		よって、本関数は、補間前に1回だけ呼べばよい。
		複数の補間データは全てSplint()にて行う。

	5.戻り値
		補間値
		
	6.備考
		ニューメリカルレシピから丸朴理した。

------------------------------------------------------------------------------------------*/
int CGlobalMath::Spline( double x[], double y[], int n, double y2[] )
{
	int i, k;
	double p,qn, sig, un, *u;
	double yp1 = 1.0e30;
	double ypn = 1.0e30;

	u = new double[n];

	if( yp1 > 0.99e30 )
	{
		y2[1] = u[1] = 0.0;
	}
	else
	{
		y2[1] = -0.5;
		u[1] = (3.0/(x[2]-x[1]))*((y[2]-y[1])/(x[2]-x[1])-yp1);
	}
	for( i=2;i<=n-1;i++)
	{
		sig = (x[i]-x[i-1])/(x[i+1]-x[i-1]);
		p=sig*y2[i-1]+2.0;
		y2[i]=(sig-1.0)/p;
		u[i]=(y[i+1]-y[i])/(x[i+1]-x[i])-(y[i]-y[i-1])/(x[i]-x[i-1]);
		u[i]=(6.0*u[i]/(x[i+1]-x[i-1])-sig*u[i-1])/p;
	}

	if(ypn > 0.99e30)
	{
		qn=un=0.0;
	}
	else
	{
		qn=0.5;
		un=(3.0/(x[n]-x[n-1]))*(ypn-(y[n]-y[n-1])/(x[n]-x[n-1]));
	}

	y2[n]=(un-qn*u[n-1])/(qn*y2[n-1]+1.0);
	for(k=n-1;k>=1;k--)
	{
		y2[k]=y2[k]*y2[k+1]+u[k];
	}

	delete [] u;


	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		スプライン補間を行う

	2.パラメタ説明
		x[1]～x[n]、y[1]～y[n]、n個のデータからy2を作成する。添え字が0～n-1ではなく
		1～nであることに注意する

	3.概要
		スプライン補間を行う

	4.機能説明
		この関数は、Splineを呼ぶ前に呼ぶことは出来ないので注意する。

	5.戻り値
		補間値
		
	6.備考
		ニューメリカルレシピから丸朴理した。
------------------------------------------------------------------------------------------*/
int CGlobalMath::Splint( double xa[], double ya[], double y2a[], int n, double x, double *y)
{
	int klo,khi,k;
	double h,b,a;

	klo=1;
	khi=n;
	while(khi-klo > 1 )
	{
		k=(khi+klo)>>1;
		if(xa[k]>x)
		{
			khi=k;
		}
		else
		{
			klo=k;
		}
	}
	h=xa[khi]-xa[klo];
	if(h==0.0)
	{
		return -1;
	}
	a=(xa[khi]-x)/h;
	b=(x-xa[klo])/h;
	*y=a*ya[klo]+b*ya[khi]+((a*a*a-a)*y2a[klo]+(b*b*b-b)*y2a[khi])*(h*h)/6.0;

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		FFTを行う

	2.パラメタ説明
		x	複素数の時間信号
		m	2の何乗か。データが1024ならm=10とする

	3.概要
		FFTを行う

	4.機能説明
		FFTを行う
		データ数は、2のべき乗(256,512,1024など)でなければならない。


	5.戻り値
		補間値
		
	6.備考
		
------------------------------------------------------------------------------------------*/
int CGlobalMath::fft(complex *x,int m)
{
 // static complex *w;     /* used to store the w complex array */
 // static int mstore = 0; /* stores m for future reference */
 // static int n = 1;      /* length of fft stored for future */
  complex *w = NULL;     /* used to store the w complex array */
  int mstore = 0; /* stores m for future reference */
  int n = 1;      /* length of fft stored for future */

  complex u,temp,tm;
  complex *xi,*xip,*xj,*wptr;

  int i,j,k,l,le,windex;

  double arg,w_real,w_imag,wrecur_real,wrecur_imag,wtemp_real;

  /* free previously allocated storage and set new m */

  if(m != mstore){
    if(mstore != 0) free(w);
    mstore = m;
    if(m == 0) return -1;

    /* n = 2**m = fft length */

    n = 1 << m;
    le = n/2;

    /* allocate the storage for w */

    w = (complex*) calloc(le-1,sizeof(complex));
    if(!w){
      return -1;
    }

    /* calculate the w values recursively */

    arg = 4.0*atan(1.0)/le;
    wrecur_real = w_real = cos(arg);
    wrecur_imag = w_imag = -sin(arg);
    xj = w;
    for(j = 1; j < le; j++){
      xj->re = (float)wrecur_real;
      xj->im = (float)wrecur_imag;
      xj++;
      wtemp_real = wrecur_real*w_real - wrecur_imag*w_imag;
      wrecur_imag = wrecur_real*w_imag + wrecur_imag*w_real;
      wrecur_real = wtemp_real;
    }
  }

  /* start fft */

  le = n;
  windex = 1;
  for(l = 0; l < m; l++){
    le = le/2;
  
    /* first iteration with no multiplies */

    for(i = 0; i < n; i=i+2*le){
      xi = x + i;
      xip = xi + le;
      temp.re = xi->re + xip->re;
      temp.im = xi->im + xip->im;
      xip->re = xi->re - xip->re;
      xip->im = xi->im - xip->im;
      *xi = temp;
    }

    /* remaining iterations use stored w */
    
    wptr = w + windex - 1;
    for(j = 1; j < le; j++){
      u = *wptr;
      for(i = j; i < n; i=i+2*le){
	xi = x + i;
	xip = xi + le;
	temp.re = xi->re + xip->re;
	temp.im = xi->im + xip->im;
	tm.re = xi->re - xip->re;
	tm.im = xi->im - xip->im;
	xip->re = tm.re*u.re - tm.im*u.im;
	xip->im = tm.re*u.im + tm.im*u.re;
	*xi = temp;
      }
      wptr = wptr + windex;
    }
    windex = 2*windex;
  }
  
  /* rearrange data by bit reversing */

  j = 0;
  for(i = 1; i < n-1; i++){
    k = n/2;
    while(k <= j){
      j = j - k;
      k = k/2;
    }
    j = j + k;
    if(i < j){
      xi = x + i;
      xj = x + j;
      temp = *xj;
      *xj = *xi;
      *xi = temp;
    }
  }

  free( w );
  return 0;
}
int CGlobalMath::ifft(complex *x,int m)
{
    int i;
    int dataSize = 1 << m;

    for (i = 0; i < dataSize; i++)
    {
        x[i].im = -x[i].im;
    }
    fft( x, m );
    for (i = 0; i < dataSize; i++)
    {
        x[i].re /= (double)dataSize;
        x[i].im /= (double)(-dataSize);
    }

	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		DFTを行う

	2.パラメタ説明
		x			複素数の時間信号
		nDataNum	データ数

	3.概要
		DFTを行う

	4.機能説明
		DFTを行う


	5.戻り値
		なし
		
	6.備考
		
------------------------------------------------------------------------------------------*/
int CGlobalMath::dft(complex *x, int nDataNum )
{
	int n,k;
	complex *F;

	F = new complex[nDataNum];

	for( k = 0; k < nDataNum; k++ )
	{
		F[k].re = F[k].im = 0.0;
		for( n = 0; n < nDataNum; n++ )
		{
			F[k].re += x[n].re*cos(-2.0*PI*(double)(k*n)/(double)nDataNum);
			F[k].im += x[n].re*sin(-2.0*PI*(double)(k*n)/(double)nDataNum);
		}
	}

	for( k = 0; k < nDataNum; k++ )
	{
		x[k].re = F[k].re;
		x[k].im = F[k].im;
	}

	delete [] F;

	return 0;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		逆DFTを行う

	2.パラメタ説明
		x			複素数の時間信号
		nDataNum	データ数

	3.概要
		逆DFTを行う

	4.機能説明
		逆DFTを行う


	5.戻り値
		なし
		
	6.備考
		
------------------------------------------------------------------------------------------*/
int CGlobalMath::idft(complex *x, int nDataNum )
{
	int n,k;
	complex *F;

	F = new complex[nDataNum];

	for( k = 0; k < nDataNum; k++ )
	{
		F[k].re = F[k].im = 0.0;
		for( n = 0; n < nDataNum; n++ )
		{
			F[k].re += ( x[n].re*cos(2.0*PI*(double)(k*n)/(double)nDataNum) - x[n].im*sin(2.0*PI*(double)(k*n)/(double)nDataNum) ) / (double)nDataNum;
			//	虚部は怪しい
//			F[k].im += ( x[n].re*sin(2.0*PI*(double)(k*n)/(double)nDataNum) + x[n].re*sin(2.0*PI*(double)(k*n)/(double)nDataNum) ) / (double)nDataNum;
		}
	}

	for( k = 0; k < nDataNum; k++ )
	{
		x[k].re = F[k].re;
		x[k].im = F[k].im;
	}

	delete [] F;


	return 0;
}


// ビットを左右反転した配列を返す
int* CGlobalMath::BitScrollArray(int arraySize)
{
    int i, j;
    int* reBitArray = (int*)calloc(arraySize, sizeof(int));
    int arraySizeHarf = arraySize >> 1;

    reBitArray[0] = 0;
    for (i = 1; i < arraySize; i <<= 1)
    {
        for (j = 0; j < i; j++)
            reBitArray[j + i] = reBitArray[j] + arraySizeHarf;
        arraySizeHarf >>= 1;
    }
    return reBitArray;
}

// FFT
void CGlobalMath::FFT2(double* inputRe, double* inputIm, double* outputRe, double* outputIm, int bitSize)
{
    int i, j, stage, type;
    int dataSize = 1 << bitSize;
    int butterflyDistance;
    int numType;
    int butterflySize;
    int jp;
    int* reverseBitArray = BitScrollArray(dataSize);
    double wRe, wIm, uRe, uIm, tempRe, tempIm, tempWRe, tempWIm;

    // バタフライ演算のための置き換え
    for (i = 0; i < dataSize; i++)
    {
        outputRe[i] = inputRe[reverseBitArray[i]];
        outputIm[i] = inputIm[reverseBitArray[i]];
    }

    // バタフライ演算
    for (stage = 1; stage <= bitSize; stage++)
    {
        butterflyDistance = 1 << stage;
        numType = butterflyDistance >> 1;
        butterflySize = butterflyDistance >> 1;

        wRe = 1.0;
        wIm = 0.0;
        uRe = cos(PI / butterflySize);
        uIm = -sin(PI / butterflySize);

        for (type = 0; type < numType; type++)
        {
            for (j = type; j < dataSize; j += butterflyDistance)
            {
                jp = j + butterflySize;
                tempRe = outputRe[jp] * wRe - outputIm[jp] * wIm;
                tempIm = outputRe[jp] * wIm + outputIm[jp] * wRe;
                outputRe[jp] = outputRe[j] - tempRe;
                outputIm[jp] = outputIm[j] - tempIm;
                outputRe[j] += tempRe;
                outputIm[j] += tempIm;
            }
            tempWRe = wRe * uRe - wIm * uIm;
            tempWIm = wRe * uIm + wIm * uRe;
            wRe = tempWRe;
            wIm = tempWIm;
        }
    }
}

// 1次元IFFT
void CGlobalMath::IFFT2(double* inputRe, double* inputIm, double* outputRe, double* outputIm, int bitSize)
{
    int i;
    int dataSize = 1 << bitSize;

    for (i = 0; i < dataSize; i++)
    {
        inputIm[i] = -inputIm[i];
    }
    FFT2(inputRe, inputIm, outputRe, outputIm, bitSize);
    for (i = 0; i < dataSize; i++)
    {
        outputRe[i] /= (double)dataSize;
        outputIm[i] /= (double)(-dataSize);
    }
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		最小二乗法により、近似平面を求める

	2.パラメタ説明
		pdX			(IN)	入力するX座標の配列
		pdY			(IN)	入力するY座標の配列
		pdZ			(IN)	入力するZ座標の配列
		niNum		(IN)	X,Y,Zのデータ数
		pa			(OUT)	近似式の係数a
		pb			(OUT)	近似式の係数b
		pd			(OUT)	近似式の係数d

	3.概要
		近似式は z = ax + by + dとなる

	4.機能説明
		技術資料 819-11461参照

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::SurfaceApproxByLeastSquaresMethod( double *pdX, double *pdY, double *pdZ, int niNum,
										   double *pa, double *pb, double *pd )
{
	int i_loop;
	double d_wrkX = 0.0;
	double d_wrkY = 0.0;
	double d_wrkXY = 0.0;
	double d_wrkXX = 0.0;
	double d_wrkYY = 0.0;
	double d_wrkZX = 0.0;
	double d_wrkZY = 0.0;
	double d_wrkZ = 0.0;

	for( i_loop = 0; i_loop < niNum; i_loop++ )
	{
		d_wrkX += pdX[i_loop];
		d_wrkY += pdY[i_loop];
		d_wrkXY += pdX[i_loop] * pdY[i_loop];
		d_wrkXX += pdX[i_loop] * pdX[i_loop];
		d_wrkYY += pdY[i_loop] * pdY[i_loop];
		d_wrkZX +=  pdZ[i_loop] * pdX[i_loop];
		d_wrkZY +=  pdZ[i_loop] * pdY[i_loop];
		d_wrkZ +=  pdZ[i_loop];
	}

	double matrix_A[9];
	double matrix_inverseA[9];
	double matrix_B[3];
	matrix_A[0] = d_wrkXX;		matrix_A[1] = d_wrkXY;		matrix_A[2] = d_wrkX;
	matrix_A[3] = d_wrkXY;		matrix_A[4] = d_wrkYY;		matrix_A[5] = d_wrkY;
	matrix_A[6] = d_wrkX;		matrix_A[7] = d_wrkY;		matrix_A[8] = (double)niNum;

	matrix_B[0] = d_wrkZX;		matrix_B[1] = d_wrkZY;		matrix_B[2] = d_wrkZ;	
	
	// A-1を求める
	InverseMatrix( matrix_A, matrix_inverseA, 3 );

	*pa = matrix_inverseA[0] * matrix_B[0] + matrix_inverseA[1] * matrix_B[1] + matrix_inverseA[2] * matrix_B[2];
	*pb = matrix_inverseA[3] * matrix_B[0] + matrix_inverseA[4] * matrix_B[1] + matrix_inverseA[5] * matrix_B[2];
	*pd = matrix_inverseA[6] * matrix_B[0] + matrix_inverseA[7] * matrix_B[1] + matrix_inverseA[8] * matrix_B[2];

	return 0;
}







//****************************************************************************************

//*********************************************//
//											   //
//	以下画像データ(2次元データ)に関する機能    //
//											   //
//*********************************************//

//****************************************************************************************





/*------------------------------------------------------------------------------------------
	1.日本語名
		相関係数を求める関数

	2.パラメタ説明
		niModelImage	(IN)	相関係数を求める画像(基準とする画像。パターンマッチングなら、モデル画像)の輝度配列
		niImage			(IN)	相関係数を求める画像の輝度配列
		niSize			(IN)	配列サイズ


	3.概要
		相関係数を求める。パターンマッチングならば、この係数の値をスコアにする
		(-1～1の間の数値となる。-1:逆相関　1:完全相関)

		画像を１次元配列として扱っているので注意。

	4.機能説明
		なし

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CGlobalMath::getCorrelationCoefficient( int niModelImage[], int niImage[], int niSize )
{
	int i_loop;
	double  sigma_m = 0.0;	//	Σm
	double  sigma_i = 0.0;	//	Σi
	double  sigma_im= 0.0;	//	Σim
	double  sigma_ii = 0.0;	//	Σii
	double  sigma_mm = 0.0;	//	Σmm
	double bunsi = 0.0;
	double bunbo = 0.0;

	double soukan_r;	//	相関係数

	//	Σm、Σi、Σim、Σii、Σmmを求める
	for( i_loop = 0; i_loop < niSize; i_loop++ )
	{
		sigma_m += ( double )niModelImage[i_loop];
		sigma_i += ( double )niImage[i_loop];
		sigma_im += ( double )( niModelImage[i_loop] * niImage[i_loop] );
		sigma_ii += ( double )( niImage[i_loop] * niImage[i_loop] );
		sigma_mm += ( double )( niModelImage[i_loop] * niModelImage[i_loop] );
	}


	//	r =(N * Σim - Σm * Σi)/ sqrt( (N * Σii - (Σi)2) *  (N * Σmm - (Σm)2) ) を求める
	//	まず分子
	bunsi = ( double )( niSize * sigma_im - sigma_m * sigma_i );
	//	分母
	bunbo = sqrt( ( double )( ( niSize * sigma_ii - sigma_i * sigma_i ) * ( niSize * sigma_mm - sigma_m * sigma_m ) ) );

	//	相関を求める
	soukan_r = bunsi / bunbo;

	return soukan_r;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		画像のフーリエ変換を行う関数

	2.パラメタ説明
		niInputImage	(IN)	入力画像
		niWidth			(IN)	画像の幅
		niHeight		(IN)	画像の高さ
		ndMagnitude		(OUT)	変換後の振幅データ( √(R*R + I*I) )  logで出力する

	3.概要
		画像のフーリエ変換を行う。２次元のFFTはよくわからないので、通常のDFTで行う
		FFTは２のべき乗の高さand幅でないと出来ないかも？なのでDFTでいいのかな
		ただし、FFTと違い、計算量が半端なく多いので、時間が非常にかかる

	4.機能説明
		配列は X[height][witdth]の形とする
		求まった変換後のデータは、画像が斜めに反転してるので直したい場合は、アプリ側で直す

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::FourierTransformDFT( int *niInputImage[], int niWidth, int niHeight, double *ndMagnitude[] )
{
	int	i_loop_width;
	int	i_loop_height;
	int i_W;
	int i_H;
	int i_loop;
	complex	d_f1;
	complex **d_F;

	//	2次元配列用メモリを確保
	d_F = new complex *[niHeight];
	for( i_loop=0; i_loop < niHeight; i_loop++ ) 
	{
		d_F[i_loop] = new complex [niWidth];
	}


	for( i_H = 0; i_H < niHeight; i_H++ )
	{
		for( i_W = 0; i_W < niWidth; i_W++ )
		{
			d_f1.re = 0.0;
			d_f1.im = 0.0;
			//	これはΣ内
			for( i_loop_height = 0; i_loop_height < niHeight; i_loop_height++ )
			{
				for( i_loop_width = 0; i_loop_width < niWidth; i_loop_width++ )
				{
					d_f1.re += (double)niInputImage[i_loop_height][i_loop_width] * cos( -2.0 * PI * ( (double)i_loop_width * (double)i_W / (double)niWidth + (double)i_loop_height * (double)i_H / (double)niHeight ) );
					d_f1.im += (double)niInputImage[i_loop_height][i_loop_height] * sin( -2.0 * PI * ( (double)i_loop_width * (double)i_W / (double)niWidth + (double)i_loop_height * (double)i_H / (double)niHeight ) );
				}
			}
			d_F[i_H][i_W] = d_f1;
		}
	}

	//	振幅を求める
	for( i_loop_height = 0; i_loop_height < niHeight; i_loop_height++ )
	{
		for( i_loop_width = 0; i_loop_width < niWidth; i_loop_width++ )
		{
			ndMagnitude[i_loop_height][i_loop_width] = pow( d_F[i_loop_height][i_loop_width].re, 2.0 ) + pow( d_F[i_loop_height][i_loop_width].im, 2.0 );
			ndMagnitude[i_loop_height][i_loop_width] = ndMagnitude[i_loop_height][i_loop_width] / ( niWidth * niHeight );
			ndMagnitude[i_loop_height][i_loop_width] = 10.0 * log10( ndMagnitude[i_loop_height][i_loop_width] + 1.e-14 );
			if( ndMagnitude[i_loop_height][i_loop_width] < 0.0 )
			{
				ndMagnitude[i_loop_height][i_loop_width] = 0.0;
			}
		}
	}

	//	メモリ解放
	for( i_loop = 0; i_loop < niHeight; i_loop++ ) 
	{
		delete [] d_F[i_loop];
	}
	delete [] d_F;

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		背景画像差分を求める関数

	2.パラメタ説明
		niInputImage	(IN/OUT)	n入力画像/差分画像
		niBackImage		(IN)		背景画像
		niSize			(IN)		配列サイズ

	3.概要
		背景画像差分を求める関数
		差分画像 = | 入力画像 - 背景画像|  とする

	4.機能説明
		なし	

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::DifferenceImageFromBackGround( int niInputImage[], int niBackImage[], int niSize )
{
	int i_loop;

	for( i_loop = 0; i_loop < niSize; i_loop++ )
	{
		niInputImage[i_loop] = abs( niInputImage[i_loop] - niBackImage[i_loop] );
	}

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		係数を指定して2次元フィルタリングを行う関数

	2.パラメタ説明
		niInputImage	(IN)		入力画像
		ndFilter		(IN)		フィルタ係数
		niSize			(IN)		フィルタタップ数(N : N×N)
		ndOutputImage	(OUT)		出力画像
		niWidth			(IN)		画像の幅
		niHeight		(IN)		画像の高さ
	3.概要

	4.機能説明
		niInputImage[480][640]のような形式。縦、横の順	
		ndFilter[3][3]のような形。　縦、横の順

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::Filtering2Dim( int *niInputImage[], double ndFilter[][3], int niTap, double *ndOutputImage[], int niWidth, int niHeight )
{
	int i_loop;
	int i_loop_width;
	int i_loop_height;
	int i_loop_w;
	int i_loop_h;
	double **pd_part_image;
	double d_val;

	//	フィルタタップは奇数とする
	if( niTap % 2 == 0 )
	{
		return -1;
	}

	//	メモリ確保
	pd_part_image = new double *[niTap];
	for( i_loop=0; i_loop < niTap; i_loop++ ) 
	{
		pd_part_image[i_loop] = new double [niTap];
	}


	for( i_loop_height = 0; i_loop_height < niHeight; i_loop_height++ )
	{
		for( i_loop_width = 0; i_loop_width < niWidth; i_loop_width++ )
		{
			//	端っこのデータはフィルタリングできない
			if( ( i_loop_width - 1 < 0 || i_loop_width + 1 > niWidth - 1 ) ||
				( i_loop_height - 1 < 0 || i_loop_height + 1 > niHeight - 1 ) )
			{
				ndOutputImage[i_loop_height][i_loop_width] = (double)niInputImage[i_loop_height][i_loop_width];
			}
			//	フィルタリング出来るなら、データを抽出する
			else
			{
				for( i_loop_h = 0; i_loop_h < niTap; i_loop_h++ )
				{
					for( i_loop_w = 0; i_loop_w < niTap; i_loop_w++ )
					{					
						pd_part_image[i_loop_h][i_loop_w] = niInputImage[i_loop_height - 1 + i_loop_h][i_loop_width - 1 + i_loop_w];
					}
				}

				//	フィルタリング実行(畳み込み？）
				d_val = 0.0;
				for( i_loop_h = 0; i_loop_h < niTap; i_loop_h++ )
				{
					for( i_loop_w = 0; i_loop_w < niTap; i_loop_w++ )
					{					
						d_val += pd_part_image[i_loop_h][i_loop_w] * ndFilter[i_loop_h][i_loop_w];
					}
				}
				if( d_val < 0.0 )
				{
					ndOutputImage[i_loop_height][i_loop_width] = 0.0;
				}
				else
				{
					ndOutputImage[i_loop_height][i_loop_width] = d_val;
				}

			}
		}
	}

	//	メモリ解放
	for( i_loop = 0; i_loop < niTap; i_loop++ ) 
	{
		delete [] pd_part_image[i_loop];
	}
	delete [] pd_part_image;

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		アフィン変換を行う関数

	2.パラメタ説明
		niInputImage	(IN)		入力画像 (変換前)
		ndOutputImage	(OUT)		出力画像 (変換後)
		niWidthInput    (IN)		画像の幅 (入力画像)
		niHeightInput   (IN)		画像の高さ(入力画像)
		niWidthOutput   (IN)		画像の幅 (出力画像)
		niHeightOutput  (IN)		画像の高さ(出力画像)
		ndA				(IN)		係数a
		ndB				(IN)		係数b
		ndC				(IN)		係数c
		ndD				(IN)		係数d
		ndE				(IN)		係数e
		ndF				(IN)		係数f
	3.概要

	 | X' |   | a b c | | x |
	 | Y' | = | d e f |*| y |
	 | 1  |   | 0 0 1 | | 1 |
	(X',Y'):変換後の座標
	(x,y)：変換前の座標
 
    このままでは使えないので、下のように式を変換する

	 | x  |   | a b c |-1 | X' |
	 | y  | = | d e f |*  | Y' |
	 | 1  |   | 0 0 1 |   | 1  |
    
    つまり、変換後の座標(X',Y')に対応する変換前の画像の座標(x,y)を求める
    そのとき、x,yが整数にならない場合が多いので、隣接する座標からその座標の輝度値を求める
	ニアレストネイバー、バイリニア、バイキュービック等
    

	4.機能説明
		niInputImage[480][640]のような形式。縦、横の順	
		ndFilter[3][3]のような形。　縦、横の順

	5.戻り値
		0  :	OK
		-1 :    エラー
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::AffineTranslation( int *niInputImage[], int *niOutputImage[], int niWidthInput, int niHeightInput,
									int niWidthOutput,	int niHeightOutput,	
									double ndA,  double ndB, double ndC, double ndD, double ndE, double ndF )
{
	int i_loop_height;
	int i_loop_width;
	double X,Y;
	double d_pixel_val;
	double alpha, beta;
	int u,v;

	double matrix_A[9];
	double matrix_inverseA[9];
	//	パラメーターを3×3行列に変換
	matrix_A[0] = ndA;		matrix_A[1] = ndB;		matrix_A[2] = ndC;
	matrix_A[3] = ndD;		matrix_A[4] = ndE;		matrix_A[5] = ndF;
	matrix_A[6] = 0.0;		matrix_A[7] = 0.0;		matrix_A[8] = 1.0;
	// A-1を求める
	InverseMatrix( matrix_A, matrix_inverseA, 3 );


	for( i_loop_height = 0; i_loop_height < niHeightOutput; i_loop_height++ )
	{
		for( i_loop_width = 0; i_loop_width < niWidthOutput; i_loop_width++ )
		{
			//	まず変換式から変換後の座標に対応する変換前の画像の座標を取得する
			//	X =A-1 * X'の　式から
			X = i_loop_width * matrix_inverseA[0] + i_loop_height * matrix_inverseA[1] + matrix_inverseA[2];
			Y = i_loop_width * matrix_inverseA[3] + i_loop_height * matrix_inverseA[4] + matrix_inverseA[5];

			//	対応する座標が画像外に外れていないかチェック
			if( X > ( double )( niWidthInput - 1 ) || X < 0.0 || 
				Y > ( double )( niHeightInput -1 ) || Y < 0.0 )
			{
				//	外れている場合は一番近い画素値にする
				u = (int)X;
				if( X > ( double )( niWidthInput - 1 ) )
				{
					u =  niWidthInput - 1;
				}
				else if( X < 0.0 )
				{
					u = 0;
				}
				v = (int)Y;
				if( Y > ( double )( niHeightInput -1 ) )
				{
					v = niHeightInput -1;
				}
				else if( Y < 0.0 )
				{
					v = 0;
				}
				niOutputImage[i_loop_height][i_loop_width] = niInputImage[v][u];
				//	外れている場合は0にする
				niOutputImage[i_loop_height][i_loop_width] = 0;
				
			}
			else
			{
				//	外れてなければその座標の輝度値を求める。ただし、X、Yは整数とは限らないので周辺4格子から近似値を求める
				alpha = X - (int)X;
				beta  = Y - (int)Y;
				u = (int)X;
				v = (int)Y;
				//	画像の一番外側の場合は周辺値が使用できないのでそのままの値を使用する
//				if( u ==  niWidthInput - 1 || u == 1 ||
//				     v ==  niHeightInput - 1 || v == 1 )
				if( u ==  niWidthInput - 1 || v ==  niHeightInput - 1 )
				{
					niOutputImage[i_loop_height][i_loop_width] = niInputImage[v][u];
				}
				else
				{
					//	バイリニア法
					d_pixel_val = ( 1.0 - alpha )*( 1.0 - beta ) * niInputImage[v][u] + alpha * ( 1.0 - beta ) * niInputImage[v][u + 1] +
									beta * ( 1.0 - alpha ) * niInputImage[v + 1][u] + beta * alpha * niInputImage[v + 1][u + 1];
					//	整数にする
					niOutputImage[i_loop_height][i_loop_width] = ( int )d_pixel_val;
				}
			}
		}
	}

	return 0;
}


/*------------------------------------------------------------------------------------------
	1.日本語名
		メディアンフィルタをかける(3×3限定)

	2.パラメタ説明
		npImageArray	モノクロ1次元配列(左上から右方向への順でデータは格納されている)
		niWidth			画像の幅
		niHeight		画像の高さ

	3.概要
		メディアンフィルタをかける

	4.機能説明
		メディアンフィルタをかける

	5.戻り値
		0
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CGlobalMath::FilterMedian( unsigned short *npImageArray, int niWidth, int niHeight )
{
	int i_loop;
	int	i_data_num;
	unsigned short *p_output_data;
	unsigned short sort_data[9];
	const int ci_max_radix = 100;

	i_data_num = niWidth * niHeight;

	p_output_data = new unsigned short[i_data_num];

	//	全画素でメディアン。画像の端の画素は4つ(6つ)の中で2,3番目(3,4番目)のデータの平均とする
	for( i_loop = 0; i_loop < i_data_num; i_loop++ )
	{
		//	左上の角
		if( i_loop == 0 )
		{
			sort_data[0] = npImageArray[0];
			sort_data[1] = npImageArray[1];
			sort_data[2] = npImageArray[niWidth];
			sort_data[3] = npImageArray[niWidth];
			//	ソート
			RadixSort( sort_data, 4, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[1] + sort_data[2] ) / 2;
		}
		//	左上
		else if( i_loop == niWidth - 1 )
		{
			sort_data[0] = npImageArray[i_loop - 1];
			sort_data[1] = npImageArray[i_loop];
			sort_data[2] = npImageArray[niWidth + i_loop - 1];
			sort_data[3] = npImageArray[niWidth + i_loop];
			//	ソート
			RadixSort( sort_data, 4, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[1] + sort_data[2] ) / 2;
		}
		//	左下
		else if( i_loop == niWidth * ( niHeight - 1 ) )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop + 1];
			sort_data[2] = npImageArray[i_loop - niWidth];
			sort_data[3] = npImageArray[i_loop - niWidth + 1];
			//	ソート
			RadixSort( sort_data, 4, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[1] + sort_data[2] ) / 2;
		}
		//	右下
		else if( i_loop == niWidth * niHeight - 1  )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop - 1];
			sort_data[2] = npImageArray[i_loop - niWidth];
			sort_data[3] = npImageArray[i_loop - niWidth - 1];
			//	ソート
			RadixSort( sort_data, 4, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[1] + sort_data[2] ) / 2;
		}
		//	一番上の行(角除く)
		else if( i_loop < niWidth )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop - 1];
			sort_data[2] = npImageArray[i_loop + 1];
			sort_data[3] = npImageArray[niWidth + i_loop];
			sort_data[4] = npImageArray[niWidth + i_loop - 1];
			sort_data[5] = npImageArray[niWidth + i_loop + 1];
			//	ソート
			RadixSort( sort_data, 6, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[2] + sort_data[3] ) / 2;
		}
		//	一番下の行(角除く)
		else if( i_loop > niWidth * ( niHeight - 1 ) )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop - 1];
			sort_data[2] = npImageArray[i_loop + 1];
			sort_data[3] = npImageArray[i_loop - niWidth];
			sort_data[4] = npImageArray[i_loop - 1 - niWidth];
			sort_data[5] = npImageArray[i_loop + 1 - niWidth];
			//	ソート
			RadixSort( sort_data, 6, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[2] + sort_data[3] ) / 2;
		}
		//	一番左の列(角除く)
		else if( i_loop % niWidth == 0 )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop + 1];
			sort_data[2] = npImageArray[i_loop - niWidth];
			sort_data[3] = npImageArray[i_loop - niWidth + 1];
			sort_data[4] = npImageArray[i_loop + niWidth];
			sort_data[5] = npImageArray[i_loop + niWidth + 1];
			//	ソート
			RadixSort( sort_data, 6, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[2] + sort_data[3] ) / 2;
		}
		//	一番右の列(角除く)
		else if( i_loop % niWidth == niWidth - 1 )
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop - 1];
			sort_data[2] = npImageArray[i_loop + niWidth];
			sort_data[3] = npImageArray[i_loop + niWidth - 1];
			sort_data[4] = npImageArray[i_loop - niWidth];
			sort_data[5] = npImageArray[i_loop - niWidth - 1];
			//	ソート
			RadixSort( sort_data, 6, ci_max_radix );
			p_output_data[i_loop] = ( sort_data[2] + sort_data[3] ) / 2;
		}
		//	その他画像の内側全部
		else
		{
			sort_data[0] = npImageArray[i_loop];
			sort_data[1] = npImageArray[i_loop + 1];
			sort_data[2] = npImageArray[i_loop - 1];
			sort_data[3] = npImageArray[i_loop + niWidth];
			sort_data[4] = npImageArray[i_loop + niWidth + 1];
			sort_data[5] = npImageArray[i_loop + niWidth - 1];
			sort_data[6] = npImageArray[i_loop - niWidth];
			sort_data[7] = npImageArray[i_loop - niWidth + 1];
			sort_data[8] = npImageArray[i_loop - niWidth - 1];
			//	ソート
			RadixSort( sort_data, 9, ci_max_radix );
			p_output_data[i_loop] = sort_data[4];
		}
	}

	//	データをコピーする
	memcpy( npImageArray, p_output_data, sizeof( unsigned short ) * i_data_num );

	delete [] p_output_data;
}
/*------------------------------------------------------------------------------------------
	1.日本語名
		基数ソートアルゴリズムを実行する

	2.パラメタ説明
		npiData			ソートするデータ(ソート後のデータ)
		niDataNum		データ数
		niMaxRadix		最大基数(10,100,1000など。255までのデータなら3桁なので100を指定すればよい)

	3.概要
		基数ソートアルゴリズムを実行する

	4.機能説明
		基数ソートアルゴリズムを実行する
		このアルゴリズムは整数のみにしか使えないので注意

	5.戻り値
		0
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CGlobalMath::RadixSort( unsigned short *npiData, unsigned short niDataNum, int niMaxRadix )
{
	unsigned short rad[9];    /* 基数をしまう配列  */
	unsigned short y[9];      /* 作業用配列 */
    int i, j, k;                        /* niMaxRadix：基数を取り出す最大値 */
    int m = 1;                          /* 基数を取り出す桁 */

    while (m <= niMaxRadix) {
        for (i = 0; i < niDataNum; i++)
            rad[i] = (npiData[i] / m) % 10;   /* 基数を取り出し rad[i] に保存 */

        k = 0;                          /* 配列の添字として使う */
        for (i = 0; i <= 9; i++)        /* 基数は 0 から 9 */
            for (j = 0; j < niDataNum; j++)
                if (rad[j] == i)        /* 基数の小さいものから */
                    y[k++] = npiData[j];      /* y[ ] にコピー */

        for (i = 0; i < niDataNum; i++)
            npiData[i] = y[i];                /* x[ ] にコピーし直す */
				
        m *= 10;                        /*  基数を取り出す桁を一つ上げる */
    }
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		相関パターンマッチングを実行

	2.パラメタ説明
		niModelImage			モデル画像(モノクロ1次元配列)
		niImage					パターンマッチング対象画像(モノクロ1次元配列)
		niModelWidth			モデル画像の幅
		niModelHeight			モデル画像の高さ
		niImageWidth			パターンマッチング対象画像の幅
		niImageHeight			パターンマッチング対象画像の高さ
		niPatternPosX			パターンマッチング結果座標(モデルの左上の座標)
		niPatternPosY			パターンマッチング結果座標(モデルの左上の座標)
	3.概要
		相関パターンマッチングを実行

	4.機能説明
		相関パターンマッチングを実行

	5.戻り値
		最大のスコアを返す(-1.0～1.0 )
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CGlobalMath::PatternMatchingByCorrelation( int niModelImage[], int niImage[], int niModelWidth, int niModelHeight,
										 int niImageWidth, int niImageHeight, int* niPatternPosX, int* niPatternPosY )
{
	int i_loop;
	int i_loop2;
	int i_loop_pattern;
	int i_loop2_pattern;
	int i_offset_index;
	int i_size = niModelHeight * niModelWidth;
	int* i_pattern_matching_area = new int[niModelWidth*niModelHeight];
	double d_max_score = -9.9e10;
	double d_score;

	//画像の左上から右下までスキャンしていく
	for (i_loop = 0; i_loop < niImageHeight; i_loop++)
	{
		for (i_loop2 = 0; i_loop2 < niImageWidth; i_loop2++)
		{
			//　モデル画像のサイズ分のデータを確保出来ない場合は何もしない(画面から外れる場合)
			if( (i_loop2 + niModelWidth > niImageWidth - 1) || (i_loop + niModelHeight > niImageHeight - 1) )
			{
				continue;
			}

			//  それ以外なら実行
			//  画像からパターンマッチング領域のデータを取り、一次配列に格納する
			i_offset_index = niImageWidth * i_loop + i_loop2;

			for (i_loop_pattern = 0; i_loop_pattern < niModelHeight; i_loop_pattern++)
			{
				for (i_loop2_pattern = 0; i_loop2_pattern < niModelWidth; i_loop2_pattern++)
				{
					i_pattern_matching_area[niModelWidth * i_loop_pattern + i_loop2_pattern]
						= niImage[i_offset_index + niImageWidth * i_loop_pattern + i_loop2_pattern];
				}
			}

			//  パターンマッチング
            d_score = getCorrelationCoefficient(niModelImage, i_pattern_matching_area, i_size );

			if (d_max_score < d_score)
			{
				d_max_score = d_score;
				*niPatternPosX = i_loop2;
				*niPatternPosY = i_loop;
			}
		}
	}

	delete[] i_pattern_matching_area;


	return d_max_score;
}


/*------------------------------------------------------------------------------------------
	1.日本語名
		データの標準偏差(σ)を求める

	2.パラメタ説明
		npdData			データ群
		niDataNum		データ数

	3.概要
		データの標準偏差(σ)を求める

	4.機能説明
		データの標準偏差(σ)を求める

		σ^2 = Σx^2 / N - m*m  (m:平均) から求める

	5.戻り値
		標準偏差(σ)
		
	6.備考
		なし
------------------------------------------------------------------------------------------*/
double CGlobalMath::getStandardDeviation( double *npdData, int niDataNum )
{
	int i_loop;
	double d_sum = 0.0;
	double d_sumv = 0.0;
	double d_mean;
	double d_ret;

	for( i_loop = 0; i_loop < niDataNum; i_loop++ )
	{
		d_sum += npdData[i_loop];
		d_sumv += npdData[i_loop] * npdData[i_loop];
	}
	d_mean = d_sum / (double)niDataNum;

	d_ret = ( d_sumv / niDataNum ) - ( d_mean * d_mean );
	d_ret = sqrt( d_ret );

	return d_ret;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		ローパスフィルタを作成する

	2.パラメタ説明
		npdFilterTap	作成されたフィルタ
		fs				サンプリング周波数
		f				カットオフ周波数
		niTap			フィルタのタップ数
		niWindow		窓関数
	3.概要
		ローパスフィルタを作成する

	4.機能説明
		窓関数法でローパスフィルタを作成する

	5.戻り値
		0 :OK
		-1:NG

	6.備考
		なし
------------------------------------------------------------------------------------------*/
int CGlobalMath::makeLowPassFilter( double *npdFilterTap, double fs, double f, int niTap, int niWindow )
{
	const int N = 1000;
	int i_loop;
	int i_ideal_cutoff;
	int i_ideal[N];
	int n,k,m;
	double W[20000];

	complex	idft_data[N];

//	//	タップ数が偶数ならエラー
//	if( niTap % 2 == 0 )
//	{
//		return -1;
//	}

	//	まず理想的な周波数特性のデータ作成

	//	データは1000で作成する
	//	1となる周波数は、f*N/fsで求められる

	//	もしf=2Hz, fs=8Hzなら
	//  1 (0 < k < 249)
	//  0 (250 < k < 499 )
	//  0 (500 < k < 749 )
	//  1 (750 < k < 999 )
	//	となる  500以降は折り返しとする必要あり

	i_ideal_cutoff = ( int )( ( f * ( double )N )/ fs ); 

	for( i_loop = 0; i_loop < N; i_loop++ )
	{
		if( i_loop < i_ideal_cutoff )
		{
			i_ideal[i_loop] = 1;
		}
		else if( i_loop >= N - i_ideal_cutoff )
		{
			i_ideal[i_loop] = 1;
		}
		else
		{
			i_ideal[i_loop] = 0;
		}

	}

	//	次にこのデータを逆フーリエ変換して、インパルス応答を求める

	/* 逆フーリエ変換 */
	for(n=0; n<N; n++)
	{
		idft_data[n].re = idft_data[n].im =0.0;
		for(k=0; k<N; k++)
		{
			idft_data[n].re += i_ideal[k]*cos(2.0*PI* (float)(k*n) /(float)N) /(float)N;
			idft_data[n].im += i_ideal[k]*sin(2.0*PI* (float)(k*n) /(float)N) /(float)N;
		}
	}

  // 窓関数をかけて、データを切り出す
  /* 窓関数データ作成 */
	for(n=0; n<niTap; n++)
	{

		if( niWindow == 0 )
		{
			//	方形窓
			W[n] = 1.00;
		}
		else if( niWindow == 1 )
		{
			//	ハニング窓
			W[n] = 0.50 -0.50*cos(2.0*PI*(float)n/(float)(niTap-1));
		}
		else if( niWindow == 2 )
		{
			//	ハミング窓
			W[n] = 0.54 -0.46*cos(2.0*PI*(float)n/(float)(niTap-1));
		}
		else
		{
			//	ブラックマン窓
			W[n] = 0.42 -0.50*cos(2.0*PI*(float)n/(float)(niTap-1)) +0.08*cos(4.0*PI*(float)n/(float)(niTap-1));
		}

//    /* 1.方形窓 */
//    W[n] = 1.00;

//    /* 2.ハニング窓 */
//      W[n] = 0.50 -0.50*cos(2.0*PI*(float)n/(float)(niTap-1));

//    /* 3.ハミング窓 */
//    W[n] = 0.54 -0.46*cos(2.0*PI*(float)n/(float)(niTap-1));

//    /* 4.ブラックマン窓 */
 //   W[n] = 0.42 -0.50*cos(2.0*PI*(float)n/(float)(niTap-1)) +0.08*cos(4.0*PI*(float)n/(float)(niTap-1));
	}

	//データ数M(奇数)だけ中心から切り出す
	for(n=0; n<niTap; n++)
	{
		m=(n+N-(niTap-1)/2)%N; /* シフトして切り出し */
		npdFilterTap[n] = W[n]*idft_data[m].re;
	}

/*
//	ファイル出力
CString str_buff;
CStdioFile p_file("C:\\temp\\data\\filter.csv", CFile::modeCreate | CFile::modeWrite| CFile::shareExclusive| CFile::typeText);

  for( int i_loop = 0; i_loop < niTap; i_loop++ )
  {

	str_buff.Format( "%f\n", npdFilterTap[i_loop] );
	p_file.WriteString( str_buff );
  }

p_file.Close();
//	ここまでファイル出力
*/

	return 0;
}

/*------------------------------------------------------------------------------------------
	1.日本語名
		フィルタリングを実行する

	2.パラメタ説明
		npdFilterTap	作成されたフィルタ
		fs				サンプリング周波数
		f				カットオフ周波数
		niTap			フィルタのタップ数

	3.概要
		フィルタリングを実行する

	4.機能説明
		窓関数法でローパスフィルタを作成する

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CGlobalMath::filtering( complex *InputData, int DataNum, double *npdFilterTap, int niTap )
{
	int k,n;
	complex *output;

	output = new complex[DataNum];


	// 畳み込みを行う
	for(n=0; n<DataNum; n++)
	{
		output[n].re=0.0;
		output[n].im=0.0;
		for(k=0; k<niTap; k++)
		{
			if(n-k >=0)
			{
				output[n].re +=npdFilterTap[k]*InputData[n-k].re;
			}
		}
    }

	for( int i_loop = 0; i_loop < DataNum; i_loop++ )
	{
		InputData[i_loop] = output[i_loop];
	}

/*
//	ファイル出力
CString str_buff;
CStdioFile p_file("C:\\temp\\data\\Filtering.csv", CFile::modeCreate | CFile::modeWrite| CFile::shareExclusive| CFile::typeText);

  for( int i_loop = 0; i_loop < DataNum; i_loop++ )
  {
	str_buff.Format( "%.5f\n", output[i_loop].re );
	p_file.WriteString( str_buff );
  }

p_file.Close();
//	ここまでファイル出力
*/
	delete [] output;


}

/*------------------------------------------------------------------------------------------
	1.日本語名
		フィルタの周波数特性を求める

	2.パラメタ説明
		npdFilterTap	作成されたフィルタ
		niTap			フィルタのタップ数

	3.概要
		フィルタの周波数特性を求める

	4.機能説明
		フィルタの周波数特性を求める

	5.戻り値
		なし

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CGlobalMath::filterCharacteristic(  double *npdFilterTap, int niTap )
{
	const int N = 1024;
	int i_loop;
	double implus[N];
	complex y[N];
	int k,n;
//	double PSD;

	//	まずインパルス信号を作成する
	for( i_loop = 0; i_loop < N; i_loop++ )
	{
		if( i_loop == 0 )
		{
			implus[i_loop] = 1.0;
		}
		else
		{
			implus[i_loop] = 0.0;
		}
	 }

	// 畳み込みを行う
	for(n=0; n<N; n++)
	{
		y[n].re=0.0;
		y[n].im=0.0;
		for(k=0; k<niTap; k++)
		{
			if(n-k >=0)
			{
				y[n].re +=npdFilterTap[k]*implus[n-k];
			}
		}
	}


	// フーリエ変換する
	fft( y, 10 );

/*
	//パワースペクトラムにしてファイル出力
CString str_buff;
CStdioFile p_file("C:\\temp\\data\\Filterfft.csv", CFile::modeCreate | CFile::modeWrite| CFile::shareExclusive| CFile::typeText);

  for( int i_loop = 0; i_loop < N; i_loop++ )
  {
    PSD  = (y[i_loop].re*y[i_loop].re+y[i_loop].im*y[i_loop].im) /(double)(N*N);
    PSD  = 10.0*log10(PSD +1.e-14);

	str_buff.Format( "%.5f\n", PSD );
	p_file.WriteString( str_buff );
  }
p_file.Close();
*/
}



/*------------------------------------------------------------------------------------------
	1.日本語名
		白色干渉計に最適なローパスフィルタのパラメーターを計算する

	2.パラメタ説明
		ndPitchUM		サンプリングピッチ(μm)
		npdFs			求まったサンプリング周波数
		npdFc			求まったカットオフ周波数
		npiWindow		求まった窓関数
		npiTapNum		求まったタップ数
	3.概要
		白色干渉計に最適なローパスフィルタのパラメーターを計算する

	4.機能説明
		現状、75nmピッチで測定する際のLPFパラメーターは
		fs=512
		fc=20
		tap=21
		window=3
		で問題なく測定できている。よってこれを基準として設計する

		明暗の1周期(約300nm)の間にサンプリングデータがいくつあるかでサンプリング周波数は決める。
		計算式はfs=300/ピッチ(nm)×300とする。
		よって、75nmピッチの場合は4×300で1200となる。
		そして、
		fs=512
		fc=20
		でうまくいっていることを考えて、fcを決めると
		20/512×1200=46.8となる。よってfc=46.8とする。
		そして、300基準とするので、fcは常に一定となり、46.8となる。


	5.戻り値
		0 :OK
		-1:NG

	6.備考
		なし
------------------------------------------------------------------------------------------*/
void CGlobalMath::calcLowPassFilterParameter( double ndPitchUM, double *npdFs, double *npdFc, int *npiWindow, int *npiTapNum )
{
	const double cd_Fc = 46.8;		//	カットオフ周波数は固定
	const int	 ci_window = 3;		//	窓関数は固定
	double d_Fs;
	int    i_tap;

	//	サンプリング周波数を求める
	d_Fs = ( 0.3 / ndPitchUM ) * 300.0;


	//	タップ数を決める
	//	タップ数は多いほうがよりよいフィルタとなるが、計算時間がかかるのであまり長く出来ない。
	//	どうしようか
	
	//	まずは適当

	//	75nm以上
	if( ndPitchUM >= 0.075 )
	{
		i_tap = 21;
	}
	//	50～75nm
	else if( ndPitchUM >= 0.05 && ndPitchUM < 0.75 )
	{
		i_tap = 35;
	}
	//	10～50nm
	else if( ndPitchUM >= 0.01 && ndPitchUM < 0.5 )
	{
		i_tap = 45;
	}
	//	それ以下
	else

	{
		i_tap = 55;
	}


	*npdFs = d_Fs;
	*npdFc = cd_Fc;
	*npiWindow = ci_window;
	*npiTapNum = i_tap;

	return;
}
