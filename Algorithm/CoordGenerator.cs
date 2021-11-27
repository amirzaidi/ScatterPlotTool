using System.Collections.Generic;

namespace ScatterPlotTool.Algorithm
{
    internal class CoordGenerator
    {
        public static IEnumerable<(int, int)> Range2D(int end) => Range2D(0, end);

        public static IEnumerable<(int, int)> Range2D(int start, int end) => Range2D(start, start, end, end);

        public static IEnumerable<(int, int)> Range2D(int x1, int y1, int x2, int y2)
        {
            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    yield return (x, y);
                }
            }
        }
    }
}
