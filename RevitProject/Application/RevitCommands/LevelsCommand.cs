using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Ganss.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevitProject
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class LevelsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIDocument Uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = Uidoc.Document;

            using (Transaction tr = new Transaction(doc, "Draw Level"))
            {
                String filename;
                try
                {
                    tr.Start();
                    //FilteredElementCollector collector1 = new FilteredElementCollector(doc);
                    //List<Level> levels0 = collector1.OfClass(typeof(Level)).Cast<Level>().ToList();
                    //foreach (Level level in levels0)
                    //{
                    //    doc.Delete(level.Id);
                    //}
                    FilteredElementCollector Collector1 = new FilteredElementCollector(doc);
                    ICollection<Element> xx = Collector1.OfClass(typeof(Level)).ToElements();
                    List<ElementId> elementsToBeDeleted = new List<ElementId>();
                    foreach (Element element in xx)
                    {
                        elementsToBeDeleted.Add(element.Id);
                    }
                    doc.Delete(elementsToBeDeleted);


                    try
                    {
                        filename = GetPath();
                    }
                    catch (Exception Ex)
                    {
                        message = Ex.Message;
                        return Result.Failed;

                    }


                    var levels = new ExcelMapper(filename).Fetch<Imported_Data>();
                    foreach (var level in levels)
                    {
                        Level lev = Level.Create(doc, level.Elevation / 304.8);
                        lev.Name = level.LevelName;
                        //lev.Elevation = level.Elevation;
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        collector.OfClass(typeof(Autodesk.Revit.DB.View));
                        var views = collector.ToElements();
                        foreach (Autodesk.Revit.DB.View view in views)
                        {
                            lev = view.GenLevel;
                        }
                    }
                    tr.Commit();
                    return Result.Succeeded;
                }
                catch (Exception Ex)
                {
                    message = Ex.Message;
                    return Result.Failed;
                }
            }
        }
        public static string GetPath()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Select an Excel File";
            openDialog.Filter = "Excel Files (.xlsx)|*.xlsx";

            openDialog.RestoreDirectory = true;
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = openDialog.FileName;
                return fileName;
            }
            else
                return null;
        }


    }
    public class Imported_Data
    {
        public string LevelName { get; set; }
        public double Elevation { get; set; }
    }
}

