using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syrup.Self.Common;
using Syrup.Self.Parts.Helpers;
using Syrup.Self.Parts.Sem;
using Syrup.Self.Parts.ServerRealses;

namespace Syrup.Self.Parts.FileDownloader
{
    public class SyrupDowloader
    {
        private readonly INotifier _notifier;
        private readonly FileDownloader _fd;

        public SyrupDowloader(INotifier notifier)
        {
            _notifier = notifier;
            _fd = new FileDownloader();
        }

        public async Task<List<ReleaseInfo>> GetListSyrupFileInfos(string url)
        {
            var json = await DownloadString(url);
            var l = JsonSerializer<List<ReleaseInfo>>.DeSerialize(json);
            return l;
        }

        public async Task<ReleaseInfo> GetNewestSyrupFileInfo(string url)
        {
            var json = await DownloadString(url);
            var l = JsonSerializer<List<ReleaseInfo>>.DeSerialize(json);
            return l.OrderByDescending(x => SemVersion.Parse(x.SemVer)).FirstOrDefault();
        }


        public async Task DownloadFile(string url, string file)
        {
            _notifier.SetProgress(100);
            await _fd.DownloadFile(url, file, i =>
            {
                _notifier.UpdateProgress(i);
            });
            _notifier.UpdateProgress(100);
           
        }

        private Task<string> DownloadString(string url)
        {
            return _fd.DownloadTextAsync(url);
        }
    }
}