using System;
using System.IO;
using Qlik.Engine;
using Qlik.Sense.RestClient;

namespace ExportDataToExcel
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var url = "<url>";
            var appId = "<appId>";
            var objectId = "<objectId>";

            var cubePath = "/qHyperCubeDef";
            var destinationFile = @"ExportedData.xlsx";

            var location = Location.FromUri(url);
            location.AsNtlmUserViaProxy(certificateValidation: false);

            ExportDataResult exportResult;
            using (var app = location.App(appId))
            {
                var obj = app.GetGenericObject(objectId);
                Console.WriteLine($"Exporting data.\n  app:    {appId}\n  object: {objectId}\n  path:   \"{cubePath}\"");
                exportResult = obj.ExportData(NxExportFileType.EXPORT_OOXML, cubePath, serveOnce: true);
            }

            var client = new RestClient(url);
            client.AsNtlmUserViaProxy(certificateValidation: false);
            Console.WriteLine($"Exporting excel file to: {destinationFile}");
            var binData = client.GetBytes(exportResult.Url);
            File.WriteAllBytes(destinationFile, binData);
            Console.WriteLine($"Wrote {binData.Length} bytes to {destinationFile}.");
        }
    }
}
