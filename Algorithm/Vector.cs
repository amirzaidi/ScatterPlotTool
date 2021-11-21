using System.Linq;

namespace ScatterPlotTool.Algorithm
{
    internal class Vector
    {
        public static int LengthSquared(params int[] components) => components.Select(x => x * x).Sum();
    }
}
