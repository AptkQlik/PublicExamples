using System;
using System.Collections.Generic;
using System.Linq;
using Qlik.Engine;
using Qlik.Engine.Communication;
using Qlik.Sense.Client;
using Qlik.Sense.Client.Visualizations;
using Qlik.Sense.Client.Visualizations.MapComponents;

namespace AbstractStructure
{
    /// <summary>
    /// This example illustrates the concept of abstract structures as used by the Qlik Sense SDK.
    /// More information about the topic of abstract structures can be found on the developer help site at 
    /// http://help.qlik.com, Building applications with the .NET SDK -> Working with Qlik Sense .NET SDK
    /// -> Abstract structure.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            BasicAbstractStructureUsage();

            var location = ConnectToDesktop();
            var allApps = OpenAllApps(location).ToArray();
	        if (allApps.Any())
	        {
				UseAbstractStructureOnMasterObjects(allApps);
				UseAbstractStructureOnMapLayers(allApps);
			}
			Console.WriteLine("Press enter to close.");
			Console.ReadLine();
        }

        private static void BasicAbstractStructureUsage()
        {
            Console.WriteLine("******************************************");
            Console.WriteLine("** Part 1) Basic abstract structure usage");
            Console.WriteLine("******************************************");
            // Create new instances of the classes A and B.
			var a = new A { N = 1 };
			var b = new B { S = "Hello" };
			// Print the names of all properties found in the two objects.
			Console.WriteLine("Properties in object a: {0}", String.Join(", ", a.GetAllProperties()));
			Console.WriteLine("Properties in object b: {0}", String.Join(", ", b.GetAllProperties()));

            // Interpret "a" as being of class B.
            var ab = a.As<B>();

            // Print the names of all properties found in object "ab". Notice that the property "n" is visible in
            // "ab" even though it is not of type A. "n" is available as a dynamic property in "ab". Notice also
            // that "ab" does not contain a property called "s". Accessing S at this point will simply return null.
            Console.WriteLine("Properties in object ab: {0}", String.Join(", ", ab.GetAllProperties()));
            
            // Add a dynamic property to "a". The property immediately becomes visible in "ab".
            a.Set("s", "Hello from A");
            Console.WriteLine("Properties in object ab after modifying dynamic property \"s\" in a: {0}", String.Join(", ", ab.GetAllProperties()));

            // The dynamic property "s" written to "a" is the same as the one wrapped by the property S of
            // class B. It can therefore be accessed through the property S when interpreting "a" as being of
            // class B.
            Console.WriteLine("This is the text written to object interpreted as B: {0}", ab.S);

            // Changes to S in B will also immediately become visible for the dynamic property "s" for object "a".
            ab.S = "Hello from B";
            Console.WriteLine("This is the text written to property S for class B: {0}", ab.S);
            Console.WriteLine("This is the same text accessed through dynamic property \"s\" for class A: {0}", a.Get<string>("s"));
        }

        // A class containing a number.
        class A : Qlik.Engine.AbstractStructure
        {
            // Wrapper for a dynamic property named "n".
            public int N
            {
                get { return Get<int>("n"); }
                set { Set("n", value); }
            }
        }

        // A class containing a string.
        class B : Qlik.Engine.AbstractStructure
        {
            // Wrapper for a dynamic property named "s".
            public string S
            {
                get { return Get<string>("s"); }
                set { Set("s", value); }
            }
        }

        // Master objects are used when it is desirable to add the same object multiple times
        // to the same app. A typical scenario would be that a certain visualization should be
        // available on multiple sheets.
        //
        // Master objects have a dedicated type, but the properties for those object will depend
        // on the type of the object that they are containers for. A master object created for a
        // barchart will for instance have properties of a barchart, while a master object created
        // for a line chart will have line chart properties. The type of the C# representation of
        // master objects does therefore not represent directly the type of the properties associate
        // with those objects.
        //
        // This example illustrates how the concept of abstract structure can be applied to interact
        // with master object properties.
        private static void UseAbstractStructureOnMasterObjects(IEnumerable<IApp> apps)
        {
            Console.WriteLine("****************************************************************");
            Console.WriteLine("** Part 2) Use of abstract structure to handle master objects.");
            Console.WriteLine("****************************************************************");
            // Get all master objects for all apps and process them using abstract structure.
            var allMasterObjects = GetAllMasterObjects(apps);
            allMasterObjects.ToList().ForEach(ProcessMasterObject);
        }

        private static void ProcessMasterObject(IMasterObject masterObject)
        {
            var properties = masterObject.Properties;

            // Check if the master object represents a visualization (i.e. has a property called "visualization").
            if (properties.Get<string>("visualization") != null)
            {
                Console.WriteLine("Master object seems to represent a Qlik Sense client visualization.");

                // Interpret properties as being of type VisualizationBaseProperties.
                var visualizationBaseProperties = properties.As<VisualizationBaseProperties>();

                // Check if the visualization represents a bar chart and print information accordingly.
                if (visualizationBaseProperties.Visualization == "barchart")
                {
                    Console.WriteLine("Master object with id {0} is a barchart.", properties.Info.Id);
                    var barchartProperties = visualizationBaseProperties.As<BarchartProperties>();
                    var barGrouping = barchartProperties.BarGrouping.Grouping;
                    Console.WriteLine("  BarGrouping is set to: " + barGrouping);
                }
                else
                    Console.WriteLine("Master object with id {0} is not a bar chart. It is a {1}.", properties.Info.Id,
                        visualizationBaseProperties.Visualization);

                // Print the title of the master object.
                Console.WriteLine("  Title is: \"{0}\"", visualizationBaseProperties.Title);
            }
            else
            {
                Console.WriteLine("The master object does not represent a Qlik Sense client visualization.");
            }

            // Treat the object as a generic hypercube container and access the HyperCubeDef if it exists.
            var hyperCubeContainer = properties.As<HyperCubeContainer>();
            if (hyperCubeContainer.ContainsHyperCube)
                ProcessHyperCubeDef(hyperCubeContainer.HyperCubeDef);
            else
                Console.WriteLine("  Object has no hypercube.");
        }
        
        // A class that contains a hypercube definition.
        class HyperCubeContainer : Qlik.Engine.AbstractStructure
        {
            public IHyperCubeDef HyperCubeDef { get { return Get<HyperCubeDef>("qHyperCubeDef"); } }
            public bool ContainsHyperCube { get { return HyperCubeDef != null; } }
        }

        /// <summary>
        /// Print information details of a hypercube definition.
        /// </summary>
        /// <param name="hyperCubeDef">The hypercube definition to process.</param>
        private static void ProcessHyperCubeDef(IHyperCubeDef hyperCubeDef)
        {
            // Process dimensions
            Console.WriteLine("  Number of dimensions: {0}", hyperCubeDef.Dimensions.Count());
            var dimensionCount = 0;
            foreach (var dimension in hyperCubeDef.Dimensions)
            {
                Console.Write("    Dimension #{0}: ", dimensionCount++);
                if (dimension.LibraryId != null)
                    Console.WriteLine("Library dimension - {0}", dimension.LibraryId);
                else
                    Console.WriteLine("Inline dimension - {0}", dimension.Def.FieldDefs.First());
            }

            // Process measures
            Console.WriteLine("  Number of measures: {0}", hyperCubeDef.Measures.Count());
            var measureCount = 0;
            foreach (var measure in hyperCubeDef.Measures)
            {
                Console.Write("    Measure #{0}: ", measureCount++);
                if (measure.LibraryId != null)
                    Console.WriteLine("Library measure - {0}", measure.LibraryId);
                else
                    Console.WriteLine("Inline measure - {0}", measure.Def.Def);
            }
        }

        private static ILocation ConnectToDesktop()
        {
            var location = Location.FromUri(new Uri("ws://127.0.0.1:4848"));
            location.AsDirectConnectionToPersonalEdition();
            return location;
        }

        private static IEnumerable<IApp> OpenAllApps(ILocation location)
        {
	        try
	        {
				return location.GetAppIdentifiers().Select(x => location.App(x));
	        }
	        catch (CommunicationErrorException e)
	        {
				Console.WriteLine("Communication error exception, no Qlik Sense instance found! " + e.Message);
				return new IApp[] { };
			}
        }

        private static IEnumerable<IMasterObject> GetAllMasterObjects(IEnumerable<IApp> apps)
        {
            return apps.SelectMany(GetAllMasterObjects);
        }

        private static IEnumerable<IMasterObject> GetAllMasterObjects(IApp app)
        {
            return app.GetMasterObjectList().Items.Select(item => app.GetObject<MasterObject>(item.Info.Id));
        }

        // The Map visualization of the Qlik.Sense.Client namespace has a concept of layers.
        // A map layer contains at least one hypercube used to display information in that layer,
        // which makes it possible to display multiple sets of information in the same map. A polygon
        // layer can for instance color the polygons according to population density of countries, while
        // a point layer might have sizes related to the population size of cities.
        //
        // This example illustrates how the concept of abstract structure can be applied to interact
        // with different types of map layers.
        private static void UseAbstractStructureOnMapLayers(IEnumerable<IApp> apps)
        {
            Console.WriteLine("************************************************************");
			Console.WriteLine("** Part 3) Use of abstract structure to handle map layers.");
            Console.WriteLine("************************************************************");
            // Get all map objects for all apps and process the layers using abstract structure.
            var allMaps = apps.SelectMany(GetAllMapObjects);
            allMaps.ToList().ForEach(ProcessMap);
        }

        private static IEnumerable<IMap> GetAllMapObjects(IApp app)
        {
            return app.GetSheets().
                SelectMany(sheet => sheet.GetChildInfos()).
                Select(info => app.GetObject<GenericObject>(info.Id)).
                OfType<Map>();
        }

        private static void ProcessMap(IMap map)
        {
            Console.WriteLine("Processing map with id {0}, and title \"{1}\"", map.Info.Id, map.Title);
            Console.WriteLine("  The map has {0} layers.", map.Layers.Count());
            var layerCount = 0;
            foreach (var layer in map.Properties.Layers)
            {
                Console.WriteLine(  "Layer #{0} is of type {1}", layerCount++, layer.Type);
                switch (layer.Type)
                {
                    case LayerType.Point:
                        // Interpret layer as a point layer.
                        ProcessPointLayer(layer.As<PointLayerDef>());
                        break;
                    case LayerType.Polygon:
                        // Interpret layer as a polygon layer.
                        ProcessPolygonLayer(layer.As<Polygon3LayerDef>());
                        break;
                }
            }
        }

        private static void ProcessPointLayer(IPointLayerDef layer)
        {
            Console.WriteLine("  PointLayer hypercube has {0} dimensions.", layer.HyperCubeDef.Dimensions.Count());
        }

        private static void ProcessPolygonLayer(IPolygon3LayerDef layer)
        {
            if (layer.HyperCubeDef != null)
                Console.WriteLine("  PolygonLayer hypercube has {0} dimensions.", layer.HyperCubeDef.Dimensions.Count());
            else
                Console.WriteLine("  PolygonLayer has no hypercube.");
            if (layer.Geodata != null)
                Console.WriteLine("  PolygonLayer geodata hypercube has {0} dimensions.", layer.Geodata.HyperCubeDef.Dimensions.Count());
            else
                Console.WriteLine("  PolygonLayer has no geodata.");
        }
    }
}
