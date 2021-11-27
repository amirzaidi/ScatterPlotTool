using ScatterPlotTool.Algorithm;
using ScatterPlotTool.Algorithm.Intrinsic;
using ScatterPlotTool.Algorithm.Matrix;
using ScatterPlotTool.Image;
using ScatterPlotTool.Render;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace ScatterPlotTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Plotting mPlotting;
        private readonly Camera mCamera;
        private readonly Dispatcher mDispatcher;

        public MainWindow()
        {
            InitializeComponent();
            mPlotting = new Plotting(ModelGroup.Children);
            mCamera = new Camera(Camera);
            mDispatcher = Application.Current.Dispatcher;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var fullResBitmap = new Bitmap("D:/Programs/DIPimage 2.9/images/flamingo.tif");
            FullResImage.Source = fullResBitmap.GetImage();
            FullResImage.Width = fullResBitmap.GetWidth();
            FullResImage.Height = fullResBitmap.GetHeight();
            LogText1.Text = "Full Resolution Image";

            /*
            var L = new LocalAreaMatrix(fullResBitmap.GetWidth(), fullResBitmap.GetHeight());
            L.ComputeL((x, y) =>
            {
                var pixels = fullResBitmap.GetPixels(x, y);
                var (r, g, b) = ((float)pixels[0], (float)pixels[1], (float)pixels[2]);
                return (r / 255f, g / 255f, b / 255f);
            });
            */

            var dsScale = 2;
            var (wHalf, hHalf) = (fullResBitmap.GetWidth() / dsScale, fullResBitmap.GetHeight() / dsScale);

            var lowResBitmap = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
            {
                lowResBitmap.SetPixels(x, y, pixels: fullResBitmap.GetPixels(x * dsScale, y * dsScale));
            }

            /*
            LowResImage1.Source = lowResBitmap.GetImage();
            LowResImage1.Width = wHalf;
            LowResImage1.Height = hHalf;

            // Duplicate for now.
            LowResImage2.Source = lowResBitmap.GetImage();
            LowResImage2.Width = wHalf;
            LowResImage2.Height = hHalf;
            */

            var byteArrLuma = new byte[1];
            var byteArrChroma = new byte[3];

            // First attempt: Simple grayscale.
            var lowResBmLuma = new Bitmap(wHalf, hHalf, PixelFormats.Gray8);
            var lowResBmChroma = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);
            foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
            {
                var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple();

                byteArrLuma[0] = (byte)Util.Clamp((r + g + b) / 3, 1, 255);
                lowResBmLuma.SetPixels(x, y, pixels: byteArrLuma);

                byteArrChroma[0] = (byte)Util.Clamp(r * 255 / byteArrLuma[0] / 2);
                byteArrChroma[1] = (byte)Util.Clamp(g * 255 / byteArrLuma[0] / 2);
                byteArrChroma[2] = (byte)Util.Clamp(b * 255 / byteArrLuma[0] / 2);
                lowResBmChroma.SetPixels(x, y, pixels: byteArrChroma);
            }

            LowResImage1.Source = lowResBmLuma.GetImage();
            LowResImage1.Width = wHalf;
            LowResImage1.Height = hHalf;

            LowResImage2.Source = lowResBmChroma.GetImage();
            LowResImage2.Width = wHalf;
            LowResImage2.Height = hHalf;

            // Second attempt: paper implementation with two iterations.
            var LCalc = new LocalAreaMatrix(wHalf, hHalf);
            var L = LCalc.ComputeL((x, y) =>
            {
                var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple();
                return (r / 255.0, g / 255.0, b / 255.0);
            });

            var GS = new GaussSeidel(L);
            var bZero = new double[wHalf * hHalf];

            foreach (var (lowResImageMono, lowResImageChroma, iterCount) in new[] {
                (LowResImage3, LowResImage4, 1),
                (LowResImage5, LowResImage6, 10),
                (LowResImage7, LowResImage8, 100)
            })
            {
                for (int i = 0; i < iterCount; i++)
                {
                    await Task.Run(() => GS.Iterate(bZero, row => LocalAreaMatrix.GetValidColumns(wHalf, hHalf, row)));
                }

                var iterVal = GS.GetInput();

                var iterBmMono = new Bitmap(wHalf, hHalf, PixelFormats.Gray8);
                var iterBmChroma = new Bitmap(wHalf, hHalf, PixelFormats.Bgr24);

                foreach (var (x, y) in CoordGenerator.Range2D(0, 0, wHalf, hHalf))
                {
                    var (r, g, b) = lowResBitmap.GetPixels(x, y).ToThreeTuple();

                    byteArrLuma[0] = (byte)Util.Clamp(iterVal[y * wHalf + x] * 128.0, 1.0, 255.0);
                    iterBmMono.SetPixels(x, y, pixels: byteArrLuma);

                    byteArrChroma[0] = (byte)Util.Clamp(r * 255 / byteArrLuma[0] / 2);
                    byteArrChroma[1] = (byte)Util.Clamp(g * 255 / byteArrLuma[0] / 2);
                    byteArrChroma[2] = (byte)Util.Clamp(b * 255 / byteArrLuma[0] / 2);
                    iterBmChroma.SetPixels(x, y, pixels: byteArrChroma);
                }

                lowResImageMono.Source = iterBmMono.GetImage();
                lowResImageMono.Width = wHalf;
                lowResImageMono.Height = hHalf;

                lowResImageChroma.Source = iterBmChroma.GetImage();
                lowResImageChroma.Width = wHalf;
                lowResImageChroma.Height = hHalf;
            }

            // Iterate over source.
            LogText3.Text = "Algorithm";

            // Set camera.
            mCamera.UpdatePosition();

            // Create plotting models.
            mPlotting.CreateAxes();
            
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
