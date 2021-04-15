using System;
using Qlik.Engine;

namespace ConnectServerStaticHeader
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://myQlikSenseServer.myDomain.com/");

            var location = Location.FromUri(uri);
            // Defines the location as a static header connection via proxy. The headerUserId contains the user and
            // headerAuthenticationHeaderName contains the header name as defined in the field "Header authentication header name"
            // of the "Authentication" section of the virtual proxy.
            location.AsStaticHeaderUserViaProxy(headerUserId: "myUser", headerAuthenticationHeaderName: "X-Qlik-HeaderAuth");

            using (var hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }
    }
}
