using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Globalization;

namespace RevitProject
{
    public class ExternalApp : IExternalApplication
    {

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("NGMM");
            string path = Assembly.GetExecutingAssembly().Location;

            PushButtonData columns = new PushButtonData("column", "Columns from Cad", path, "RevitProject.ColumnsCommand");
            PushButtonData grids = new PushButtonData("grid", "Grids", path, "RevitProject.GridsCommand");
            PushButtonData levels = new PushButtonData("level", "Levels Importer", path, "RevitProject.LevelsCommand");

            RibbonPanel panel1 = application.CreateRibbonPanel("NGMM", "Import Levels");
            RibbonPanel panel2 = application.CreateRibbonPanel("NGMM", "Grids and Columns from cad link");

            panel1.AddItem(levels);
            panel2.AddItem(columns);
            panel2.AddItem(grids);
           
           
            return Result.Succeeded;
        }
    }
}