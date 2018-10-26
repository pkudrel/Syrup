using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Syrup.Common.Io
{
    public class Misc
    {
        public static void CreateDirIfNotExist(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public static void RemoveFilesInDir(string path)
        {
            if (!Directory.Exists(path)) return;
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
        }

        public static void RemovedDir(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);
                foreach (var fi in dir.GetFiles())
                {
                    var numberTry = 3;
                    fi.IsReadOnly = false;
                    fi.Delete();
                    fi.Refresh();

                    while (fi.Exists && numberTry > 0)
                    {
                        Thread.Sleep(100);
                        fi.Refresh();
                        numberTry--;
                    }
                }

                foreach (var di in dir.GetDirectories())
                {
                    ClearFolder(di.FullName);
                    di.Delete();
                }
                dir.Delete();
            }
        }
        public static void CopyDirectory(string sourcePath, string destPath)
        {
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            foreach (var file in Directory.GetFiles(sourcePath))
            {
                {
                    var dest = Path.Combine(destPath, Path.GetFileName(file));
                    File.Copy(file, dest);
                }
            }

            foreach (var folder in Directory.GetDirectories(sourcePath))
            {
                if (folder != null)
                {
                    var dest = Path.Combine(destPath, Path.GetFileName(folder));
                    CopyDirectory(folder, dest);
                }
            }
        }
        public static void ClearFolder(string path)
        {
            var dir = new DirectoryInfo(path);

            foreach (var fi in dir.GetFiles())
            {
                fi.IsReadOnly = false;
                fi.Delete();
            }

            foreach (var di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }
        public static void RemoveFilesAndDirsInDir(string path)
        {
            if (!Directory.Exists(path)) return;
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        public static void RemoveFolder(string path)
        {
            if (Directory.Exists(path))
            {
                var dir = new DirectoryInfo(path);

                foreach (var fi in dir.GetFiles())
                {
                    fi.IsReadOnly = false;
                    fi.Delete();
                }

                foreach (var di in dir.GetDirectories())
                {
                    ClearFolder(di.FullName);
                    di.Delete();
                }

                dir.Delete();
            }
        }

        public static void RemoveFile(string path)
        {
            var fileInfo = new FileInfo(path);


            fileInfo.Delete();
        }

        public static string[] CleanEmptyLines(string[] lines)
        {
            return lines.Where(x => x.Trim().Length > 0).ToArray();
        }

        public static string[] CleanComments(string[] lines)
        {
            return lines.Where(x => !x.Trim().StartsWith("#")).ToArray();
        }

        public static void WriteJson(string path, object obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, json, Encoding.UTF8);
        }

        public static T ReadJson<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T ReadJsonSafe<T>(string path)
        {
            if (!File.Exists(path)) return default(T);

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}