using System;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectDirectLocalServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // The default port number is 4747 but can be customized
            var uri = new Uri("https://localhost:4747");
            var certs = CertificateManager.LoadCertificateFromStore();

            var location = Location.FromUri(uri);
            location.AsDirectConnection(userDirectory: "myDomain", userId: "myUser", certificateCollection: certs);

            using (var hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }
    }
}
