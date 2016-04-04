using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            PrintQlikSenseVersionNumber();
        }

        private static void PrintQlikSenseVersionNumber()
        {
            try
            {
                // Single Qlik Sense Desktop with no special settings
                ILocation location = Qlik.Engine.Location.FromUri(new Uri("ws://127.0.0.1:4848"));

                // Defining the location as a direct connection to Qlik Sense Personal
                location.AsDirectConnectionToPersonalEdition();

                using (IHub hub = location.Hub(noVersionCheck: true))
                {
                    Console.WriteLine(hub.ProductVersion());
                }
            }
            catch (CommunicationErrorException cex)
            {
                Console.WriteLine("Can not connect to Qlik Sense instance, check that Qlik Sense is running");
            }
            catch (Exception ex)
            {
                Console.WriteLine("General error : " + ex.Message);
            }
            Console.ReadLine();
        }
    }
}
