using System.Threading;

namespace ScatterPlotTool.Algorithm
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
        }

        // Step 1: Calculate for each point to which mean it belongs.
        public void FindMeansForPoints(bool parallel = true)
        {
            // Reset the mean counts.
            for (int i = 0; i < mMeans.Length; i++)
            {
                mMeanTotals[i, 0] = 0;
                mMeanTotals[i, 1] = 0;
                mMeanTotals[i, 2] = 0;
                mMeanTotals[i, 3] = 0;
            }

            // For a given color, add it to the best mean.
            Util.ParallelLoop(0, mDataPoints.Length, dataPointIndex =>
            {
                var (r, g, b) = mDataPoints[dataPointIndex];
                var dists = new int[mMeans.Length];
                int minDist = int.MaxValue;
                int minMean = -1;

                // Find the best mean.
                for (int j = 0; j < mMeans.Length; j++)
                {
                    var (rMean, gMean, bMean) = mMeans[j];
                    var currDist = Vector.LengthSquared(r - rMean, g - gMean, b - bMean);
                    dists[j] = currDist;
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        minMean = j;
                    }
                }

                for (int j = 0; j < mMeans.Length; j++)
                {
                    if (dists[j] == minDist)
                    {
                        // Add this point to every mean that is minDist away from that point.
                        Interlocked.Add(ref mMeanTotals[j, 0], 1);
                        Interlocked.Add(ref mMeanTotals[j, 1], r);
                        Interlocked.Add(ref mMeanTotals[j, 2], g);
                        Interlocked.Add(ref mMeanTotals[j, 3], b);
                    }
                }

                // Save the mean that was found. This is only used for visualization.
                mDataPointMeans[dataPointIndex] = minMean;
            }, parallel);
        }

        // Step 2: Move each mean to the average of points it belongs to.
        public int MoveMeansToPoints(bool parallel = true)
        {
            // Keep track of if there was a difference.
            int differences = 0;
            Util.ParallelLoop(0, mMeans.Length, meanIndex =>
            {
                var meanDiv = mMeanTotals[meanIndex, 0];
                if (meanDiv != 0)
                {
                    var newMean = (
                        (byte)(mMeanTotals[meanIndex, 1] / meanDiv),
                        (byte)(mMeanTotals[meanIndex, 2] / meanDiv),
                        (byte)(mMeanTotals[meanIndex, 3] / meanDiv)
                    );

                    if (newMean != mMeans[meanIndex])
                    {
                        mMeans[meanIndex] = newMean;
                        Interlocked.Increment(ref differences);
                    }
                }
            }, parallel);
            return differences;
        }

        public int GetMeanIndexForPointIndex(int pointIndex)
        {
            return mDataPointMeans[pointIndex];
        }

        public int GetPointCountForMean(int meanIndex)
        {
            return mMeanTotals[meanIndex, 0];
        }
    }
}
