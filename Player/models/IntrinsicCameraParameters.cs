using System;

namespace Player.Models
{
    [Serializable]
    public class IntrinsicCameraParameters
    {
        public float cx, cy;        // principal points
        public float fx, fy;        // focal lengths
        public float r2, r4, r6;    // camera radial distortion parameters (second, fourth and sixth order)
    }
}
