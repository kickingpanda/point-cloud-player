using System;

namespace Player.Models
{
    [Serializable]
    public class MarkerPose
    {
        public AffineTransform pose { get; set; }
        public int id { get; set; }

        public MarkerPose()
        {
            pose = new AffineTransform();
            id = 3;
            UpdateRotationMatrix();
        }

        public void SetOrientation(float X, float Y, float Z)
        {
            r[0] = X;
            r[1] = Y;
            r[2] = Z;

            UpdateRotationMatrix();
        }

        public void GetOrientation(out float X, out float Y, out float Z)
        {
            X = r[0];
            Y = r[1];
            Z = r[2];
        }

        private void UpdateRotationMatrix()
        {
            float radX = r[0] * (float)Math.PI / 180.0f;
            float radY = r[1] * (float)Math.PI / 180.0f;
            float radZ = r[2] * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(radZ);
            float c2 = (float)Math.Cos(radY);
            float c3 = (float)Math.Cos(radX);
            float s1 = (float)Math.Sin(radZ);
            float s2 = (float)Math.Sin(radY);
            float s3 = (float)Math.Sin(radX);

            //Z Y X rotation
            pose.R[0, 0] = c1 * c2;
            pose.R[0, 1] = c1 * s2 * s3 - c3 * s1;
            pose.R[0, 2] = s1 * s3 + c1 * c3 * s2;
            pose.R[1, 0] = c2 * s1;
            pose.R[1, 1] = c1 * c3 + s1 * s2 * s3;
            pose.R[1, 2] = c3 * s1 * s2 - c1 * s3;
            pose.R[2, 0] = -s2;
            pose.R[2, 1] = c2 * s3;
            pose.R[2, 2] = c2 * c3;
        }

        private float[] r = new float[3];
    }
}
