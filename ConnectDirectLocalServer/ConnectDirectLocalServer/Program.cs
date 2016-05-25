using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDirectLocalServer
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
            // Qlik Sense Server on the local machine.
            // Note 1. The adress to the host must be the same as the one given from the installation, this due to that the certificate has that computername.
            // Note 2. The account executing this program must the same as the one running the Qlik Sense services.
            ILocation location = Qlik.Engine.Location.FromUri(uri);

            // Defining the location as direct connection, userDirectory contains the name of the domain / user directory (AD), userId contains the user and extendedSecurityEnvironment defines if the an extended security environment (default is false)
            location.AsDirectConnection(userDirectory: "myDomain", userId: "myUser", extendedSecurityEnvironment:false);

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
