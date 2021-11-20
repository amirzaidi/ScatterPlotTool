using System;

namespace ScatterPlotTool
{
    internal class Cluster
    {
        private readonly (byte, byte, byte)[] mDataPoints;
        private readonly (byte, byte, byte)[] mMeans;

        private readonly int[] mDataPointMeans;
        private readonly int[,] mMeanTotals;

        public Cluster((byte, byte, byte)[] dataPoints, (byte, byte, byte)[] means)
        {
            mDataPoints = dataPoints;
            mMeans = means;

            mDataPointMeans = new int[dataPoints.Length];
            mMeanTotals = new int[means.Length, 4];

            FindMeans();
        }

        private void FindMeans()
        {
            // Reset the mean counts.
            for (int i = 0; i < mMeans.Length; i++)
            {
                mMeanTotals[i, 0] = 0;
                mMeanTotals[i, 1] = 0;
                mMeanTotals[i, 2] = 0;
                mMeanTotals[i, 3] = 0;
            }

            // For each color, add it to the best mean.
            for (int i = 0; i < mDataPointMeans.Length; i++)
            {
                var (r, g, b) = mDataPoints[i];
                int minDist = int.MaxValue;
                int minMean = -1;

                // Find the best mean.
                for (int j = 0; j < mMeans.Length; j++)
                {
                    var (rMean, gMean, bMean) = mMeans[j];
                    var currDist = Vector.LengthSquared(r - rMean, g - gMean, b - bMean);
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        minMean = j;
                    }
                }

                // Add it to the mean.
                mMeanTotals[minMean, 0] += 1;
                mMeanTotals[minMean, 1] += r;
                mMeanTotals[minMean, 2] += g;
                mMeanTotals[minMean, 3] += b;

                // Save the mean that was found.
                mDataPointMeans[i] = minMean;
            }
        }

        // The means have been updated by the constructor or previous call to this function.
        public bool Iterate()
        {
            // Keep track of if there was a difference.
            var foundDiff = false;
            for (int j = 0; j < mMeans.Length; j++)
            {
                var meanDiv = mMeanTotals[j, 0];
                var newMean = (
                    (byte)(mMeanTotals[j, 1] / meanDiv),
                    (byte)(mMeanTotals[j, 2] / meanDiv),
                    (byte)(mMeanTotals[j, 3] / meanDiv)
                );

                if (newMean != mMeans[j])
                {
                    foundDiff = true;
                }

                mMeans[j] = newMean;
            }

            // Update the means for each data point for the next iteration.
            FindMeans();

            return foundDiff;
        }

        public int GetMeanIndexForPointIndex(int pointIndex)
        {
            return mDataPointMeans[pointIndex];
        }
    }
}
