using System;
using System.Collections.Generic;
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
        private readonly MeshGeometry3D mPlaneMesh = Models.CreatePlane(1.0);
        private readonly MeshGeometry3D mCubeMesh = Models.CreateCube(0.025);
        private readonly MeshGeometry3D mTrihedronMesh = Models.CreateTrihedron(0.1);
        private readonly List<Action> mClearRGBPoints = new();

        public Plotting(Model3DCollection modelCollection)
        {
            mModelCollection = modelCollection;
        }

        public void ClearAll()
        {
            mModelCollection.Clear();
        }

        public void CreateAxes()
        {
            // Create texture.
            Grid.CreateRB(GRID_RB_PNG);
            Grid.CreateWhite(GRID_WHITE_PNG);

            // Create models.
            for (int i = 0; i < 3; i++)
            {
                var model = Models.CreateModel(mPlaneMesh, new ImageBrush(new BitmapImage(new Uri(
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
                    transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1.0, 0.0, 0.0), 90)));
                }
                else if (i == 2)
                {
                    transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0.0, 0.0, 1.0), -90)));
                }

                model.Transform = transformGroup;
                mModelCollection.Add(model);
            }
        }

        // These points are fixed.
        public Point3D AddRGBPoint(byte r, byte g, byte b)
        {
            var (x, y, z) = ConvertRGBtoXYZ(r, g, b);

            var model = Models.CreateModel(mCubeMesh, new SolidColorBrush(Color.FromRgb(r, g, b)));
            model.Transform = new TranslateTransform3D(x, y, z);
            mModelCollection.Add(model);
            mClearRGBPoints.Add(() => mModelCollection.Remove(model));

            return new Point3D(x, y, z);
        }

        public void ClearRGBPoints() => mClearRGBPoints.CallAllThenClear();

        // Return a lambda that can modify the position and color.
        public Func<byte, byte, byte, Point3D> AddRGBMean()
        {
            var brush = new SolidColorBrush(Colors.Gray);
            var model = Models.CreateModel(mTrihedronMesh, brush);

            mModelCollection.Add(model);

            return (r, g, b) =>
            {
                var (x, y, z) = ConvertRGBtoXYZ(r, g, b);

                brush.Color = Color.FromRgb(r, g, b);
                model.Transform = new TranslateTransform3D(x, y, z);

                return new Point3D(x, y, z);
            };
        }

        private static (double, double, double) ConvertRGBtoXYZ(double r, double g, double b)
        {
            var x = 2.0 * r / 255.0 - 1.0;
            var y = 2.0 * g / 255.0 - 1.0;
            var z = 2.0 * b / 255.0 - 1.0;

            return (x, y, z);
        }
    }
}
