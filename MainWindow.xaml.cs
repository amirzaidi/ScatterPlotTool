using ScatterPlotTool.Algorithm;
using ScatterPlotTool.Render;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
            // Set camera.
            mCamera.UpdatePosition();

            // Create plotting models.
            mPlotting.CreateAxes();

            /*
            var (Ax, Ay, Az) = mPlotting.AddRGBPoint(127, 127, 127);
            var (Bx, By, Bz) = mPlotting.AddRGBPoint(255, 255, 255);

            var A = new Point3D(Ax, Ay, Az);
            var B = new Point3D(Bx, By, Bz);

            var model = Line.Between(B, A);
            ModelGroup.Children.Add(model);
            */
            
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

                LogText1.Text = $"Mean Iteration: {iter + 1}. ";

                if (iter == NUM_ITER - 1)
                {
                    return;
                }

                await Task.Delay(4000);
                if (!clustering.Iterate())
                {
                    LogText1.Text += "Converged. ";
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
