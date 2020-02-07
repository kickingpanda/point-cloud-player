using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace Player.Utils
{
    public class FileExport
    {
        public static void SaveBinaryToPly(string filename, List<Single> vertices, List<byte> colors)
        {
            int nVertices = vertices.Count / 3;

            using (FileStream fileStream = File.Open(filename, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                // Write PLY file header
                streamWriter.WriteLine("ply\nformat binary_little_endian 1.0");
                streamWriter.Write($"element vertex {nVertices}\n");
                streamWriter.Write("property float x\nproperty float y\nproperty float z\nproperty uchar red\nproperty uchar green\nproperty uchar blue\nend_header\n");
                streamWriter.Flush();

                // Write Vertex and color data
                for (var j = 0; j < vertices.Count / 3; j++)
                {
                    for (var k = 0; k < 3; k++)
                    {
                        binaryWriter.Write(vertices[j * 3 + k]);
                    }
                    for (var k = 0; k < 3; k++)
                    {
                        byte temp = colors[j * 3 + k];
                        binaryWriter.Write(temp);
                    }
                }

                // flush
                streamWriter.Flush();
                binaryWriter.Flush();
            }
        }

        public static void SaveStreamToPly(string filename, List<Single> vertices, List<byte> colors)
        {
            int nVertices = vertices.Count / 3;

            using (FileStream fileStream = File.Open(filename, FileMode.Create))
            using (var streamWriter = new StreamWriter(fileStream))
            {
                // Write PLY file header
                streamWriter.WriteLine("ply\nformat ascii 1.0\n");
                streamWriter.Write($"element vertex {nVertices}\n");
                streamWriter.Write("property float x\nproperty float y\nproperty float z\nproperty uchar red\nproperty uchar green\nproperty uchar blue\nend_header\n");
                streamWriter.Flush();

                // Write Vertex and color data
                for (var j = 0; j < vertices.Count / 3; j++)
                {
                    var s = "";
                    for (var k = 0; k < 3; k++)
                    {
                        s += vertices[j * 3 + k].ToString(CultureInfo.InvariantCulture) + " ";
                    }
                    for (var k = 0; k < 3; k++)
                    {
                        s += colors[j * 3 + k].ToString(CultureInfo.InvariantCulture) + " ";
                    }
                    streamWriter.WriteLine(s);
                }

                // flush
                streamWriter.Flush();
            }
        }
    }
}
