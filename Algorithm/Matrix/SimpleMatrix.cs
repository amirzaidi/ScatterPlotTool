namespace ScatterPlotTool.Algorithm.Matrix
{
    internal class SimpleMatrix<T> : IMatrix<T>
    {
        private readonly T[,] mData;

        public SimpleMatrix(int diagonalCount) : this(diagonalCount, diagonalCount)
        {
        }

        public SimpleMatrix(int rowCount, int columnCount)
        {
            mData = new T[rowCount, columnCount];
        }

        public SimpleMatrix(T[,] data)
        {
            mData = data;
        }

        public T Get(int row, int column) => mData[row, column];

        public void Set(int row, int column, T value) => mData[row, column] = value;

        public int GetRowCount()
        {
            return mData.GetLength(0);
        }

        public int GetColumnCount()
        {
            return mData.GetLength(1);
        }
    }
}
