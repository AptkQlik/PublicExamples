using System.Linq;
using Qlik.Engine;
using Qlik.Sense.Client;
using Qlik.Sense.Client.Visualizations;

namespace AppTraverser
{
    public static class Traversing
    {
        // Entry point for traversing an app.
        public static void Traverse(IApp app)
        {
            if (app == null)
                return;
            try
            {
                // Get the title of the app from the apps layout.
                var layout = app.GetAppLayout();
                TextHelper.WriteLine(layout.Title + " [" + app.Type + "]");

                // Traverse all sheets of the app.
                foreach (var sheet in app.GetSheets())
                {
                    Traverse(sheet, app);
                }
            }
            catch (MethodInvocationException e)
            {
                TextHelper.WriteLine("Method failed: " + e.Message);
            }
        }

        private static void Traverse(ISheet sheet, IApp app)
        {
            // Print information from the layout of the sheet. Layout properties are accessible
            // directly from the object.
            TextHelper.WriteLine(2, "Rank: " + sheet.Layout.Rank);

            if (sheet.Layout.Cells == null) return;

            TextHelper.WriteLine(2, "# Elements: " + sheet.Layout.Cells.Count());

            // Traverse all cells of the sheet.
            foreach (var cell in sheet.Layout.Cells)
            {
                Traverse(cell, app);
            }
        }

        private static void Traverse(ISheetCell cell, IApp app)
        {
            if (cell == null)
                return;

            TextHelper.WriteLine(3, cell.Name + " [" + cell.Type + "]" +
                                 " Size: " + cell.Rowspan + " / " + cell.Colspan +
                                 " Position: " + cell.Row + " / " + cell.Col);

            // cell.Name is the id of the object residing in that cell.
            var genericObject = app.GetGenericObject(cell.Name);

            // Traverse the object that the cell contains.
            Traverse(genericObject, app);
        }

        private static void Traverse(IGenericObject genericObject, IApp app)
        {
            if (genericObject == null)
                return;

            // Check if the object is an extensions of a master object.
            if (genericObject.Properties.ExtendsObject != null)
            {
                var masterObject = (IMasterObject) genericObject.Properties.ExtendsObject;
                TextHelper.WriteLine(4, "MasterObject version " + masterObject.Properties.MasterVersion);
            }

            switch (genericObject)
            {
                case Table o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Barchart o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Linechart o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Piechart o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Scatterplot o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Treemap o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Gauge o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Combochart o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case TextImage o: TextHelper.Print(o.GetType().Name, o.Properties.HyperCubeDef, o.Layout.Title, app); break;
                case Listbox o: TextHelper.Print(o.GetType().Name, o.Layout.Title); break;
                case Filterpane o: TextHelper.Print(o.GetType().Name, o.Layout.Title); break;
                case Kpi o: TextHelper.Print(o.GetType().Name, o.Layout.Title); break;
                case Pivottable o: TextHelper.Print(o.GetType().Name, o.Layout.Title); break;
                case Map o: TextHelper.Print(o.GetType().Name, o.Layout.Title); break;
                default: TextHelper.WriteLine(4, "Unknown type: " + genericObject.GetType().FullName); break;
            }
        }
    }
}
