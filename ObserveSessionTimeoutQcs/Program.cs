using System;
using System.Threading;
using Qlik.Sense.JsonRpc;
using Qlik.Sense.JsonRpc.WebSocket;
using QcsLocation = Qlik.Engine.QcsLocation;


namespace ObserveSessionTimeoutQcs
{
    class Program
    {
        static void Main(string[] args)
        {
            var url = "<url>";
            var key = "<api key>";
            var location = QcsLocation.FromUri(url);
            location.AsApiKey(key);

            using (var app = location.SessionApp())
            {
                var sessionTimeoutEvent = new ManualResetEventSlim(false);
                // In QCS versions of Qlik Sense, session timeouts are reported a websocket close status code.
                app.Session.CommunicationErrorEvent += (o, a) => OnCommunicationErrorEvent(o, a, sessionTimeoutEvent);
                Console.WriteLine(DateTime.Now + " - Waiting for session timeout...");
                sessionTimeoutEvent.Wait();
                Console.WriteLine(DateTime.Now + " - Session timeout received.");
            }
        }

        private static void OnCommunicationErrorEvent(object sender, CommunicationErrorEventArgs args, ManualResetEventSlim manualResetEventSlim)
        {
            Console.WriteLine(DateTime.Now + " - Communication error detected: " + args.Exception.Message);
            if (args.QlikWebSocketClosedStatus == QlikWebSocketClosedStatus.IdleTimeout)
            {
                manualResetEventSlim.Set();
            }
        }
    }
}
