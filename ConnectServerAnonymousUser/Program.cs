using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectServerAnonymousUser
{
    class Program
    {
        private static Uri _qlikSenseServerUri;

        static void Main(string[] args)
        {
            // Uri to Qlik Sense Server can be given by command-line or use a default value.
            if (args.Length > 0)
                _qlikSenseServerUri = new Uri(args[0]);
            else
                _qlikSenseServerUri = new Uri("https://playground.qlik.com");

            ILocation location = SetupConnection();

            PrintQlikSenseVersionNumber(location);
        }

        private static ILocation SetupConnection()
        {
            // Qlik Sense Server with no special settings
            ILocation location = Qlik.Engine.Location.FromUri(_qlikSenseServerUri);

            // Defining the location as using anonymous mode i.e. not using a specific user when establishing connection
            location.AsAnonymousUserViaProxy();
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
