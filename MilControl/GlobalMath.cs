using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Matrox.MatroxImagingLibrary;

namespace MilControl
{
	/// <summary>
	/// 演算集
	/// </summary>
	/// <remarks>
	///  OIS.WLInterferometryのC++で作られたGlobalMathクラスをC#へ置きかえ、又は
	///  OISのC#で作られたDataProcessクラスの一部を抜き出したクラス。
	///  どちらにしても必要な機能（関数）のみ移植
	/// </remarks>
	static class CGlobalMath
	{
		#region 逆行列を求める
		/// <summary>
		/// 逆行列を求める関数
		/// Gauss-Jordan法により、逆行列を求める。
		/// 入力する行列は正方行列であることが原則である
		/// 入力、出力される行列は１次元配列。
		/// 例えば 
		///		2,3,2
		///		1,2,2
		///		4,5,3
		/// という行列は、a[9] = { 2,3,2,1,2,2,4,5,3 }
		/// と表される
		/// </summary>
		/// <param name="ndarrOrgMatrix">(IN)	オリジナル行列</param>
		/// <param name="ndarrInvMatrix">(OUT)	逆行列</param>
		/// <param name="niN">(IN)	N×N行列のN</param>
		/// <returns>true:成功</returns>
		/// <remarks>
		///  Gauss-Jordan法により、逆行列を求める。
		///  入力する行列は正方行列であることが原則である
		///  入力、出力される行列は１次元配列。
		///  例えば 
		///		2,3,2
		///		1,2,2
		///		4,5,3
		///  という行列は、a[9] = { 2,3,2,1,2,2,4,5,3 }
		///  と表される
		/// </remarks>
		static private bool InverseMatrix( double[] ndarrOrgMatrix, double[] ndarrInvMatrix, int niNum )
		{
			bool	b_ret		= false;

			try
			{
				double d_temp;

				//単位行列で初期化
				for( int i_loop2 = 0; i_loop2 < niNum; i_loop2++ )
				{
					Parallel.For( 0, niNum, i_loop =>
					{
						if( i_loop == i_loop2 )
						{
							ndarrInvMatrix[ i_loop + i_loop2 * niNum ] = 1;
						}
						else
						{
							ndarrInvMatrix[ i_loop + i_loop2 * niNum ] = 0;
						}
					} );
				}

				// 正常終了
				b_ret	= true;
				for( int i_loop_k = 0; i_loop_k < niNum; i_loop_k++ )
				{
					//k行k列目の要素を１にする
					d_temp = ndarrOrgMatrix[ i_loop_k + i_loop_k * niNum ];
					if( 0 == d_temp )
					{
						b_ret	= false;
						break;
					}
					Parallel.For( 0, niNum, i_loop =>
					{
						ndarrOrgMatrix[ i_loop + i_loop_k * niNum ] /= d_temp;
						ndarrInvMatrix[ i_loop + i_loop_k * niNum ] /= d_temp;
					} );

					Parallel.For( 0, niNum, i_loop2 =>
					{
						if( i_loop2 != i_loop_k )
						{
							double	d_temp2		= ndarrOrgMatrix[ i_loop_k + i_loop2 * niNum ] / ndarrOrgMatrix[ i_loop_k + i_loop_k * niNum ];
							for( int i_loop = 0; i_loop < niNum; i_loop++ )
							{
								ndarrOrgMatrix[ i_loop + i_loop2 * niNum ] -= ndarrOrgMatrix[ i_loop + i_loop_k * niNum ] * d_temp2;
								ndarrInvMatrix[ i_loop + i_loop2 * niNum ] -= ndarrInvMatrix[ i_loop + i_loop_k * niNum ] * d_temp2;
							}
						}
					} );
				}
			}
			catch( System.Exception ex )
			{
				string str_log = System.Reflection.MethodBase.GetCurrentMethod().Name + "  " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
			}
			return	b_ret;
		}
		#endregion


