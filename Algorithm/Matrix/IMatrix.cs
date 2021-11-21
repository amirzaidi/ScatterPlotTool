namespace ScatterPlotTool.Algorithm.Matrix
{
    internal interface IMatrix<T>
    {
        public T Get(int row, int column);

        public void Set(int row, int column, T value);

        public int GetRowCount();

        public int GetColumnCount();
    }
}
