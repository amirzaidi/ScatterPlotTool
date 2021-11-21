using System;

namespace ScatterPlotTool.Algorithm.Matrix
{
    internal class GaussSeidel
    {
        private readonly IMatrix<double> mA;

        public GaussSeidel(IMatrix<double> A)
        {
            mA = A;
        }

        public double[] FindInputForOutput(double[] b, (int, double)[]? constraints = null, Action<double[], int>? log = null, int iterationCount = 10)
        {
            var x = new double[mA.GetColumnCount()];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = 1f;
            }

            for (int iter = 0; iter < iterationCount; iter++)
            {
                var x_iter = new double[x.Length];
                for (int row = 0; row < mA.GetRowCount(); row++)
                {
                    for (int col = 0; col < mA.GetColumnCount(); col++)
                    {
                        if (col != row)
                        {
                            x_iter[row] -= mA.Get(row, col) * x[col];
                        }
                    }

                    x_iter[row] += b[row];
                    x_iter[row] /= mA.Get(row, row);
                }

                if (constraints != null)
                {
                    for (int i = 0; i < constraints.Length; i++)
                    {
                        var (index, val) = constraints[i];
                        x_iter[index] = val;
                    }
                }

                if (log != null)
                {
                    log(x_iter, iter);
                }

                Array.Copy(x_iter, x, x.Length);
            }

            return x;
        }
    }
}
