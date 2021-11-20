using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ScatterPlotTool
{
    internal class Models
    {
        public static MeshGeometry3D CreateTrihedron(double size = 1.0)
        {
            var mesh = new MeshGeometry3D();

            var top = new Point3D(0.0, size, 0.0);

            var a = new Point3D(size, -0.5 * size, 0.0);
            var b = new Point3D(-0.5 * size, -0.5 * size, -0.5 * Math.Sqrt(3) * size);
            var c = new Point3D(-0.5 * size, -0.5 * size, 0.5 * Math.Sqrt(3) * size);

            mesh.Positions.Add(top);
            mesh.TextureCoordinates.Add(new Point(0.0, 0.0));

            mesh.Positions.Add(a);
            mesh.TextureCoordinates.Add(new Point(0.0, 1.0));

            mesh.Positions.Add(b);
            mesh.TextureCoordinates.Add(new Point(1.0, 0.0));

            mesh.Positions.Add(c);
            mesh.TextureCoordinates.Add(new Point(1.0, 1.0));

            // Face 1.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 4);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);

            // Face 2.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 4);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);

            // Face 3.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 4);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);

            // Bottom.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 1);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);

            return mesh;
        }

        public static MeshGeometry3D CreatePlane(double size = 1.0)
        {
            var mesh = new MeshGeometry3D();

            var BL = new Point3D(-size, 0.0, -size);
            var TL = new Point3D(-size, 0.0, size);
            var BR = new Point3D(size, 0.0, -size);
            var TR = new Point3D(size, 0.0, size);

            mesh.Positions.Add(BL);
            mesh.TextureCoordinates.Add(new Point(0.0, 0.0));

            mesh.Positions.Add(TL);
            mesh.TextureCoordinates.Add(new Point(0.0, 1.0));

            mesh.Positions.Add(BR);
            mesh.TextureCoordinates.Add(new Point(1.0, 0.0));

            mesh.Positions.Add(TR);
            mesh.TextureCoordinates.Add(new Point(1.0, 1.0));

            // Face 1.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 4);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);

            // Face 2.
            mesh.TriangleIndices.Add(mesh.Positions.Count - 2);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 3);
            mesh.TriangleIndices.Add(mesh.Positions.Count - 1);

            return mesh;
        }

        public static MeshGeometry3D CreateCube(double size = 1.0)
        {
            var mesh = new MeshGeometry3D();

            var BLA = AddToGeometry(mesh, new Point3D(-size, -size, -size), new Point(0.0, 0.0));
            var TLA = AddToGeometry(mesh, new Point3D(-size, -size, size), new Point(0.0, 1.0));
            var BRA = AddToGeometry(mesh, new Point3D(size, -size, -size), new Point(1.0, 0.0));
            var TRA = AddToGeometry(mesh, new Point3D(size, -size, size), new Point(1.0, 1.0));

            var BLB = AddToGeometry(mesh, new Point3D(-size, size, -size), new Point(0.0, 0.0));
            var TLB = AddToGeometry(mesh, new Point3D(-size, size, size), new Point(0.0, 1.0));
            var BRB = AddToGeometry(mesh, new Point3D(size, size, -size), new Point(1.0, 0.0));
            var TRB = AddToGeometry(mesh, new Point3D(size, size, size), new Point(1.0, 1.0));

            // Lower Face 1.
            mesh.TriangleIndices.Add(BLA);
            mesh.TriangleIndices.Add(BRA);
            mesh.TriangleIndices.Add(TLA);

            // Lower Face 2.
            mesh.TriangleIndices.Add(BRA);
            mesh.TriangleIndices.Add(TRA);
            mesh.TriangleIndices.Add(TLA);

            // Upper Face 1.
            mesh.TriangleIndices.Add(BLB);
            mesh.TriangleIndices.Add(TLB);
            mesh.TriangleIndices.Add(BRB);

            // Upper Face 2.
            mesh.TriangleIndices.Add(BRB);
            mesh.TriangleIndices.Add(TLB);
            mesh.TriangleIndices.Add(TRB);

            // Left Face 1.
            mesh.TriangleIndices.Add(BLA);
            mesh.TriangleIndices.Add(TLA);
            mesh.TriangleIndices.Add(TLB);

            // Left Face 2.
            mesh.TriangleIndices.Add(BLA);
            mesh.TriangleIndices.Add(TLB);
            mesh.TriangleIndices.Add(BLB);

            // Right Face 1.
            mesh.TriangleIndices.Add(BRA);
            mesh.TriangleIndices.Add(TRB);
            mesh.TriangleIndices.Add(TRA);

            // Right Face 2.
            mesh.TriangleIndices.Add(BRA);
            mesh.TriangleIndices.Add(BRB);
            mesh.TriangleIndices.Add(TRB);

            // Front Face 1.
            mesh.TriangleIndices.Add(BLA);
            mesh.TriangleIndices.Add(BLB);
            mesh.TriangleIndices.Add(BRB);

            // Front Face 2.
            mesh.TriangleIndices.Add(BLA);
            mesh.TriangleIndices.Add(BRB);
            mesh.TriangleIndices.Add(BRA);

            // Back Face 1.
            mesh.TriangleIndices.Add(TLA);
            mesh.TriangleIndices.Add(TRB);
            mesh.TriangleIndices.Add(TLB);

            // Back Face 2.
            mesh.TriangleIndices.Add(TLA);
            mesh.TriangleIndices.Add(TRA);
            mesh.TriangleIndices.Add(TRB);

            return mesh;
        }

        public static GeometryModel3D CreateModel(MeshGeometry3D mesh, Brush brush)
            => new(mesh, new DiffuseMaterial(brush));

        private static int AddToGeometry(MeshGeometry3D mesh, Point3D vertex, Point tex)
        {
            mesh.Positions.Add(vertex);
            mesh.TextureCoordinates.Add(tex);
            return mesh.Positions.Count - 1;
        }
    }
}
