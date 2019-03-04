using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectServerNTLM
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://myQlikSenseServer.myDomain.com");

            if (args.Length > 0)
            {
                uri = new Uri(args[0]);
            }
            ILocation location = SetupConnection(uri);
            PrintQlikSenseVersionNumber(location);
        }
        private static ILocation SetupConnection(Uri uri)
        {
            ILocation location = Qlik.Engine.Location.FromUri(uri);

            // Defines the location as NTLM via proxy. The default value for proxyUsesSsl is true. Must be set to false if the connection uses http.
            location.AsNtlmUserViaProxy(proxyUsesSsl:true);
            return location;
        }

        private static void PrintQlikSenseVersionNumber(ILocation location)
        {
            try
            {
                using (IHub hub = location.Hub(noVersionCheck: true))
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
