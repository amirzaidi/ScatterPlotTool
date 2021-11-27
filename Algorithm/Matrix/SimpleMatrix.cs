using System;

namespace ScatterPlotTool.Algorithm.Matrix
{
    public class SimpleMatrix<T> : IMatrix<T>
    {
        private readonly T[,] mData;

        public SimpleMatrix(int diagonalCount) : this(diagonalCount, diagonalCount)
        {
        }

        public SimpleMatrix(int rowCount, int columnCount) => mData = new T[rowCount, columnCount];

        public SimpleMatrix(T[,] data) => mData = data;

        public T Get(int row, int column) => mData[row, column];

        public void Set(int row, int column, T value) => mData[row, column] = value;

        public void Update(int row, int column, Func<T, T> update) => Set(row, column, update(Get(row, column)));

        public int GetRowCount() => mData.GetLength(0);

        public int GetColumnCount() => mData.GetLength(1);
    }
}
