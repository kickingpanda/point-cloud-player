using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Player.Utils
{
    public static class LoadSaveFrame
    {
        private static readonly string _dir = Path.GetDirectoryName(Application.ExecutablePath);

        public static byte[] LoadColorsFromBinFile(string filePath)
        {
            using (var br = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int length = (int)br.BaseStream.Length;
                return br.ReadBytes(length).ToArray();
            }
        }

        public static float[] LoadVerticesFromBinFile(string filePath)
        {
            var data = new List<float>();
            using (var br = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                int pos = 0;
                int length = (int)br.BaseStream.Length;
                while (pos < length)
                {
                    var fl = br.ReadSingle();
                    data.Add(fl);
                    pos += sizeof(float);
                }
            }

            return data.ToArray();
        }
    }
}
