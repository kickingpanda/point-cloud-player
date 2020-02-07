using System;

namespace Player.Models
{
    [Serializable]
    public class AffineTransform
    {
        public float[,] R { get; set; }
        public float[] t { get; set; }

        public AffineTransform()
        {
            R = new float[3, 3];
            t = new float[3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    R[i, j] = i == j ? 1 : 0;
                }
                t[i] = 0;
            }
        }
    }
}
