using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Qlik.Engine;
using Qlik.Engine.Communication;

namespace ConnectServerTicketAttach
{
    class Program
    {
        private static Uri _qlikSenseServerUri;

        static void Main(string[] args)
        {
            // Uri to Qlik Sense Server can be given by command-line or use a default value.
            if (args.Length > 0)
                _qlikSenseServerUri = new Uri(args[0]);
            else
                _qlikSenseServerUri = new Uri("https://server.domain.com");

            // Extract session cookie
            var sessionCookie = ExtractTicketFromCookies();

            ILocation location = SetupConnection(sessionCookie);

            PrintQlikSenseVersionNumber(location);
        }

        /// <summary>
        /// This method is an example on how a session cookie can be extracted.
        /// It opens up a connection towards Qlik Sense Server on given 
        /// uri and extracts the session id 'X-Qlik-Session' from the response cookies
        /// </summary>
        /// <returns>session cookie</returns>
        private static Cookie ExtractTicketFromCookies()
        {
            CookieContainer cookieContainer = new CookieContainer();
            var connectionHandler = new HttpClientHandler
            {
                UseDefaultCredentials = true,
                CookieContainer = cookieContainer
            };
            var connection = new HttpClient(connectionHandler);

            connection.DefaultRequestHeaders.Add("X-Qlik-xrfkey", "ABCDEFG123456789");
            connection.DefaultRequestHeaders.Add("User-Agent", "Windows");

            connection.GetAsync(_qlikSenseServerUri).Wait();

            IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(_qlikSenseServerUri).Cast<Cookie>();

            return responseCookies.First(cookie => cookie.Name.Equals("X-Qlik-Session"));
        }

        private static ILocation SetupConnection(Cookie sessionCookie)
        {
            // Qlik Sense Server with no special settings
            ILocation location = Qlik.Engine.Location.FromUri(_qlikSenseServerUri);

            // Defining the location as an existing session to Qlik Sense Server
            location.AsExistingSessionViaProxy(sessionCookie.Value, sessionCookie.Name);
            return location;
        }

        private static void PrintQlikSenseVersionNumber(ILocation location)
        {
            try
            {
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
