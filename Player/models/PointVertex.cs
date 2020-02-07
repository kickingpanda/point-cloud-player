namespace Player.Models
{
    public class PointVertex
    {
        public Rgb Color { get; set; }
        public Point3f Vertex { get; set; }
        public int GroupId { get; set; }

        public PointVertex (byte[] colorPts, float[] vertexPts)
        {
            Color = new Rgb()
            {
                R = colorPts[0],
                G = colorPts[1],
                B = colorPts[2]
            };
            Vertex = new Point3f()
            {
                X = vertexPts[0],
                Y = vertexPts[1],
                Z = vertexPts[2]
            };
            GroupId = 0;
        }
    }
}
