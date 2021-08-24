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
        static void Main(string[] args)
        {
            var uri = new Uri("https://myQlikSenseServer.myDomain.com");

            // Extract session cookie
            var sessionCookie = ExtractTicketFromCookies(uri);

            var location = Location.FromUri(uri);
            location.AsExistingSessionViaProxy(sessionCookie.Value, sessionCookie.Name);

            using (IHub hub = location.Hub())
            {
                Console.WriteLine(hub.EngineVersion().ComponentVersion);
            }
        }

        /// <summary>
        /// This method is an example on how a session cookie can be extracted.
        /// It opens up a connection towards Qlik Sense Server on given 
        /// uri and extracts the session id 'X-Qlik-Session' from the response cookies
        /// </summary>
        /// <returns>session cookie</returns>
        private static Cookie ExtractTicketFromCookies(Uri uri)
        {
            CookieContainer cookieContainer = new CookieContainer();
            var connectionHandler = new HttpClientHandler
            {
                CookieContainer = cookieContainer,
                Credentials = new CredentialCache
                {
                    {uri, "ntlm", CredentialCache.DefaultCredentials.GetCredential(uri, "ntlm")}
                }
            };
            var connection = new HttpClient(connectionHandler);

            connection.DefaultRequestHeaders.Add("X-Qlik-xrfkey", "ABCDEFG123456789");
            connection.DefaultRequestHeaders.Add("User-Agent", "Windows");

            connection.GetAsync(uri).Wait();

            IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(uri);

            return responseCookies.First(cookie => cookie.Name.Equals("X-Qlik-Session"));
        }
    }
}
