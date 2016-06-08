using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://127.0.0.1:4848");
            ILocation location = SetupConnection(uri);
            PrintQlikSenseVersionNumber(location);
        }

        private static ILocation SetupConnection(Uri uri)
        {
            // Qlik Sense Desktop.
            ILocation location = Qlik.Engine.Location.FromUri(uri);

            // Defines the location as a direct connection to Qlik Sense Personal Edition.
            location.AsDirectConnectionToPersonalEdition();
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
