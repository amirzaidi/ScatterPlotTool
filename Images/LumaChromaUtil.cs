namespace ScatterPlotTool.Images
{
    internal class LumaChromaUtil
    {
        public const double MIN_LUMA = 0.1;
        public const double MAX_LUMA = 2.0;

        private static readonly byte[] byteArrLuma = new byte[1];
        private static readonly byte[] byteArrChroma = new byte[3];

        public static void ApplyToPixel(Bitmap chromaBm, Bitmap lumaBm, int x, int y, double r, double g, double b, double luma)
        {
            luma = Util.Clamp(luma, MIN_LUMA, MAX_LUMA);

            byteArrChroma[0] = PackColorToByte(r / luma);
            byteArrChroma[1] = PackColorToByte(g / luma);
            byteArrChroma[2] = PackColorToByte(b / luma);
            chromaBm.SetPixels(x, y, pixels: byteArrChroma);

            byteArrLuma[0] = PackColorToByte(luma / MAX_LUMA);
            lumaBm.SetPixels(x, y, pixels: byteArrLuma);
        }

        public static byte PackColorToByte(double color)
        {
            return (byte)Util.Clamp(ColorConversion.AddGamma(color) * 255.0);
        }
    }
}
