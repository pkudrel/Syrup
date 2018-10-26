using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syrup.Self.Common;
using Syrup.Self.Parts.FileDownloader;
using Syrup.Self.Parts.Sem;
using Syrup.Self.Parts.ServerRealses;
using Xunit;

namespace Syrup.Tests.FileDownloaderTests
{
    public class SimpleFileDownloaderTests
    {
        private FileDownloader _fd;
        private const string url = "https://clients.deneblab.com/syrup-api-main/index";
        public SimpleFileDownloaderTests()
        {
            _fd = new FileDownloader();
        }

        [Fact]
        public async Task SimpleTest()
        {
            var json = await DownloadString(url);
            var l = JsonSerializer<List<ReleaseInfo>>.DeSerialize(json);
            var max = l.OrderByDescending(x => SemVersion.Parse(x.SemVer));
        }

        Task<string> DownloadString(string url)
        {
            return _fd.DownloadTextAsync(url);
        }
    }
}