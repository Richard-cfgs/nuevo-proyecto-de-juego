namespace PixelWallE.Core
{
	public static class Canvas
	{
		private static int CanvasWidth = Dictionarys.CanvasSize;
		private static int CanvasHeight = Dictionarys.CanvasSize;
		private static string[,] _pixels = InitPixels();

		private static string[,] InitPixels()
		{
			var pixels = new string[CanvasWidth, CanvasHeight];
			for (int y = 0; y < CanvasHeight; y++)
				for (int x = 0; x < CanvasWidth; x++)
					pixels[x, y] = "White";
			return pixels;
		}

		public static void Reset()
		{
			CanvasWidth = CanvasHeight = Dictionarys.CanvasSize;
			_pixels = InitPixels();
		}

		public static void SetPixel(int x, int y, string color)
		{
			if (_pixels == null) return;
			if (x < 0 || x >= CanvasWidth || y < 0 || y >= CanvasHeight)
				return;
			_pixels[x, y] = color;
		}

		public static string GetPixel(int x, int y)
		{
			if (_pixels == null) return "White"; // Prevención contra el error
			return _pixels[x, y];
		}
	}
}
