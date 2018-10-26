using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Syrup.Self.Common
{
    public class Io
    {
        public static void DeleteFileAndWait(string filepath, int timeout = 30000)
        {
            var dirName = Path.GetDirectoryName(filepath);
            if (dirName == null) return;
            using (var fw = new FileSystemWatcher(dirName, Path.GetFileName(filepath)))
            {
                using (var mre = new ManualResetEventSlim())
                {
                    fw.EnableRaisingEvents = true;
                    fw.Deleted += (sender, e) =>
                    {
                        mre?.Set();
                    };
                    File.Delete(filepath);
                    mre.Wait(timeout);
                }
            }
        }

        public static async Task MoveFile(string sourceFile, string destinationFile)
        {
            using (FileStream sourceStream = File.Open(sourceFile, FileMode.Open))
            {
                using (FileStream destinationStream = File.Create(destinationFile))
                {
                    await sourceStream.CopyToAsync(destinationStream);

                        sourceStream.Close();
                        File.Delete(sourceFile);
                    
                }
            }
        }

        public static void CreateDirIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}