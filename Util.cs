using System.Runtime.CompilerServices;

namespace ScatterPlotTool
{
    internal static class Util
    {
        public static string Log<T>(this T value, [CallerArgumentExpression("value")] string var = "") => $"{var}={value}";

        public static T[] Vector<T>(params T[] values) => values;
    }
}
