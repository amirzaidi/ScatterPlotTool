using System;

namespace ScatterPlotTool.Algorithm.Matrix
{
    internal class OptimizedMatrix<T> : IMatrix<T>
    {
        private readonly Func<int, int, int> mGetIndexForColumn;
        private readonly int mColumnCount;
        private readonly T mDefaultValue;
        private readonly T[][] mData;

        public OptimizedMatrix(int rowCount, int columnCount, T defaultValue, Func<int, int> getIndexCount, Func<int, int, int> getIndexForColumn)
        {
            (mGetIndexForColumn, mColumnCount, mDefaultValue) = (getIndexForColumn, columnCount, defaultValue);
            mData = new T[rowCount][];
            for (int i = 0; i < rowCount; i++)
            {
                mData[i] = new T[getIndexCount(i)];
            }
        }

        public T Get(int row, int column)
        {
            var index = mGetIndexForColumn(row, column);
            var rowData = mData[row];
            return (index < 0 || index >= rowData.Length) ? mDefaultValue : rowData[index];
        }

        public void Set(int row, int column, T value)
        {
            var index = mGetIndexForColumn(row, column);
            var rowData = mData[row];
            if (index < 0 || index >= rowData.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            rowData[index] = value;
        }

        public int GetRowCount()
        {
            return mData.Length;
        }

        public int GetColumnCount()
        {
            return mColumnCount;
        }
    }
}
