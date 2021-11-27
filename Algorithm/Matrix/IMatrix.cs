using System;

namespace ScatterPlotTool.Algorithm.Matrix
{
    public interface IMatrix<T>
    {
        public T Get(int row, int column);

        public void Set(int row, int column, T value);

        public void Update(int row, int column, Func<T, T> update);

        public int GetRowCount();

        public int GetColumnCount();
    }
}
