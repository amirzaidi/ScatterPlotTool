using ScatterPlotTool.Algorithm;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScatterPlotTool.Images
{
    internal class PermuteDownsampled
    {
        public static async Task<Bitmap> Run(Image image, Bitmap fullResBm, Bitmap lowResBm, int wHalf, int hHalf, CancellationToken token)
        {
            var downsamplePermutedBm = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            downsamplePermutedBm.ApplyTo(image);

            var pixels2x2ordered = new byte[12];
            var lowestL2Dist = int.MaxValue;
            var lowestL2Order = Array.Empty<int>();

            var (wQuar, hQuar) = (wHalf / 2, hHalf / 2);
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wQuar, hQuar))
            {
                var pixels4x4 = fullResBm.GetPixels(x * 4, y * 4, 4, 4);
                var pixels2x2 = lowResBm.GetPixels(x * 2, y * 2, 2, 2);

                // We will re-order the 2x2 grid in the best order.
                foreach (var order in Permutation.FindAll(Util.Vector(0, 1, 2, 3)))
                {
                    // Copy the ordered 2x2 data to a new array.
                    Buffer.BlockCopy(pixels2x2, 3 * order[0], pixels2x2ordered, 0, 3);
                    Buffer.BlockCopy(pixels2x2, 3 * order[1], pixels2x2ordered, 3, 3);
                    Buffer.BlockCopy(pixels2x2, 3 * order[2], pixels2x2ordered, 6, 3);
                    Buffer.BlockCopy(pixels2x2, 3 * order[3], pixels2x2ordered, 9, 3);

                    // Compare each pixel in the large image to the ordered small image and sum it.
                    var L2Dist = 0;
                    foreach (var (xSmall, ySmall) in CoordGenerator.Range2D(4))
                    {
                        var indexLarge = 4 * (ySmall * 4 + xSmall);
                        var indexSmall = 3 * ((ySmall / 2) * 2 + xSmall / 2);

                        var bDiff = pixels2x2ordered[indexSmall] - pixels4x4[indexLarge];
                        var gDiff = pixels2x2ordered[indexSmall + 1] - pixels4x4[indexLarge + 1];
                        var rDiff = pixels2x2ordered[indexSmall + 2] - pixels4x4[indexLarge + 2];

                        L2Dist += Vector.LengthSquared(bDiff, gDiff, rDiff);
                    }

                    if (L2Dist < lowestL2Dist)
                    {
                        lowestL2Dist = L2Dist;
                        lowestL2Order = order.ToArray();
                    }
                }

                // Use the best ordering.
                Buffer.BlockCopy(pixels2x2, 3 * lowestL2Order[0], pixels2x2ordered, 0, 3);
                Buffer.BlockCopy(pixels2x2, 3 * lowestL2Order[1], pixels2x2ordered, 3, 3);
                Buffer.BlockCopy(pixels2x2, 3 * lowestL2Order[2], pixels2x2ordered, 6, 3);
                Buffer.BlockCopy(pixels2x2, 3 * lowestL2Order[3], pixels2x2ordered, 9, 3);
                downsamplePermutedBm.SetPixels(x * 2, y * 2, 2, 2, pixels2x2ordered);

                if (!token.IsCancellationRequested || x == wQuar - 1)
                {
                    // Update the UI.
                    await Task.Delay(1);
                }
            }

            return downsamplePermutedBm;
        }
    }
}
