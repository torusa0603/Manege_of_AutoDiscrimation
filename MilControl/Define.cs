using System;

	/// <summary>
	/// CameraControl用Defineクラス
	/// </summary>
	/// <remarks>
	/// 親プロジェクトも使用するのでnamespaceは無し
	/// </remarks>


	/// <summary>
	/// Geometric Model Finder結果用構造体
	/// </summary>
	public struct tag_GMF_Result
	{
		public int model;
		public double x;
		public double y;
		public double score;
		public double angle;
		public double scale;
	}
