using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScatterPlotTool.Algorithm.Matrix
{
    public class GaussSeidel
    {
        private readonly IMatrix<double> mA;
        private readonly double[] mX, mXIter;

        public GaussSeidel(IMatrix<double> A, double defaultValue = 1.0)
        {
            mA = A;
            mX = new double[mA.GetColumnCount()];
            mXIter = new double[mX.Length];

            // Initialize the first iteration to the default value.
            for (int i = 0; i < mX.Length; i++)
            {
                mX[i] = defaultValue;
            }
        }

        public void AverageOver(IEnumerable<int> coords)
        {
            var totalValue = 0.0;
            var totalCount = 0;

            foreach (var coord in coords)
            {
                totalValue += mX[coord];
                totalCount += 1;
            }

            totalValue /= totalCount;

            foreach (var coord in coords)
            {
                mX[coord] = totalValue;
            }
        }

        public void Iterate(double[] b, Func<int, IEnumerable<int>> getValidColumns = null, bool parallel = true)
        {
            var rowCount = mA.GetRowCount();
            var columnCount = mA.GetColumnCount();

            if (getValidColumns == null)
            {
                getValidColumns = row => Enumerable.Range(0, columnCount);
            }

            void iterateRow(int row)
            {
                mXIter[row] = b[row];
                foreach (var col in getValidColumns(row))
                {
                    if (col != row)
                    {
                        mXIter[row] -= mA.Get(row, col) * mX[col];
                    }
                }
                mXIter[row] /= mA.Get(row, row);
            }

            if (parallel)
            {
                Parallel.For(0, rowCount, iterateRow);
            }
            else
            {
                for (int row = 0; row < rowCount; row++)
                {
                    iterateRow(row);
                }
            }

            Array.Copy(mXIter, mX, mX.Length);
        }

        public void ForceConstraints((int, double)[] constraints)
        {
            foreach (var (index, val) in constraints)
            {
                mX[index] = val;
            }
        }

        public void ClampAll(double lower, double higher)
        {
            for (int row = 0; row < mX.Length; row++)
            {
                if (mX[row] < lower)
                {
                    mX[row] = lower;
                }
                else if (mX[row] > higher)
                {
                    mX[row] = higher;
                }
            }
        }

        public double[] GetInput()
        {
            return mX;
        }
    }
}
