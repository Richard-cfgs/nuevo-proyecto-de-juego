namespace PixelWallE.Core
{
    public class Canvas
    {
        private static int CanvasWidth;
        private static int CanvasHeight;
        private static string[,] _pixels = new string[CanvasWidth, CanvasHeight];
        public Canvas(int width, int height)
        {
            CanvasWidth = width;
            CanvasHeight = height;
        }
        public static void SetPixel(int x, int y, string color)
        {
            if (x < 0 || x >= CanvasWidth || y < 0 || y >= CanvasHeight)
                return;

            _pixels[x, y] = color;
        }
        public static string GetPixel(int x, int y)
        {
            return _pixels[x, y];
        }
    }
}