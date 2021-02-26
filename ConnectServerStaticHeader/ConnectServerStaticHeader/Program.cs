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

            // Set the prefix for the virtual proxy.
            location.VirtualProxyPath = "static";

            // Defines the location as a static header connection via proxy. The headerUserId contains the user and
            // headerAuthenticationHeaderName contains the header name as defined in the field "Header authentication header name"
            // of the "Authentication" section of the virtual proxy.
            location.AsStaticHeaderUserViaProxy(headerUserId: "myUser", headerAuthenticationHeaderName:"X-Qlik-HeaderAuth");

            return location;
        }

        private static void PrintQlikSenseVersionNumber(ILocation location)
        {
            try
            {
                using (IHub hub = location.Hub())
                {
                    Console.WriteLine(hub.EngineVersion().ComponentVersion);
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
