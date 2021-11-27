using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ScatterPlotTool
{
    public static class Util
    {
        public static string Log<T>(this T value, [CallerArgumentExpression("value")] string var = "") => $"{var}={value}";

        public static T[] Vector<T>(params T[] values) => values;

        public static (T, T) ToTwoTuple<T>(this T[] values) => (values[0], values[1]);

        public static (T, T, T) ToThreeTuple<T>(this T[] values) => (values[0], values[1], values[2]);

        public static (T, T, T, T) ToFourTuple<T>(this T[] values) => (values[0], values[1], values[2], values[3]);

        public static T[] ToArray<T>(this (T, T) tuple) => Vector(tuple.Item1, tuple.Item2);

        public static T[] ToArray<T>(this (T, T, T) tuple) => Vector(tuple.Item1, tuple.Item2, tuple.Item3);

        public static T[] ToArray<T>(this (T, T, T, T) tuple) => Vector(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);

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

        public static void CallAllThenClear(this List<Action> list)
        {
            list.ForEach(x => x());
            list.Clear();
        }

        public static Task IgnoreExceptions(this Task input) => input.ContinueWith(_ => { });

        public static void ParallelLoop(int from, int to, Action<int> iteration, bool parallel)
        {
            if (parallel)
            {
                Parallel.For(from, to, iteration);
            }
            else
            {
                for (int i = from; i < to; i++)
                {
                    iteration(i);
                }
            }
        }
    }
}
