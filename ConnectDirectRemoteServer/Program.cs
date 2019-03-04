using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDirectRemoteServer
{
    class Program
    {
        //Reads a file.
        internal static byte[] ReadFile(string fileName)
        {
            FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            int size = (int)f.Length;
            byte[] data = new byte[size];
            size = f.Read(data, 0, size);
            f.Close();
            return data;
        }

        static void Main(string[] args)
        {
            PrintQlikSenseVersionNumber();
        }

        private static void PrintQlikSenseVersionNumber()
        {
            try
            {
                var uri = new Uri("https://myQlikSenseServer.myDomain.com");
                ILocation location = Qlik.Engine.Location.FromUri(uri);
                X509Certificate2 x509 = new X509Certificate2();
                //Create X509Certificate2 object from .cert file.
                byte[] rawData = ReadFile("PathToMyCertFile.pfx");
                x509.Import(rawData, "PasswordToMyCert", X509KeyStorageFlags.UserKeySet);
                X509Certificate2Collection certificateCollection = new X509Certificate2Collection(x509);
                // Defining the location as a direct connection to Qlik Sense Server
                location.AsDirectConnection("domain", "user", certificateCollection: certificateCollection);

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

