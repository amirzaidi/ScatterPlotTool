using System.Runtime.CompilerServices;

namespace ScatterPlotTool
{
    public static class Util
    {
        public static string Log<T>(this T value, [CallerArgumentExpression("value")] string var = "") => $"{var}={value}";

        public static T[] Vector<T>(params T[] values) => values;

        public static (T, T) ToTwoTuple<T>(this T[] values) => (values[0], values[1]);

        public static (T, T, T) ToThreeTuple<T>(this T[] values) => (values[0], values[1], values[2]);

        public static (T, T, T, T) ToFourTuple<T>(this T[] values) => (values[0], values[1], values[2], values[3]);

        public static int Clamp(int val, int lower = 0, int upper = 255)
        {
            if (val < lower)
            {
                return lower;
            }
            if (val > upper)
            {
                return upper;
            }
            return val;
        }

        public static double Clamp(double val, double lower = 0.0, double upper = 255.0)
        {
            if (val < lower)
            {
                return lower;
            }
            if (val > upper)
            {
                return upper;
            }
            return val;
        }
    }
}
