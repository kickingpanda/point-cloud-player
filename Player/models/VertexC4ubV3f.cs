namespace Player.Models
{
    public struct VertexC4ubV3f
    {
        public byte R, G, B, A;
        public float X, Y, Z;

        /*public VertexC4ubV3s toVertexC4ubV3s()
        {
            VertexC4ubV3s out_v;
            out_v.R = R;
            out_v.G = G;
            out_v.B = B;
            out_v.A = A;
            out_v.X = (short)(X * 1000);
            out_v.Y = (short)(Y * 1000);
            out_v.Z = (short)(Z * 1000);
            return out_v;
        }*/

        //public static int SizeInBytes = 16;
    }
}