		#region 最小二乗法
		/// <summary>
		/// 最小二乗法により、近似円を求める
		/// </summary>
		/// <param name="ndarrX">入力するX座標の配列</param>
		/// <param name="ndarrY">入力するY座標の配列</param>
		/// <param name="niNum">X,Yのデータ数</param>
		/// <param name="nrdCenterX">円の中心座標X</param>
		/// <param name="nrdCenterY">円の中心座標Y</param>
		/// <param name="nrdRadius">円の半径</param>
		/// <returns>true:成功</returns>
		/// <remarks>
		///   計算式
		///   | Σx ^ 2  Σxy Σx |   | A |   | -Σ(x ^ 3 + xy ^ 2) |
		///   | Σxy  Σy ^ 2 Σy | * | B | = | -Σ(x ^ 2 * y + y ^ 3) |
		///   | Σx	Σy	   Σ1 |   | C |   | -Σ(x ^ 2 + y ^ 2) |
		///   A = -2a
		///   B = -2b
		///   C = a ^ 2 + b ^ 2 - r ^ 2
		///   中心(a, b)半径rの円
		/// </remarks>
		static public bool CircleApproxByLeastSquaresMethod( double[] ndarrX, double[] ndarrY,int niNum, out double nrdCenterX, out double nrdCenterY, out double nrdRadius )
		{
			bool	b_ret		= false;

			// 参照渡しの変数に不定値を代入する
			nrdCenterX = double.MinValue;
			nrdCenterY = double.MinValue;
			nrdRadius = double.MinValue;
			try
			{
				// データが3つ以下
				if( niNum < 3 )
				{
					return	b_ret;
				}

				double d_sum; // Σ計算用
				double[] darr_matrixA = new double[ 9 ]; // 行列A
				double[] darr_matrixB = new double[ 3 ]; // 行列B
				double[] darr_inv_matrixA = new double[ 9 ]; // 逆行列A
				double[] darr_resultABC = new double[ 3 ]; // 行列の積の計算結果ABC

				// 行列Aを求める
				// Σx ^ 2
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = Math.Pow( ndarrX[ i_loop ], 2.0 );
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixA[ 0 ] = d_sum;
				// Σxy
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = ndarrX[ i_loop ] * ndarrY[ i_loop ];
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixA[ 1 ] = d_sum;
				darr_matrixA[ 3 ] = d_sum;
				// Σx
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					lock( System.Threading.Thread.CurrentContext )
						d_sum += ndarrX[ i_loop ];
				} );
				darr_matrixA[ 2 ] = d_sum;
				darr_matrixA[ 6 ] = d_sum;
				// Σy ^ 2
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = Math.Pow( ndarrY[ i_loop ], 2.0 );
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixA[ 4 ] = d_sum;
				// Σy
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					lock( System.Threading.Thread.CurrentContext )
						d_sum += ndarrY[ i_loop ];
				} );
				darr_matrixA[ 5 ] = d_sum;
				darr_matrixA[ 7 ] = d_sum;
				// Σ1
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					lock( System.Threading.Thread.CurrentContext )
						d_sum += 1;
				} );
				darr_matrixA[ 8 ] = d_sum;
				// 行列Bを求める
				// -Σ(x ^ 3 + xy ^ 2)
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = Math.Pow( ndarrX[ i_loop ], 3.0 ) + ndarrX[ i_loop ] * Math.Pow( ndarrY[ i_loop ], 2.0 );
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixB[0] = -d_sum;
				// -Σ(x ^ 2 * y + y ^ 3) 
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = Math.Pow( ndarrX[ i_loop ], 2.0 ) * ndarrY[ i_loop ] + Math.Pow( ndarrY[ i_loop ], 3.0 );
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixB[1] = -d_sum;
				// -Σ(x ^ 2 + y ^ 2)
				d_sum = 0.0;
				Parallel.For( 0, niNum, i_loop =>
				{
					double d_temp = Math.Pow( ndarrX[ i_loop ], 2.0 ) + Math.Pow( ndarrY[ i_loop ], 2.0 );
					lock( System.Threading.Thread.CurrentContext )
						d_sum += d_temp;
				} );
				darr_matrixB[ 2 ] = -d_sum;
				// Aの逆行列を求める
				InverseMatrix( darr_matrixA, darr_inv_matrixA, 3 );
				// Aの逆行列とBを掛ける
				for( int i_loop_x = 0; i_loop_x < 3; i_loop_x++ )
				{
					d_sum = 0.0;
					for( int  i_loop_y = 0; i_loop_y < 3; i_loop_y++ )
					{
						d_sum += darr_inv_matrixA[ i_loop_x * 3 + i_loop_y ] * darr_matrixB[ i_loop_y ];
					}
					darr_resultABC[ i_loop_x ] = d_sum;
				}
				// A = -2a からaを求める
				nrdCenterX = -0.5 * darr_resultABC[ 0 ];
				// B = -2b からbを求める
				nrdCenterY = -0.5 * darr_resultABC[ 1 ];
				// C = a^2 + b^2 - r^2 からrを求める
				nrdRadius = Math.Pow( nrdCenterX, 2 ) + Math.Pow( nrdCenterY, 2 ) - darr_resultABC[ 2 ];
				nrdRadius = Math.Sqrt( nrdRadius );
				// 正常を返す
				b_ret	= true;
			}
			catch( System.Exception ex )
			{
				string str_log = System.Reflection.MethodBase.GetCurrentMethod().Name + "  " + ex.Message;
				System.Diagnostics.Debug.WriteLine( str_log );
				MessageBox.Show( str_log );
			}
			return	b_ret;
		}
		#endregion
	}
}
