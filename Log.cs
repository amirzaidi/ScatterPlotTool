using System;
using System.Diagnostics;

namespace ScatterPlotTool
{
    class Log
    {
        public static void Write(string Msg) =>
            Debug.WriteLine($"[{DateTime.Now.ToLongTimeString()}] {Msg}");
    }
}
