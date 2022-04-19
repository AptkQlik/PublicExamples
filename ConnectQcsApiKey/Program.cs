using System;
using Qlik.Engine;

namespace ConnectQcsApiKey
{
    class Program
    {
        static void Main(string[] args)
        {
            const string key = "<API key>";
            const string appId = "<appId>";
            const string url = "<uri>";

            var location = QcsLocation.FromUri(url);
            location.AsApiKey(key);

            using (var app = location.App(appId))
            {
                Console.WriteLine(app.GetAppProperties().Title);
            }
        }
    }
}
