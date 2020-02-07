using System;

namespace Player.Models
{
    public struct Mesh
    {
        public int nVertices;
        public IntPtr verticesWithColors;

        public int nTriangles;
        public IntPtr triangles;
    }
}
