using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Syrup.Core.Server.Models;
using Syrup.Core._Infrastructure.Global;

namespace Syrup.Core.Server.Logic
{
    public interface IReleaseInfoService
    {
        Task<List<ReleaseInfo>> GetReleaseInfoAsync(string url);
        Task FetchReleaseInfoAsync();
        List<ReleaseInfo> GetCurrentReleaseInfo();
    }

    public class ReleaseInfoService : IReleaseInfoService
    {

        private readonly IFileDownloader _fileDownloader;
        private readonly Registry _registry;
        private List<ReleaseInfo> _relaseInfos = new List<ReleaseInfo>();

        public ReleaseInfoService(Registry registry, IFileDownloader fileDownloader)
        {
            _registry = registry;
            _fileDownloader = fileDownloader;
        }

        public async Task<List<ReleaseInfo>> GetReleaseInfoAsync(string url)
        {
         
            var t = await _fileDownloader.DownloadTextAsync(url);
            var r = JsonConvert.DeserializeObject<List<ReleaseInfo>>(t);
           
            return r;
        }


        public async Task FetchReleaseInfoAsync()
        {
            var t = await _fileDownloader.DownloadTextAsync(_registry.ReleaseInfoUrl);
            _relaseInfos = JsonConvert.DeserializeObject<List<ReleaseInfo>>(t);
        }

        public List<ReleaseInfo> GetCurrentReleaseInfo()
        {
            return _relaseInfos;
        }
    }
}