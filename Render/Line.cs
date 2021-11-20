using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScatterPlotTool.Render
{
    internal class Line
    {
        public static GeometryModel3D Between(Point3D A, Point3D B)
        {
            var mesh = Models.CreateCube(0.5);
            var model = Models.CreateModel(mesh, new SolidColorBrush(Colors.Gray));

            var diff = B - A;
            var center = A + 0.5 * diff;
            var length = diff.Length;

            var X = new Vector3D(1.0, 0.0, 0.0);
            var crossProduct = Vector3D.CrossProduct(X, diff);
            var angle = Vector3D.AngleBetween(X, diff);

            var transformGroup = new Transform3DGroup();

            transformGroup.Children.Add(new ScaleTransform3D(length, 0.01, 0.01));
            transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(crossProduct, angle))); // Rotate around cross product of vectors.
            transformGroup.Children.Add(new TranslateTransform3D(center.X, center.Y, center.Z));

            model.Transform = transformGroup;

            return model;
        }
    }
}
