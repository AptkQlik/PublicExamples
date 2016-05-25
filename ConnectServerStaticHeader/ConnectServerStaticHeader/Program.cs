using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectServerStaticHeader
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://myQlikSenseServer.myDomain.com");
            ILocation location = SetupConnection(uri);
            PrintQlikSenseVersionNumber(location);
        }

        private static ILocation SetupConnection(Uri uri)
        {
            ILocation location = Qlik.Engine.Location.FromUri(uri);

            // Set the prefix for the virtual proxy
            location.VirtualProxyPath = "static";

            // Defining the location as static header connection via proxy, headerUserId contains the user and headerAuthenticationHeaderName contains the session cookie header name.
            location.AsStaticHeaderUserViaProxy(headerUserId: "myUser", headerAuthenticationHeaderName:"X-Qlik-HeaderAuth");

            return location;
        }

        private static void PrintQlikSenseVersionNumber(ILocation location)
        {
            try
            {
                using (IHub hub = location.Hub(noVersionCheck: true))
                {
                    Console.WriteLine(hub.ProductVersion());
                }
            }
            catch (CommunicationErrorException cex)
            {
                Console.WriteLine("Can not connect to Qlik Sense instance, check that Qlik Sense is running." + Environment.NewLine + cex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error." + Environment.NewLine + ex.Message);
            }
            Console.ReadLine();
        }
    }
}
