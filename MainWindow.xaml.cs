using ScatterPlotTool.Algorithm;
using ScatterPlotTool.Algorithm.Intrinsic;
using ScatterPlotTool.Algorithm.Matrix;
using ScatterPlotTool.Images;
using ScatterPlotTool.Render;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ScatterPlotTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MIN_LUMA = 0.1;
        private const double MAX_LUMA = 2.0;

        private readonly Plotting mPlotting;
        private readonly Camera mCamera;
        private readonly Dispatcher mDispatcher;

        private readonly byte[] byteArrLuma = new byte[1];
        private readonly byte[] byteArrChroma = new byte[3];

        public MainWindow()
        {
            InitializeComponent();
            mPlotting = new Plotting(ModelGroup.Children);
            mCamera = new Camera(Camera);
            mDispatcher = Application.Current.Dispatcher;
        }

        private void ApplyToPixel(Bitmap chromaBm, Bitmap lumaBm, int x, int y, double r, double g, double b, double luma)
        {
            luma = Util.Clamp(luma, MIN_LUMA, MAX_LUMA);

            byteArrChroma[0] = PackColorToByte(r / luma);
            byteArrChroma[1] = PackColorToByte(g / luma);
            byteArrChroma[2] = PackColorToByte(b / luma);
            chromaBm.SetPixels(x, y, pixels: byteArrChroma);

            byteArrLuma[0] = PackColorToByte(luma / MAX_LUMA);
            lumaBm.SetPixels(x, y, pixels: byteArrLuma);
        }

        private byte PackColorToByte(double color)
        {
            return (byte)Util.Clamp(ColorConversion.AddGamma(color) * 255.0);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogText1.Text = "Downsampling: Nearest Neighbour Left, K-Means Right";
            LogText2.Text = "K-Nearest Neighbours 16 -> 4 Pixels";
            LogText3.Text = "Gauss-Seidel Iterations: 1, 20, 400";

            //const string PATH = "D:/Programs/DIPimage 2.9/images/flamingo.tif";
            //const string PATH = @"C:\Users\Amir\Desktop\campus.tif";
            //const string PATH = @"C:\Users\Amir\Desktop\campus2.tif";
            //const string PATH = @"C:\Users\Amir\Desktop\minor.tif";
            const string PATH = @"C:\Users\Amir\Desktop\street.tif";

            var fullResBitmap = new Bitmap(PATH);
            fullResBitmap.ApplyTo(FullResImage);

            var dsScale = 2;
            var (wHalf, hHalf) = (fullResBitmap.GetWidth() / dsScale, fullResBitmap.GetHeight() / dsScale);

            var lowResBitmap = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
            {
                lowResBitmap.SetPixels(x, y, pixels: fullResBitmap.GetPixels(x * dsScale, y * dsScale));
            }

            lowResBitmap.ApplyTo(DownsampleNormal);

            // Downsample properly here.
            lowResBitmap.ApplyTo(DownsampleSmart);
            lowResBitmap.ApplyTo(Downsample3);
            lowResBitmap.ApplyTo(Downsample4);

            // Intrinsic Image Algorithm starts here.

            // First attempt: Simple grayscale.
            var lowResBmLuma = new Bitmap(wHalf, hHalf, PixelFormats.Gray8);
            var lowResBmChroma = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);

            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
            {
                var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple(); // With gamma.
                var (r2, g2, b2) = ColorConversion.RemoveGamma(r / 255.0, g / 255.0, b / 255.0); // Without gamma.
                var luma = ColorConversion.GetGrayscale(r2, g2, b2) * 2.0 * MAX_LUMA; // Without gamma.
                ApplyToPixel(lowResBmChroma, lowResBmLuma, x, y, r2, g2, b2, luma); // Pass all values without gamma.
            }

            lowResBmLuma.ApplyTo(LowResImage1);
            lowResBmChroma.ApplyTo(LowResImage2);

            // Second attempt: paper implementation with two iterations.
            var LCalc = new LocalAreaMatrix(wHalf, hHalf);
            var L = LCalc.ComputeL((x, y) =>
            {
                var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple();
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
                        GS.ClampAll(MIN_LUMA, MAX_LUMA);
                    });

                    foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
                    {
                        var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple(); // With gamma.
                        var (r2, g2, b2) = ColorConversion.RemoveGamma(r / 255.0, g / 255.0, b / 255.0); // Without gamma.
                        var luma = iterVal[y * wHalf + x]; // Without gamma.
                        ApplyToPixel(iterBmChroma, iterBmLuma, x, y, r2, g2, b2, luma); // Pass all values without gamma.
                    }
                }
            }

            // Set camera.
            mCamera.UpdatePosition();

            // Create plotting models.
            mPlotting.CreateAxes();
            
            // Downsampling algorithm here.
            var rand = new Random(123456789);
            var bytes = new byte[3 * 16];
            rand.NextBytes(bytes);

            var data = new (byte, byte, byte)[16];
            var dataPoints = new Point3D[data.Length];
            for (int i = 0; i < bytes.Length; i += 3)
            {
                data[i / 3] = (bytes[i], bytes[i + 1], bytes[i + 2]);
                dataPoints[i / 3] = mPlotting.AddRGBPoint(bytes[i], bytes[i + 1], bytes[i + 2]);
            }

            var means = new (byte, byte, byte)[]
            {
                (0, 0, 0),
                (255, 0, 0),
                (0, 255, 0),
                (0, 0, 255),
            };

            var meanUpdaters = new Func<byte, byte, byte, Point3D>[means.Length];
            var meanPoints = new Point3D[means.Length];

            for (int i = 0; i < 4; i++)
            {
                var (r, g, b) = means[i];
                meanUpdaters[i] = mPlotting.AddRGBMean();
                meanPoints[i] = meanUpdaters[i](r, g, b);
            }

            var clustering = new Cluster(data, means);
            var removeLines = new List<Action>();

            /*
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    mDispatcher.Invoke(() =>
                    {
                        mCamera.AddRot(0.0, -0.001);
                        mCamera.UpdatePosition();
                    });
                }
            });
            */

            const int NUM_ITER = 10;
            for (int iter = 0; iter < NUM_ITER; iter++)
            {
                // Add one line for each point.
                for (int i = 0; i < data.Length; i++)
                {
                    var A = meanPoints[clustering.GetMeanIndexForPointIndex(i)];
                    var B = dataPoints[i];

                    var model = Line.Between(A, B);
                    ModelGroup.Children.Add(model);
                    removeLines.Add(() => ModelGroup.Children.Remove(model));
                }

                LogText2.Text = $"Mean Iteration: {iter + 1}. ";

                if (iter == NUM_ITER - 1)
                {
                    return;
                }

                await Task.Delay(4000);
                if (!clustering.Iterate())
                {
                    LogText2.Text += "Converged. ";
                    return;
                }

                // Update the means in the UI.
                for (int i = 0; i < means.Length; i++)
                {
                    var (r, g, b) = means[i];
                    meanPoints[i] = meanUpdaters[i](r, g, b);
                }

                // Clear the lines that are now invalid.
                removeLines.ForEach(x => x());
                removeLines.Clear();
            }
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
    }
}
