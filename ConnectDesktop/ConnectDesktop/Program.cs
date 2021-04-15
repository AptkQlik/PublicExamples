using System;
using Qlik.Engine;

namespace ConnectDesktop
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("http://127.0.0.1:4848");

            var location = Location.FromUri(uri);
            location.AsDirectConnectionToPersonalEdition();

            using (var hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }
    }
}
