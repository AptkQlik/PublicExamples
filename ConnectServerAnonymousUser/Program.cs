﻿using System;
using Qlik.Engine;

namespace ConnectServerAnonymousUser
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://myQlikSenseServer.myDomain.com");

            var location = Location.FromUri(uri);
            location.AsAnonymousUserViaProxy();

            using (var hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }
    }
}
