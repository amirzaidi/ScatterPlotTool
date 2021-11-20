using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace ScatterPlotTool
{
    internal class Plotting
    {
        private const string GRID_WHITE_PNG = "grid_white.png";
        private const string GRID_RB_PNG = "grid_rb.png";

        private readonly Model3DCollection mModelCollection;

        public Plotting(Model3DCollection modelCollection)
        {
            mModelCollection = modelCollection;
        }

        public void CreateAxes()
        {
            // Create texture.
            GridBitmap.CreateRB(GRID_RB_PNG);
            GridBitmap.CreateWhite(GRID_WHITE_PNG);

            // Create models.
            for (int i = 0; i < 3; i++)
            {
                var mesh = Models.CreatePlane(1.0);
                var model = Models.CreateModel(mesh, new ImageBrush(new BitmapImage(new Uri(
                    i == 0
                        ? GRID_RB_PNG
                        : GRID_WHITE_PNG,
                    UriKind.Relative
                ))));
                model.BackMaterial = model.Material;

                var transformGroup = new Transform3DGroup();
                transformGroup.Children.Add(new TranslateTransform3D(0.0, -1.0, 0.0));
                if (i == 1)
                {
                    transformGroup.Children.Add(new RotateTransform3D
                    {
                        Rotation = new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), 90)
                    });
                }
                else if (i == 2)
                {
                    transformGroup.Children.Add(new RotateTransform3D
                    {
                        Rotation = new AxisAngleRotation3D(new Vector3D(0.0, 0.0, 1.0), -90)
                    });
                }

                model.Transform = transformGroup;
                mModelCollection.Add(model);
            }
        }

        public void AddRGBPoint(byte r, byte g, byte b)
        {
            // var mesh = Models.CreateTrihedron(0.075);
            var mesh = Models.CreateCube(0.025);
            var model = Models.CreateModel(mesh, new SolidColorBrush(Color.FromRgb(r, g, b)));
            model.Transform = new TranslateTransform3D(
                2.0 * r / 255.0 - 1.0,
                2.0 * g / 255.0 - 1.0,
                2.0 * b / 255.0 - 1.0
            );
            mModelCollection.Add(model);
        }
    }
}
