using System.Net;

namespace Syrup.Self.Parts.LocalWebClient
{
    public sealed class WebClientService
    {
        public WebClient Wc { get; }
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static WebClientService()
        {
        }

        private WebClientService()
        {
            Wc = Wc ?? (Wc = CreateWebClient());
        }


        public static WebClientService Instance { get; } = new WebClientService();

        private static WebClient CreateWebClient()
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
            return ret;
        }
    }
}