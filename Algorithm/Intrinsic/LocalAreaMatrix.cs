using MathNet.Numerics.LinearAlgebra;
using ScatterPlotTool.Algorithm.Matrix;
using System;
using System.Collections.Generic;

namespace ScatterPlotTool.Algorithm.Intrinsic
{
    internal class LocalAreaMatrix
    {
        // This should be half the propagation.
        private const int MAX_OFFSET = 1; // 1 for 3x3, 2 for 5x5, etc.
        private const int WSIZE = 2 * MAX_OFFSET + 1;
        private const int WSIZE2 = WSIZE * WSIZE;

        // This should be double the offset.
        private const int PROPAGATE = 2; // 2 for 5x5, 3 for 7x7, etc.
        private const int SIZE_N = 2 * PROPAGATE + 1;
        private const int SIZE_N2 = SIZE_N * SIZE_N;

        private const float EPSILON = 0.000001f;

        private readonly int mWidth, mHeight, mPixelCount;
        private readonly IMatrix<double> mL;

        public LocalAreaMatrix(int width, int height)
        {
            mWidth = width;
            mHeight = height;
            mPixelCount = width * height;

            mL = new OptimizedMatrix<double>(
                mPixelCount,
                mPixelCount,
                0.0,
                row => SIZE_N2,
                (row, col) =>
                {
                    // Row to (x, y) coordinates for the center pixel of the window.
                    var (x, y) = (row % width, row / width);

                    // Col to (x, y) coordinates for the offset pixel in the window.
                    var (x2, y2) = (col % width, col / width);

                    // Translate the offset to the index.
                    var (xOffset, yOffset) = (x2 - x, y2 - y);
                    var (xOffsetShift, yOffsetShift) = (xOffset + PROPAGATE, yOffset + PROPAGATE);

                    if (xOffsetShift < 0 || xOffsetShift >= SIZE_N || yOffsetShift < 0 || yOffsetShift >= SIZE_N)
                    {
                        throw new Exception();

                        // This should give zero.
                        // return -1;
                    }

                    var index = yOffsetShift * SIZE_N + xOffsetShift;
                    return index;
                }
            );
        }

        public static IEnumerable<int> GetValidColumns(int width, int height, int row)
        {
            // Row to (x, y) coordinates for the center pixel of the window.
            var (xCenter, yCenter) = (row % width, row / width);

            foreach (var (xOffset, yOffset) in CoordGenerator.Range2D(SIZE_N))
            {
                var x = xCenter + xOffset - PROPAGATE;
                var y = yCenter + yOffset - PROPAGATE;

                // If this is a valid pixel, add its column.
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    yield return y * width + x;
                }
            }
        }

        public IMatrix<double> ComputeL(Func<int, int, (double, double, double)> image)
        {
            // Create (N+3)x3 Matrix.
            var Mi = Matrix<double>.Build.Dense(WSIZE2 + 3, 3);
            var Id = Matrix<double>.Build.DiagonalIdentity(WSIZE2 + 3);

            // Diagonal of epsilons at the end.
            for (int i = 0; i < 3; i++)
            {
                Mi[WSIZE2 + i, i] = EPSILON;
            }

            // For each pixel, compute M, then N, then add to L.
            foreach (var (xCenter, yCenter) in CoordGenerator.Range2D(MAX_OFFSET, MAX_OFFSET, mWidth - MAX_OFFSET, mHeight - MAX_OFFSET))
            {
                // Fill M with pixel data in the window around (xCenter, yCenter).
                foreach (var (xOffset, yOffset) in CoordGenerator.Range2D(WSIZE))
                {
                    var x = xCenter + xOffset - MAX_OFFSET;
                    var y = yCenter + yOffset - MAX_OFFSET;

                    var (r, g, b) = image(x, y);
                    Mi[yOffset * WSIZE + xOffset, 0] = r;
                    Mi[yOffset * WSIZE + xOffset, 1] = g;
                    Mi[yOffset * WSIZE + xOffset, 2] = b;
                }
                
                // Compute N from M using equations 9 and 10.
                var MiT = Mi.Transpose(); // Slight speedup because this is used twice.
                var Ni = Id - Mi * (MiT * Mi).Inverse() * MiT;
                var NiTNi = Ni.Transpose() * Ni; // This step is where the energy squaring happens.

                // Add N to L using equation 11.
                AddNToL(xCenter, yCenter, NiTNi);
            }

            return mL;
        }

        private void AddNToL(int xCenter, int yCenter, Matrix<double> N)
        {
            // For each row in N:
            foreach (var (xOffset, yOffset) in CoordGenerator.Range2D(WSIZE))
            {
                var x = xCenter + xOffset - MAX_OFFSET;
                var y = yCenter + yOffset - MAX_OFFSET;

                // Calculate the position of the corresponding row in L.
                var rowL = y * mWidth + x;
                var rowN = yOffset * WSIZE + xOffset;

                // Then for each column in N:
                foreach (var (xOffset2, yOffset2) in CoordGenerator.Range2D(WSIZE))
                {
                    var x2 = xCenter + xOffset2 - MAX_OFFSET;
                    var y2 = yCenter + yOffset2 - MAX_OFFSET;

                    // Calculate the position of the corresponding column in L.
                    var colL = y2 * mWidth + x2;
                    var colN = yOffset2 * WSIZE + xOffset2;

                    // Add N(rowN, colN) to L(rowL, colL).
                    mL.Update(rowL, colL, val => val + N[rowN, colN]);
                }
            }
        }
    }
}
