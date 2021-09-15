using System;

	/// <summary>
	/// ログの種類を定義するクラス
	/// </summary>
	/// <remarks>
	/// プログラムの可読性を高める為
	/// </remarks>
	public static class LogKind
	{
		/// <summary>
		/// 軸種類
		/// </summary>
		public const ushort error = 0;
		public const ushort execute = error + 1;
		public const ushort counter = execute + 1;
		public const ushort axis = counter + 1;
		public const ushort light = axis + 1;
		public const ushort revolver = light + 1;
		public const ushort camera = revolver + 1;
		public const ushort pio = camera + 1;
		public const ushort mainprocess = pio + 1;
		public const ushort max = mainprocess + 1;

		public const string Error = "Error";
		public const string Execute = "Execute";
		public const string Counter = "Counter";
		public const string Axis = "Axis";
		public const string Light = "Light";
		public const string Revolver = "Revolver";
		public const string Camera = "Camera";
		public const string Pio = "Pio";
		public const string MainProcess = "MainProcess";
		public const string MAX = "MAX";

		static public string ToString( ushort nusKind )
		{
			if( error == nusKind )
			{
				return Error;
			}
			else if( execute == nusKind )
			{
				return Execute;
			}
			else if( counter == nusKind )
			{
				return Counter;
			}
			else if( axis == nusKind )
			{
				return Axis;
			}
			else if( light == nusKind )
			{
				return Light;
			}
			else if( revolver == nusKind )
			{
				return Revolver;
			}
			else if( camera == nusKind )
			{
				return Camera;
			}
			else if( pio == nusKind )
			{
				return Pio;
			}
			else if( pio == nusKind )
			{
				return Pio;
			}
			else if (mainprocess == nusKind)
			{
				return MainProcess;
			}
			else
			{
				return MAX;
			}
		}
	}
