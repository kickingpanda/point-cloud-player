using System;

namespace Player.Models
{
    [Serializable]
    public class Frame
    {
        public int SocketCount { get; set; }
        public DateTime Timestamp { get; set; }
        public byte[] Colors { get; set; }
        public float[] Vertices { get; set; }
        public Body[] Bodies { get; set; }
        public AffineTransform[] CameraPoses { get; set; }

        public Frame()
        {
            SocketCount = 0;
            Timestamp = new DateTime();
            Colors = new byte[0];
            Vertices = new float[0];
            Bodies = new Body[0];
            CameraPoses = new AffineTransform[0];
        }
    }
}
