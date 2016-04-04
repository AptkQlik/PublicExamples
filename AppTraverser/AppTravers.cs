using System;
using Qlik.Engine;

namespace AppTraverser
{
    internal static class AppTravers
    {
        private static void Main(string[] args)
        {
            var location = GetQlikDesktopLocation();

            ExitIfServerUnavailable(location);

            ListApps(location);

            Console.ReadLine();
        }

        private static ILocation GetQlikDesktopLocation()
        {
            //Get location for Qlik Desktop on localhost
            var location = Location.FromUri(new Uri("ws://127.0.0.1:4848"));
            location.AsDirectConnectionToPersonalEdition();
            return location;
        }

        private static void ListApps(ILocation location)
        {
            foreach (var appIdentifier in location.GetAppIdentifiers())
            {
                try
                {
                    using (var app = location.App(appIdentifier))
                    {
                        Traversing.Traverse(app);
                    }
                }
                catch (MethodInvocationException e)
                {
                    TextHelper.WriteLine("Could not open app: " + appIdentifier.AppName + Environment.NewLine +
                                         TextHelper.Indent() +
                                         e.InvocationError.Message);
                }
                catch (TimeoutException e)
                {
                    TextHelper.WriteLine("Timeout for: " + appIdentifier.AppName + Environment.NewLine +
                                         TextHelper.Indent() + e.Message);
                }
            }
        }

        private static void ExitIfServerUnavailable(ILocation location)
        {
            if (location.IsAlive()) return;

            TextHelper.WriteLine("Engine at " + location.ServerUri + " is not alive");
            WaitAndExit();
        }


        private static void WaitAndExit()
        {
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}