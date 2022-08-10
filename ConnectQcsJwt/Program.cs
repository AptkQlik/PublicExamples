using System;
using Qlik.Engine;

namespace ConnectQcsJwt
{
    class Program
    {
        static void Main(string[] args)
        {
            const string url = "<uri>";
            const string appId = "<appId>";
            const string jwt = "<JSON Web Token>";

            var location = QcsLocation.FromUri(url);
            location.AsJwt(jwt);

            using (var app = location.App(appId))
            {
                Console.WriteLine(app.GetAppProperties().Title);
            }
        }
    }
}
