using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Player.Utils
{
    // TODO MOVE
    public class JsonDataPoints {
        public Dictionary<int, float[]> C { get; set; }
        public Dictionary<int, float[]> V { get; set; }

        public JsonDataPoints()
        {
            C = new Dictionary<int, float[]>();
            V = new Dictionary<int, float[]>();
        }
    }

    public class FilePackaging
    {
        /// <summary>
        /// Compresses files into a package to be shared for replay
        /// </summary>
        /// <param name="targetFolder">Location of folder for packaging</param>
        /// <param name="fileName">Filename to save as</param>
        /// <param name="deleteFiles">remove files after packaging</param>
        /// <returns>File path of package</returns>
        public static string CompressFilesIntoPackage(string targetFolder, string fileName, bool deleteFiles=true)
        {
            // pack files into a create .tar.gz file
            using (FileStream outStream = File.Create(Path.Combine(Directory.GetParent(targetFolder).Parent.FullName, $"{fileName}.holo")))
            using (GZipOutputStream gzoStream = new GZipOutputStream(outStream))
            using (TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream))
            {
                tarArchive.RootPath = targetFolder.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                {
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);
                }
                _AddDirectoryFilesToTar(tarArchive, targetFolder, true);
            }

            // remove source files if directed
            if (deleteFiles)
            {
                DeleteTempFiles(targetFolder);
            }

            return Path.Combine(targetFolder, fileName);
        }

        public static string PackageAsJson(string targetFolder, string fileName, bool deleteFiles = true)
        {
            var jsonObject = new JsonDataPoints();
            string[] filenames = Directory.GetFiles(targetFolder);
            int cFrame = 0;
            int vFrame = 0;
            foreach (string fn in filenames.OrderBy(f=>f).Take(300))
            {
                var txt = File.ReadAllText(fn).TrimEnd(',');
                if (fn.EndsWith("_Colors.txt"))
                {
                    jsonObject.C[cFrame] = txt.Split(',').Select(c => float.Parse(c)/255f).ToArray();
                    cFrame++;
                };
                if (fn.EndsWith("_Vertices.txt")) {
                    jsonObject.V[vFrame] = txt.Split(',').Select(float.Parse).ToArray();
                    vFrame++;
                };
            }

            // save
            string json = JsonConvert.SerializeObject(jsonObject, Formatting.None);
            using (var fs = File.Open(Path.Combine(targetFolder, fileName), File.Exists(Path.Combine(targetFolder, fileName)) ? FileMode.Append : FileMode.OpenOrCreate))
            using (var sw = new StreamWriter(fs))
            {
                sw.Write(json);
            }

            // remove source files if directed
            if (deleteFiles)
            {
                DeleteTempFiles(targetFolder);
            }
            return Path.Combine(targetFolder, fileName);
        }

        /// <summary>
        /// Decompresses a file to read contents for replay
        /// </summary>
        /// <param name="filePath">Location of file to decompress</param>
        /// <param name="targetFolder">Location to unpack (should be a temp folder)</param>
        /// <returns>array of files unpacked</returns>
        public static string[] UnpackCompressedFiles(string filePath, string targetFolder)
        {
            // check if data was already previously unpacked
            if (Directory.Exists(targetFolder) && new DirectoryInfo(targetFolder).GetFiles().Length > 25)
            {
                return new DirectoryInfo(targetFolder)
                    .GetFiles()
                    .Select(file => file.FullName)
                    .ToArray();
            }

            using (FileStream inStream = File.OpenRead(filePath))
            using (GZipInputStream gzipStream = new GZipInputStream(inStream))
            using (TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream))
            {
                tarArchive.ExtractContents(targetFolder);
            }

            return new DirectoryInfo(targetFolder)
                .GetFiles()
                .Select(file => file.FullName)
                .ToArray();
        }

        public static string FastZipFilesIntoPackage(string targetFolder, string fileName, bool deleteFiles = true)
        {
            FastZip fastZip = new FastZip();
            fastZip.CreateZip(Path.Combine(Directory.GetParent(targetFolder).Parent.FullName, $"{fileName}.holo"), targetFolder, true, null);
            
            // remove source files if directed
            if (deleteFiles)
            {
                DeleteTempFiles(targetFolder);
            }

            return Path.Combine(targetFolder, fileName);
        }

        public static string[] FastUnzipFiles(string filePath, string targetFolder)
        {
            // check if data was already previously unpacked
            if (Directory.Exists(targetFolder) && new DirectoryInfo(targetFolder).GetFiles().Length > 25)
            {
                return new DirectoryInfo(targetFolder)
                    .GetFiles()
                    .Select(file => file.FullName)
                    .ToArray();
            }

            FastZip fastZip = new FastZip();
            // Will overwrite
            fastZip.ExtractZip(filePath, targetFolder, null);

            return new DirectoryInfo(targetFolder)
                .GetFiles()
                .Select(file => file.FullName)
                .ToArray();
        }

        /// <summary>
        /// Remove all files from directory except holo files
        /// </summary>
        /// <param name="sourceDirectory"></param>
        public static void DeleteTempFiles(string sourceDirectory)
        {
            DirectoryInfo directorySelected = new DirectoryInfo(sourceDirectory);
            foreach (FileInfo fileToDelete in directorySelected.GetFiles())
            {
                if (!fileToDelete.FullName.EndsWith(".holo") && !fileToDelete.FullName.EndsWith(".json"))
                {
                    File.Delete(fileToDelete.FullName);
                };
            };
        }

        #region PrivateMethods

        // SOURCE: https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#user-content--create-a-tgz-targz
        private static void _AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory, bool recurse)
        {
            // Optionally, write an entry for the directory itself.
            // Specify false for recursion here if we will add the directory's files individually.
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);

            // Write each file to the tar.
            string[] filenames = Directory.GetFiles(sourceDirectory);
            foreach (string filename in filenames.Where(fn => !fn.EndsWith(".holo")))
            {
                tarEntry = TarEntry.CreateEntryFromFile(filename);
                if (tarEntry.Name.Split(@"/").Length > 1)
                {
                    tarEntry.Name = tarEntry.Name.Split(@"/")[^1];
                }
                tarArchive.WriteEntry(tarEntry, true);
            }

            if (recurse)
            {
                string[] directories = Directory.GetDirectories(sourceDirectory);
                foreach (string directory in directories)
                {
                    _AddDirectoryFilesToTar(tarArchive, directory, recurse);
                }
            }
        }

        #endregion
    }
}
