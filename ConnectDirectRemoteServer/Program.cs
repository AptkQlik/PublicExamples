using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDirectRemoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // The default port number is 4747 but can be customized
            var uri = new Uri("https://myQlikSenseServer.myDomain.com:4747");
            var certs = CertificateManager.LoadCertificateFromDirectory(@"\path\to\folder\containing\exported\certificates");

            ILocation location = Qlik.Engine.Location.FromUri(uri);
            location.AsDirectConnection("domain", "user", certificateCollection: certs);

            using (var hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }
    }
}

