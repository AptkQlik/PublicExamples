using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Qlik.Engine;
using Qlik.Engine.Communication;
using Qlik.Sense.Client;
using Qlik.Sense.Client.Snapshot;
using Qlik.Sense.Client.Storytelling;
using Qlik.Sense.Client.Visualizations;
using Qlik.Sense.Client.Visualizations.Components;

namespace DataStorytelling
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to desktop
            var location = Qlik.Engine.Location.FromUri(new Uri("ws://127.0.0.1:4848"));
            location.AsDirectConnectionToPersonalEdition();
            try
            {
                // Open the app with name "Beginner's tutorial"
                var appIdentifier = location.AppWithNameOrDefault(@"Beginner's tutorial", noVersionCheck: true);
                using (var app = location.App(appIdentifier, noVersionCheck: true))
                {
                    //9.1 Taking snapshots
                    // Get the Dashbord sheet
                    // Clear all selection to set the app in en known state
                    app.ClearAll();

                    // Get the sheet with the title "Dashboard"
                    var sheet = GetSheetWithTitle(app, "Dashboard");

                    ISnapshot nordicTop5Customers;
                    ISnapshot nordicQuarterlyTrend;
                    ISnapshot usaTop5Customers;
                    ISnapshot usaQuarterlyTrend;
                    ISnapshot japanTop5Customers;
                    ISnapshot japanQuarterlyTrend;
                    TakeSnapshots(sheet, app, out nordicTop5Customers, out nordicQuarterlyTrend, out usaTop5Customers, out usaQuarterlyTrend, out japanTop5Customers, out japanQuarterlyTrend);

                    //9.2 Createing a simple story
                    // 1 Create a new story
                    var storyProps = new StoryProperties();
                    // 2 Enter the title
                    storyProps.MetaDef.Title = "SDK Creted - Three largest regions";
                    var story = app.CreateStory("ThreelargestregionsStory", storyProps);
                    // Create a slide
                    var slideProps = new SlideProperties();
                    var slide1 = story.CreateSlide("ThreelargestregionsSlide", slideProps);

                    // 4 Add a title
                    var titleProp = slide1.CreateTextSlideItemProperties("ThreelargestregionsSlideTitle", Slide.TextType.Title, text: "Three largest regions");
                    titleProp.Position = new SlidePosition
                    {
                        Height = "20%",
                        Left = "5%",
                        Top = "0.1%",
                        Width = "40%",
                        ZIndex = 1
                    };
                    slide1.CreateSlideItem(null, titleProp);

                    // 7 Add the Sales per Region snapshot to the slide
                    // 8 Resize (SDK set the size)
                    AddSnapshotToSlide(slide1, "ThreelargestregionsSlideUsa", "SDK_SalesPerRegion", "Usa", "1%");
                    AddSnapshotToSlide(slide1, "ThreelargestregionsSlideNordic", "SDK_SalesPerRegion", "Nordic", "34%");
                    AddSnapshotToSlide(slide1, "ThreelargestregionsSlideJapan", "SDK_SalesPerRegion", "Japan", "67%");


                    // Create slides 2-4
                    var slide2 = story.CreateSlide("NordicSlide", slideProps);
                    CreateRegionItemsOnSlide(slide2, "Nordic", nordicTop5Customers, nordicQuarterlyTrend);
                    var slide3 = story.CreateSlide("UsaSlide", slideProps);
                    CreateRegionItemsOnSlide(slide3, "Usa", usaTop5Customers, usaQuarterlyTrend);
                    var slide4 = story.CreateSlide("JapanSlide", slideProps);
                    CreateRegionItemsOnSlide(slide4, "Japan", japanTop5Customers, japanQuarterlyTrend);
                    
                    // Just in clase lets clear all selections.
                    app.ClearAll();
                    // Save our new story and snaphots
                    app.DoSave();
                    Console.WriteLine(@"A new story by the name 'SDK Created - Tree largest regions' has been created. Open 'Beginner's tutorial' and verify your new story.");
                }
            }
            catch (MethodInvocationException e)
            {
                Console.WriteLine("Could not open app! "  + e.InvocationError.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout : " + e.Message);
            }
            Console.ReadLine();
        }

        private static void TakeSnapshots(ISheet sheet, IApp app, out ISnapshot nordicTop5, out ISnapshot nordicQuarterly, out ISnapshot usaTop5, out ISnapshot usaQuarterly, out ISnapshot japanTop5, out ISnapshot japanQuarterly)
        {
            // Get the Sales per Region on the sheet
            var salesPerRegion = GetVisualisationWithTitle(sheet, "Sales per Region");
            // Take a snapshot of SalesPerRegion
            if (salesPerRegion != null)
                app.CreateSnapshot("SDK_SalesPerRegion", sheet.Id, salesPerRegion.Id);

            // Select a region an take a snapshot of Top5Customers and QuarterlyTrend
            GetSelectAndCreateSnapshots(app, sheet, "Nordic", out nordicTop5, out nordicQuarterly);
            app.ClearAll();

            GetSelectAndCreateSnapshots(app, sheet, "Usa", out usaTop5, out usaQuarterly);
            app.ClearAll();

            GetSelectAndCreateSnapshots(app, sheet, "Japan", out japanTop5, out japanQuarterly);
            app.ClearAll();
        }

        private static void CreateRegionItemsOnSlide(ISlide slide, string title, ISnapshot top5Customers, ISnapshot quarterlyTrend)
        {
            var titleProp = slide.CreateTextSlideItemProperties(null, Slide.TextType.Title, text: title);
            titleProp.Position = new SlidePosition {Height = "20%", Left = "5%", Top = "0.1%", Width = "40%", ZIndex = 1};
            slide.CreateSlideItem(null, titleProp);
            AddSnapshotToSlide(slide, "Top5Slide", top5Customers.Id, null, "1%");
            AddSnapshotToSlide(slide, "QuarterlySlite", quarterlyTrend.Id, null, "51%");
        }

        private static void AddSnapshotToSlide(ISlide slide, string name, string snapshotId, string region, string left)
        {
            int selectedIndex = 0;
            var prop = slide.CreateSnapshotSlideItemProperties(name, snapshotId);
            prop.Position = new SlidePosition
            {
                Height = "33%",
                Left = left,
                Top = "25%",
                Width = "33%",
                ZIndex = 1
            };
            var slideItem = slide.CreateSlideItem(name, prop);
            slideItem.EmbedSnapshotObject(snapshotId);
            var mySnapShotedItem = slideItem.GetSnapshotObject();

            var data = mySnapShotedItem.GetProperties();
            if (region != null)
            {
                var hypercube = data.Get<HyperCube>("qHyperCube");
                foreach (var nxDataPage in hypercube.DataPages)
                {
                    int index = -1;
                    foreach (var cellRows in nxDataPage.Matrix)
                    {
                        index++;
                        foreach (var row in cellRows)
                        {
                            if (row.Text == region)
                                selectedIndex = index;
                        }
                    }
                }
            }
            using (mySnapShotedItem.SuspendedLayout)
            {
                JObject originalModelSettings = new JObject();
                JObject datapoint = new JObject();
                JObject legend = new JObject();
                datapoint.Add("auto", false);
                datapoint.Add("labelmode", "share");
                legend.Add("show", false);
                legend.Add("dock", "auto");
                legend.Add("showTitle", true);
                originalModelSettings.Add("dataPoint", datapoint);
                originalModelSettings.Add("legend", legend);
                originalModelSettings.Add("dimensionTitle", true);
                data.Set("originalModelSettings", originalModelSettings);
                if (region != null)
                {
                    data.Set("effectPath", "/effects/highlight-value");
                    JObject effectProperties = new JObject();
                    effectProperties.Add("selectedIndex", selectedIndex);

                    data.Set("effectProperties", effectProperties);
                }
            }
        }

        private static int GetSelectAndCreateSnapshots(IApp app, ISheet sheet, string region, out ISnapshot top5CustomersSnapshot, out ISnapshot quarterlyTrendSnapshot)
        {
            // Select region
            int selectedIndex = 0;
            IField field = null;
            foreach (var item in app.GetFieldList().Items)
            {
                if (item.Name == "Region")
                    field = app.GetField(item.Name);
            }
            var res = field != null && field.Select(region);
            var extendedSel = app.GetExtendedCurrentSelection();
            foreach (var nxDataPage in extendedSel.GetField("Region").DataPages)
            {
                foreach (var rows in nxDataPage.Matrix)
                {
                    var cell = rows.FirstOrDefault();
                    if (cell != null)
                    {
                        if (cell.State == StateEnumType.SELECTED)
                            selectedIndex = cell.ElemNumber;
                    }
                }
            }

            // Get the Top 5 Customers on the sheet
            var top5Customers = GetVisualisationWithTitle(sheet, "Top 5 Customers");

            // Take a snapshot of Top 5 Customers
            top5CustomersSnapshot = app.CreateSnapshot(region + "Top5Customers", sheet.Id, top5Customers.Id);

            // Get the Quarterly Trend on the sheet
            var quarterlyTrend = GetVisualisationWithTitle(sheet, "Quarterly Trend");

            // Take a snapshot of Quarterly Trend
            quarterlyTrendSnapshot = app.CreateSnapshot(region + "QuarterlyTrend", sheet.Id, quarterlyTrend.Id);
            return selectedIndex;
        }

        private static ISheet GetSheetWithTitle(IApp app, string title)
        {
            return app.GetSheets().FirstOrDefault(sheet => sheet.MetaAttributes.Title.ToLower() == title.ToLower());
        }

        private static IGenericObject GetVisualisationWithTitle(ISheet sheet, string title)
        {
            foreach (var cell in sheet.Cells)
            {
                var child = sheet.GetChild(cell.Name);
                if (child.GetLayout().As<VisualizationBaseLayout>().Title == title)
                    return child;
            }
            return null;
        }
    }
}
