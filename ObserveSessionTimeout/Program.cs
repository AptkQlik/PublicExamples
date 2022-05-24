using System;
using System.Threading;
using Qlik.Engine;
using Qlik.Sense.JsonRpc;
using Location = Qlik.Engine.Location;

namespace ObserveSessionTimeout
{
    class Program
    {

        static void Main(string[] args)
        {
            var url = "<url>";
            var location = Location.FromUri(url);
            location.AsNtlmUserViaProxy(certificateValidation: false);

            using (var hub = location.Hub())
            {
                var sessionTimeoutEvent = new ManualResetEventSlim(false);
                // In client managed versions of Qlik Sense, session timeouts are reported are reported through a push method.
                hub.Session.PushMethodReceivedEvent += (o, a) => OnPushMethodReceivedEvent(o, a, sessionTimeoutEvent);
                Console.WriteLine(DateTime.Now + " - Waiting for session timeout...");
                sessionTimeoutEvent.Wait();
                Console.WriteLine(DateTime.Now + " - Session timeout received.");
            }
        }

        private static void OnPushMethodReceivedEvent(object sender, PushMethodReceivedEventArgs args, ManualResetEventSlim manualResetEventSlim)
        {
            Console.WriteLine(DateTime.Now + " - Push method received: " + args.Method);
            if (args.Method == "OnSessionTimedOut")
            {
                manualResetEventSlim.Set();
            }
        }
    }
}
