using System;
using System.Windows.Media.Media3D;

namespace ScatterPlotTool
{
    internal class Camera
    {
        private readonly PerspectiveCamera mCamera;

        public double Phi = 0; // [-180, 180].
        public double Theta = 0; // [-180, 180].
        public double RadiusToCenter = 4.0;

        public Camera(PerspectiveCamera camera)
        {
            mCamera = camera;
            mCamera.FieldOfView = 60;
        }

        public void UpdatePosition()
        {
            // Calculate the camera's position in Cartesian coordinates.
            double y = RadiusToCenter * Math.Sin(Phi);
            double hyp = RadiusToCenter * Math.Cos(Phi);
            double x = hyp * Math.Sin(Theta);
            double z = hyp * Math.Cos(Theta);

            mCamera.Position = new Point3D(x, y, z);

            // Look toward the origin.
            mCamera.LookDirection = new Vector3D(-x, -y, -z);

            // Set the Up direction.
            mCamera.UpDirection = new Vector3D(0.0, 1.0, 0.0);
        }

        public void AddRot(double difPhi, double difTheta)
        {
            Phi += difPhi;
            Theta += difTheta;

            // Lock.
            if (Phi < -0.5 * Math.PI)
            {
                Phi = -0.5 * Math.PI;
            }

            if (Phi > 0.5 * Math.PI)
            {
                Phi = 0.5 * Math.PI;
            }

            // Wrap around.
            if (Theta < -Math.PI)
            {
                Theta += 2.0 * Math.PI;
            }

            if (Theta > Math.PI)
            {
                Theta -= 2.0 * Math.PI;
            }
        }

        public void AddRadius(double difRadius)
        {
            RadiusToCenter += difRadius;
        }
    }
}
