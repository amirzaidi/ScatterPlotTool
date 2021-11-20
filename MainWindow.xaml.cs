using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set camera.
            mCamera.UpdatePosition();

            // Create plotting models.
            mPlotting.CreateAxes();

            var rand = new Random(123456789);
            var bytes = new byte[3 * 16];
            rand.NextBytes(bytes);
            for (int i = 0; i < bytes.Length; i += 3)
            {
                mPlotting.AddRGBPoint(bytes[i], bytes[i + 1], bytes[i + 2]);
            }

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
