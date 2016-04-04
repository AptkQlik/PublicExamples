// This program loads all apps in the local hub into engine memory and forces the engine to
// make all computations required to display the app in the client. This is sometimes useful
// for large apps where it is desirable to have fast load times when apps are opened in the
// client for the first time.
//
// The preload is performed by retrieving the layout of all visualization objects
// in all apps, thereby triggering the engine to compute all values required to show the
// objects. The engine will cache the results of the computations and keeps them in
// memory so that future, identical, queries will not have to be recomputed from scratch.
//
// The engine eventually purges the apps and all computation results from memory if the
// data is not accessed within a certain amount of time. The exact size of the time limit
// for when this purge is performed depends on how the engine is configured.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qlik.Engine;
using Qlik.Engine.Communication;
using Qlik.Sense.Client;

namespace AppPreload
{
    class AppPreload
    {
        static void Main(string[] args)
        {
            var location = Location.FromUri(new Uri("ws://127.0.0.1:4848"));
            location.AsDirectConnectionToPersonalEdition();
            QlikConnection.Timeout = Int32.MaxValue;
            var t = DateTime.Now;
            location.GetAppIdentifiers().ToList().ForEach(id => LoadApp(location, id));
            var dt = DateTime.Now - t;
            Print("Cache initialization complete. Total time: {0}", dt.ToString());
        }

        static void LoadApp(ILocation location, IAppIdentifier id)
        {
            Print("{0}: Opening app", id.AppName);

            // Load the app to memory.
            var app = location.App(id);
            Print("{0}: App opened, getting sheets", id.AppName);
            var sheets = app.GetSheets().ToArray();
            Print("{0}: Number of sheets - {1}, getting children", id.AppName, sheets.Count());
            var allObjects = sheets.Concat(sheets.SelectMany(sheet => GetAllChildren(app, sheet))).ToArray();
            Print("{0}: Number of objects - {1}, getting layouts", id.AppName, allObjects.Count());

            // Trigger the engine to execute all evaluations required to display all objects included in the app.
            // The evaluation results are stored in memory so that subsequent identical queries don't need
            // to be recomputed.
            var allLayoutTasks = allObjects.Select(o => o.GetLayoutAsync());
            Task.WaitAll(allLayoutTasks.ToArray<Task>());
            Print("{0}: Completed loading layouts", id.AppName);
        }

        // Recursively collects the entire tree of objects.
        private static IEnumerable<IGenericObject> GetAllChildren(IApp app, IGenericObject obj)
        {
            var children = obj.GetChildInfos().Select(o => app.GetObject<GenericObject>(o.Id)).ToArray();
            return children.Concat(children.SelectMany(child => GetAllChildren(app, child)));
        }

        private static void Print(string txt, params object[] os)
        {
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " - " + String.Format(txt, os));
        }

    }
}
