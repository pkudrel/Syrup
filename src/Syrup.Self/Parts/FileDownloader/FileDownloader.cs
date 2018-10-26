using System;
using System.Net;
using System.Threading.Tasks;
using Syrup.Self.Parts.LocalWebClient;
using Syrup.Self.Parts.Log;

namespace Syrup.Self.Parts.FileDownloader
{
    public interface IFileDownloader
    {
        Task DownloadFile(string url, string targetFile, Action<int> progress);
        Task<byte[]> DownloadUrl(string url);
        Task<string> DownloadTextAsync(string url);
    }

    public class FileDownloader : IFileDownloader
    {
        private static readonly Logger _log = Logger.Instance;
        private readonly WebClient _wc;


        public FileDownloader()
        {
            _wc = WebClientService.Instance.Wc;
        }

        public async Task DownloadFile(string url, string targetFile, Action<int> progress)
        {
            var failedUrl = default(string);

            var lastSignalled = DateTime.MinValue;
            _wc.DownloadProgressChanged += (sender, args) =>
            {
                var now = DateTime.Now;

                if (now - lastSignalled > TimeSpan.FromMilliseconds(500))
                {
                    lastSignalled = now;
                    progress(args.ProgressPercentage);
                }
            };

            retry:
            try
            {
                _log.Info("Downloading file: " + (failedUrl ?? url));
                await _wc.DownloadFileTaskAsync(failedUrl ?? url, targetFile);
            }
            catch (Exception)
            {
                if (failedUrl != null) throw;

                failedUrl = url.ToLower();
                progress(0);
                goto retry;
            }
        }

        public async Task<byte[]> DownloadUrl(string url)
        {
            var failedUrl = default(string);

            retry:
            try
            {
                _log.Info("Downloading url: " + (failedUrl ?? url));

                return await _wc.DownloadDataTaskAsync(failedUrl ?? url);
            }
            catch (Exception)
            {
                // NB: Some super brain-dead services are case-sensitive yet 
                // corrupt case on upload. I can't even.
                if (failedUrl != null) throw;

                failedUrl = url.ToLower();
                goto retry;
            }
        }

        public async Task<string> DownloadTextAsync(string url)
        {
            var failedUrl = default(string);

            retry:
            try
            {
                _log.Info("Downloading url: " + (failedUrl ?? url));

                return await _wc.DownloadStringTaskAsync(failedUrl ?? url);
            }
            catch (Exception)
            {
                // NB: Some super brain-dead services are case-sensitive yet 
                // corrupt case on upload. I can't even.
                if (failedUrl != null) throw;

                failedUrl = url.ToLower();
                goto retry;
            }
        }
    }
}