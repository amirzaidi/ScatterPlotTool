using Microsoft.Win32;
using ScatterPlotTool.Algorithm;
using ScatterPlotTool.Algorithm.Intrinsic;
using ScatterPlotTool.Algorithm.Matrix;
using ScatterPlotTool.Images;
using ScatterPlotTool.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScatterPlotTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Plotting mPlotting;
        private readonly Camera mCamera;
        private CancellationTokenSource? mCTS;
        private Bitmap? mLowResBitmap;

        public MainWindow()
        {
            InitializeComponent();
            mPlotting = new Plotting(ModelGroup.Children);
            mCamera = new Camera(Camera);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogText1.Text = "Downsampling: Linear Left, K-Means Right";
            LogText2.Text = "K-Means: 16 to 4 Pixels Downsampling";
            LogText3.Text = "Gauss-Seidel: First Row Luma/Chroma, 1, 5, 50 Iterations";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    mCamera.AddRot(0.1, 0.0);
                    break;
                case Key.Down:
                    mCamera.AddRot(-0.1, 0.0);
                    break;
                case Key.Left:
                    mCamera.AddRot(0.0, -0.1);
                    break;
                case Key.Right:
                    mCamera.AddRot(0.0, 0.1);
                    break;
                case Key.Add:
                case Key.OemPlus:
                    mCamera.AddRadius(-1.0);
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    mCamera.AddRadius(1.0);
                    break;
            }

            // Update the camera's position.
            mCamera.UpdatePosition();
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Images (*.tif)|*.tif|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                if (mCTS != null)
                {
                    mCTS.Cancel();
                }

                mCTS = new CancellationTokenSource();
                StartProcessing(dialog.FileName, mCTS.Token);
            }
        }

        private async void StartProcessing(string filename, CancellationToken token)
        {
            // Full size image.
            var fullResBm = new Bitmap(filename);
            fullResBm.ApplyTo(FullResImage);

            var (wHalf, hHalf) = (fullResBm.GetWidth() / 2, fullResBm.GetHeight() / 2);
            var (wQuar, hQuar) = (wHalf / 2, hHalf / 2);

            // Simple downsample.
            var lowResNormalBm = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            lowResNormalBm.ApplyTo(DownsampleNormal);

            var downsampleNormalBuf = new byte[3];
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wQuar * 2, hQuar * 2))
            {
                var pixels = fullResBm.GetPixels(x * 2, y * 2, 2, 2);
                downsampleNormalBuf[0] = (byte)((pixels[0] + pixels[4] + pixels[8] + pixels[12]) / 4);
                downsampleNormalBuf[1] = (byte)((pixels[1] + pixels[5] + pixels[9] + pixels[13]) / 4);
                downsampleNormalBuf[2] = (byte)((pixels[2] + pixels[6] + pixels[10] + pixels[14]) / 4);
                lowResNormalBm.SetPixels(x, y, pixels: downsampleNormalBuf);
            }

            // Setup for the smart downsample 3D render.
            mCamera.UpdatePosition();
            mPlotting.ClearAll();
            mPlotting.CreateAxes();

            // Smart downsample.
            var lowResSmartBm = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            lowResSmartBm.ApplyTo(DownsampleSmart);

            // Reusable buffers.
            var data = new (byte, byte, byte)[16];
            var dataPoints = new Point3D[data.Length];

            var means = new (byte, byte, byte)[4];
            var meanUpdaters = new Func<byte, byte, byte, Point3D>[means.Length];
            var meanPoints = new Point3D[means.Length];

            for (int i = 0; i < 4; i++)
            {
                meanUpdaters[i] = mPlotting.AddRGBMean();
                meanPoints[i] = meanUpdaters[i](0, 0, 0);
            }

            var clustering = new Cluster(data, means);
            var removeLines = new List<Action>();

            const int NUM_ITER = 8;
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wQuar, hQuar))
            {
                // Take an area of 4x4.
                var pixelsBGRA = fullResBm.GetPixels(x * 4, y * 4, 4, 4);

                // Remove old RGB points.
                mPlotting.ClearRGBPoints();

                // Fill the buffers.
                for (int i = 0; i < pixelsBGRA.Length; i += 4)
                {
                    data[i / 4] = (pixelsBGRA[i], pixelsBGRA[i + 1], pixelsBGRA[i + 2]);

                    if (!token.IsCancellationRequested)
                    {
                        dataPoints[i / 4] = mPlotting.AddRGBPoint(pixelsBGRA[i + 2], pixelsBGRA[i + 1], pixelsBGRA[i + 0]);
                    }
                }

                // Reset the means to the top left pixel of each 2x2 area in the 4x4 area.
                // Array.Copy(data, means, 4);
                //means[0] = data[0];
                //means[1] = data[2];
                //means[2] = data[8];
                //means[3] = data[10];

                // Invert the order for a more obvious effect in the permuting step.
                means[0] = data[10];
                means[1] = data[8];
                means[2] = data[2];
                means[3] = data[0];

                for (int iter = 0; iter < NUM_ITER; iter++)
                {
                    // Find the best mean for each point.
                    clustering.FindMeansForPoints();

                    // Clear the old lines that are now invalid.
                    removeLines.CallAllThenClear();

                    if (!token.IsCancellationRequested)
                    {
                        // Wait for a bit to show the user what is happening.
                        //LogText2.Text = $"Mean Iteration: {iter + 1}. ";

                        // Update the lines between the means and the points.
                        for (int i = 0; i < data.Length; i++)
                        {
                            var A = meanPoints[clustering.GetMeanIndexForPointIndex(i)];
                            var B = dataPoints[i];

                            // Add one line for each point.
                            var model = Line.Between(A, B);
                            ModelGroup.Children.Add(model);
                            removeLines.Add(() => ModelGroup.Children.Remove(model));
                        }

                        await Task.Delay(150, token).IgnoreExceptions();
                    }

                    if (clustering.MoveMeansToPoints() == 0)
                    {
                        LogText2.Text = "Converged.";
                        break;
                    }

                    if (!token.IsCancellationRequested)
                    {
                        // Update the means in the UI if there were changes.
                        for (int i = 0; i < means.Length; i++)
                        {
                            var (b, g, r) = means[i];
                            meanPoints[i] = meanUpdaters[i](r, g, b);
                        }
                    }
                }

                lowResSmartBm.SetPixels(x * 2, y * 2, 2, 2, Util.Concat(
                    means[0].ToArray(),
                    means[1].ToArray(),
                    means[2].ToArray(),
                    means[3].ToArray()
                ));

                if (!token.IsCancellationRequested || x == wQuar - 1)
                {
                    // Update the UI.
                    await Task.Delay(1);
                }
            }

            // Now we try to align the pixels in the right orientation.
            await PermuteDownsampled.Run(DownsampleNormalPermuted, fullResBm, lowResNormalBm, wHalf, hHalf, token);
            mLowResBitmap = await PermuteDownsampled.Run(DownsampleSmartPermuted, fullResBm, lowResSmartBm, wHalf, hHalf, token);
        }

        private void FastForward_Click(object sender, RoutedEventArgs e)
        {
            mCTS?.Cancel();
        }

        private async void IntrinsicSplit_Click(object sender, RoutedEventArgs e)
        {
            if (mLowResBitmap == null)
            {
                return;
            }

            // Intrinsic Image Algorithm starts here.
            var wHalf = mLowResBitmap.GetWidth();
            var hHalf = mLowResBitmap.GetHeight();

            // First attempt: Simple grayscale.
            var lowResBmLuma = new Bitmap(wHalf, hHalf, PixelFormats.Gray8);
            var lowResBmChroma = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);

            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
            {
                var (r, g, b) = mLowResBitmap.GetPixels(x, y).ToThreeTuple(); // With gamma.
                var (r2, g2, b2) = ColorConversion.RemoveGamma(r / 255.0, g / 255.0, b / 255.0); // Without gamma.
                var luma = ColorConversion.GetGrayscale(r2, g2, b2) * 2.0 * LumaChromaUtil.MAX_LUMA; // Without gamma.
                LumaChromaUtil.ApplyToPixel(lowResBmChroma, lowResBmLuma, x, y, r2, g2, b2, luma); // Pass all values without gamma.
            }

            lowResBmLuma.ApplyTo(LowResImage1);
            lowResBmChroma.ApplyTo(LowResImage2);

            // Second attempt: paper implementation with two iterations.
            var LCalc = new LocalAreaMatrix(wHalf, hHalf);
            var L = LCalc.ComputeL((x, y) =>
            {
                var (r, g, b) = mLowResBitmap.GetPixels(x, y).ToThreeTuple();
                return (
                    ColorConversion.RemoveGamma(r / 255.0),
                    ColorConversion.RemoveGamma(g / 255.0),
                    ColorConversion.RemoveGamma(b / 255.0)
                );
            });

            var GS = new GaussSeidel(L);
            var bZero = new double[wHalf * hHalf];

            foreach (var (lowResImageLuma, lowResImageChroma, iterCount) in new[] {
                (LowResImage3, LowResImage4, 1),
                (LowResImage5, LowResImage6, 5),
                (LowResImage7, LowResImage8, 50)
            })
            {
                var iterVal = GS.GetInput();

                var iterBmLuma = new Bitmap(wHalf, hHalf, PixelFormats.Gray8);
                var iterBmChroma = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);

                iterBmLuma.ApplyTo(lowResImageLuma);
                iterBmChroma.ApplyTo(lowResImageChroma);

                for (int i = 0; i < iterCount; i++)
                {
                    await Task.Run(() =>
                    {
                        GS.Iterate(bZero, row => LocalAreaMatrix.GetValidColumns(wHalf, hHalf, row));
                        GS.ClampAll(LumaChromaUtil.MIN_LUMA, LumaChromaUtil.MAX_LUMA);

                        // Two diagonals as constraints.
                        GS.AverageOver(Enumerable.Range(0, wHalf).Select(x => x + (wHalf * (x * hHalf / wHalf))));
                        GS.AverageOver(Enumerable.Range(0, wHalf).Select(x => (wHalf - x - 1) + (wHalf * (x * hHalf / wHalf))));
                    });

                    foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
                    {
                        var (r, g, b) = mLowResBitmap.GetPixels(x, y).ToThreeTuple(); // With gamma.
                        var (r2, g2, b2) = ColorConversion.RemoveGamma(r / 255.0, g / 255.0, b / 255.0); // Without gamma.
                        var luma = iterVal[y * wHalf + x]; // Without gamma.
                        LumaChromaUtil.ApplyToPixel(iterBmChroma, iterBmLuma, x, y, r2, g2, b2, luma); // Pass all values without gamma.
                    }
                }
            }
        }
    }
}
