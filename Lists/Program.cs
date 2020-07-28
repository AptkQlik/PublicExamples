using System;
using System.Linq;
using Qlik.Engine;
using Qlik.Sense.Client;
using IMeasureList = Qlik.Sense.Client.IMeasureList;

namespace Lists
{
    class Program
    {
        static void Main(string[] args)
        {
            var location = Location.FromUri("http://localhost:4848");
            location.AsDirectConnectionToPersonalEdition();

            var appId = location.GetAppIdentifiers().First();
            using (var app = location.App(appId))
            {
                Console.WriteLine($"Opened app named \"{appId.Title}\" (id={appId.AppId})");

                // Get a session object for listing library measures:
                var measureList = app.GetMeasureList();

                IllustrateMeasureListUsage(measureList);

                // Lists should always be destroyed when no longer needed to avoid object leakage.
                app.DestroyGenericSessionObject(measureList.Id);
            }
        }

        private static void IllustrateMeasureListUsage(IMeasureList measureList)
        {
            // All list information is accessed through the layout of the session object. The
            // property "MeasureList.Items" is where information about the individual measures can be found.
            var theList = measureList.Layout.MeasureList;
            var cnt = theList.Items.Count();
            Console.WriteLine($"Number of measures in the app is {cnt}");
            if (cnt == 0)
            {
                Console.WriteLine("Add library measures to the app or connect to a different app to run the rest of the example.");
                return;
            }

            // List the id of all measures
            Console.WriteLine("Measure IDs:");
            foreach (var measure in theList.Items)
            {
                // The ID's of the measures can be found in the "Info" section of each item.
                Console.WriteLine($"  {measure.Info.Id}");
            }

            // List the title of all measures
            Console.WriteLine("Measure IDs and titles:");
            foreach (var measure in theList.Items)
            {
                // Apart from ID and type information, all other information is found in the "Data" section for each item.
                Console.WriteLine($"  {measure.Info.Id}: {measure.Data.Title}");
            }

            // Configure data retrieval
            using (measureList.SuspendedLayout)
            {
                // The set of information that the list retrieves in the "Data" section is configurable. Configuration
                // is done by modifying the "Data" properties of the list.
                var data = measureList.Properties.MeasureListDef.Data;

                // The data object contains one property for each entry that should be retrieved by the list when GetLayout
                // is called. The value of the property should be the path to where in the properties structure of the
                // measure the value of interest is found.
                var pathToDefinition = "/qMeasure/qDef";
                data.Set("myProp", pathToDefinition);
            }

            // The properties have now changed, so reload the layout.
            theList = measureList.Layout.MeasureList;

            // The items will now include information about the measure definitions.
            Console.WriteLine("Measure IDs and definitions:");
            foreach (var measure in theList.Items)
            {
                // The custom property previously created can be retrieved using the generic "Get" method.
                Console.WriteLine($"  {measure.Info.Id}: {measure.Data.Get<string>("myProp")}");
            }
        }
    }
}
