using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Qlik.Engine;
using Qlik.OAuthManager;
using Qlik.Sense.RestClient;

namespace ConnectQcsOAuth
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tenantUrl = "<url>";
            var clientId = "<clientId>";
            var redirectUri = "<redirectUrl>"; // The redirect url configured for the OAuth client. Example: "http://localhost:8123"
            var appId = "<appId>";

            // OAuth authorization flow relying on browser for authentication.
            // The NuGet library "Qlik.OAuthManager" is used for OAuth interaction.
            var oauthManager = new OAuthManager(tenantUrl, clientId);
            oauthManager.AuthorizeInBrowser("user_default offline_access", redirectUri, Browser.Default).Wait();
            var accessToken = oauthManager.RequestNewAccessToken().Result;

            // Use access token to retrieve app information through REST.
            // The NuGet library "QlikSenseRestClient" is used for REST interaction.
            var client = new RestClient(tenantUrl);
            client.AsApiKeyViaQcs(accessToken);

            var appResource = client.Get<JObject>($"/api/v1/items?resourceType=app&resourceId={appId}")["data"].OfType<JObject>().Single();
            Console.WriteLine("App title from REST: " + appResource["name"]);

            // Use access token to retrieve app information through engine.
            // The NuGet library "QlikSense.NetSDK" is used for websocket interaction.
            var location = QcsLocation.FromUri(tenantUrl);
            location.AsApiKey(accessToken);

            using (var app = location.App(appId))
            {
                Console.WriteLine("App title from engine: " + app.GetAppProperties().Title);
            }
        }
    }
}
