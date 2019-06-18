using System.Net;

namespace Syrup.Core._Infrastructure.Misc
{
    public class Utility
    {
        public static WebClient CreateWebClient(string key)
        {
            // WHY DOESNT IT JUST DO THISSSSSSSS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
            var ret = new WebClient();
            var wp = WebRequest.DefaultWebProxy;
            if (wp != null)
            {
                wp.Credentials = CredentialCache.DefaultCredentials;
                ret.Proxy = wp;
            }
           // ret.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + key);
            return ret;
        }
    }
}