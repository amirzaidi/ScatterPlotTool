using System;

namespace ScatterPlotTool.Images
{
    class ColorConversion
    {
        public static double GetGrayscale(double r, double g, double b) =>
            0.2126 * r + 0.7152 * g + 0.0722 * b;

        public static double RemoveGamma(double val) =>
            val < 0.04045
                ? val / 12.92
                : Math.Pow((val + 0.055) / 1.055, 2.4);

        public static (double, double, double) RemoveGamma(double r, double g, double b) =>
            (RemoveGamma(r), RemoveGamma(g), RemoveGamma(b));

        public static double AddGamma(double val) =>
            val <= 0.0031308
                ? 12.92 * val
                : 1.055 * Math.Pow(val, 1 / 2.4) - 0.055;

        public static (double, double, double) AddGamma(double r, double g, double b) =>
            (AddGamma(r), AddGamma(g), AddGamma(b));
    }
}
