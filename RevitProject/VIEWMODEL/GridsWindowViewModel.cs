using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RevitProject
{
    public class GridsWindowViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void onPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                      
        }

        #endregion

        #region Constructor
        public GridsWindowViewModel()
        {
            CadLoad();
            OK = new Command(DoneCommand);
            RenumberGrid = new Command(GridStyle);
        }
        #endregion

        #region Properties & Fields

        Document Doc = GridsCommand.Doc;
        
        private IList<Arc> arcs = new List<Arc>();
        private IList<Line> lines = new List<Line>();
        private List<Grid> VerticalGrids = new List<Grid>();
        private List<Grid> HorizontalGrids = new List<Grid>();

        private List<string> letters = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        public Command OK { get; set; }
        public Command RenumberGrid { get; set; }
        public IList<string> Layersname { get; set; } = new List<string>();

        public IList<string> GridsNameStyles { get; set; } = new List<String>() { "Numbers : 1,2,3,...","Letters : A,B,C,...", "Letters : a,b,c,...", "Roman : I,II,III,..." };

        private string _selectedlayer;

        public string Selectedlayer
        {
            get { return _selectedlayer; }
            set
            {

                _selectedlayer = value;

                onPropertyChanged(nameof(Selectedlayer));
            }
        }
        private string _selectedVerticalNumbering;

        public string SelectedVerticalNumbering
        {
            get { return _selectedVerticalNumbering; }
            set
            {

                _selectedVerticalNumbering = value;

                onPropertyChanged(nameof(SelectedVerticalNumbering));
            }
        }
        private string _selectedHorizontalNumbering;

        public string SelectedHorizontalNumbering
        {
            get { return _selectedHorizontalNumbering; }
            set
            {

                _selectedHorizontalNumbering = value;

                onPropertyChanged(nameof(SelectedHorizontalNumbering));
            }
        }
        private Boolean _reverseVertical;

        public Boolean ReverseVertical
        {
            get { return _reverseVertical; }
            set
            {
                _reverseVertical = value;
                onPropertyChanged(nameof(ReverseVertical));
            }
        }
        private Boolean _reverseHorizontal;

        public Boolean ReverseHorizontal
        {
            get { return _reverseHorizontal; }
            set
            {
                _reverseHorizontal = value;
                onPropertyChanged(nameof(ReverseHorizontal));
            }
        }

        private Boolean _dimensions = true;

        public Boolean Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                onPropertyChanged(nameof(Dimensions));
            }
        }

        #endregion

        #region Methods
        public void GridStyle()
        {
            GridSorting();
            RenumberVerticalGrids();
            RenumberHorizontalGrids();
            
        }
        public void CadLoad()
        {
            IList<ElementId> cadimports = (IList<ElementId>)new FilteredElementCollector(Doc).OfClass(typeof(ImportInstance))
                .WhereElementIsNotElementType().ToElementIds();
           
            if (cadimports.Count > 0)
            {
                ImportInstance imp = Doc.GetElement(cadimports.First()) as ImportInstance;
                GeometryElement geoel = imp.get_Geometry(new Options());
                foreach (GeometryObject go in geoel)
                {
                    if (go is GeometryInstance)
                    {
                        GeometryInstance gi = go as GeometryInstance;
                        GeometryElement gl = gi.GetInstanceGeometry();
                        if (gl != null)
                        {
                            foreach (GeometryObject go2 in gl)
                            {                              
                                if (go2 is Arc)
                                {
                                    GraphicsStyle gstyle = Doc.GetElement(go2.GraphicsStyleId) as GraphicsStyle;
                                    string layer = gstyle.GraphicsStyleCategory.Name;
                                    if (!Layersname.Contains(layer))
                                    {
                                        Layersname.Add(layer);
                                    }
                                    arcs.Add(go2 as Arc);
                                }
                                if (go2 is Line)
                                {
                                    GraphicsStyle gstyle = Doc.GetElement(go2.GraphicsStyleId) as GraphicsStyle;
                                    string layer = gstyle.GraphicsStyleCategory.Name;
                                    if (!Layersname.Contains(layer))
                                    {
                                        Layersname.Add(layer);
                                    }
                                    lines.Add(go2 as Line);
                                }

                            }
                        }
                    }
                }

            }
            else
            {
                TaskDialog.Show("error", "can't find cad import");
            }
        }

        public void AddDimensions()
        {           
            using (Transaction trans = new Transaction(Doc, "Add Dimensions Between Grids"))
            {
                trans.Start();

               

                trans.Commit();
            }

        }

        private void SelectedNumberingStyle(List<Grid> griddirection,string selectedNumberingStyle)
        {
            for (int i = 0; i < griddirection.Count; i++)
            {
                if (selectedNumberingStyle == GridsNameStyles[0])
                {
                    griddirection[i].Name = (i + 1).ToString();
                }
                else if (selectedNumberingStyle == GridsNameStyles[1])
                {
                    griddirection[i].Name = letters[i % 26];
                }
                else if (selectedNumberingStyle == GridsNameStyles[2])
                {
                    griddirection[i].Name = letters[i % 26].ToLower();
                }
                else if (selectedNumberingStyle == GridsNameStyles[3])
                {
                    griddirection[i].Name = ToRoman(i + 1);
                }               
            }
        }
        public void RenumberHorizontalGrids()
        {           
           
            using (Transaction t = new Transaction(Doc, "Renumber Grids"))
            {
                t.Start();
                SelectedNumberingStyle(HorizontalGrids, _selectedHorizontalNumbering);                          
                Doc.Regenerate();
                t.Commit();
            }
        }
        public void RenumberVerticalGrids()
        {
            using (Transaction t = new Transaction(Doc, "Renumber Grids"))
            {
                t.Start();
                SelectedNumberingStyle(VerticalGrids, _selectedVerticalNumbering);
                Doc.Regenerate();
                t.Commit();
            }
        }

        private void GridSorting()
        {
            
            List<Grid> grids = new FilteredElementCollector(Doc).OfCategory(BuiltInCategory.OST_Grids).WhereElementIsNotElementType().Cast<Grid>().ToList();
            using (Transaction t = new Transaction(Doc, "Sort Grids"))
            {
                HorizontalGrids = null;
                VerticalGrids = null;
                t.Start();
                int i = 0;
                foreach (Grid grid in grids)
                {
                    Curve curve = grid.Curve;
                    XYZ startPoint = curve.GetEndPoint(0);
                    XYZ endPoint = curve.GetEndPoint(1);
                    if (Math.Abs(startPoint.Y - endPoint.Y) < Math.Abs(startPoint.X - endPoint.X))
                    {
                        HorizontalGrids.Add(grid);
                        grid.Name = "grid" + ++i;
                    }
                    else
                    {
                        VerticalGrids.Add(grid);
                        grid.Name = "grid" + ++i;
                    }
                }
                if (_reverseHorizontal)
                {
                    HorizontalGrids = HorizontalGrids.OrderBy(g => g.Curve.GetEndPoint(0).Y).ToList();
                }
                else
                {
                    HorizontalGrids = HorizontalGrids.OrderByDescending(g => g.Curve.GetEndPoint(0).Y).ToList();
                }
                if (_reverseVertical)
                {
                    VerticalGrids = VerticalGrids.OrderBy(g => g.Curve.GetEndPoint(0).X).ToList();
                }
                else
                {
                    VerticalGrids = VerticalGrids.OrderByDescending(g => g.Curve.GetEndPoint(0).X).ToList();
                }                                           
                Doc.Regenerate();
                t.Commit();
            }
        }
        public void DoneCommand()
        {
            Document Doc = GridsCommand.Doc;
            foreach (Line l in lines)
            {
                GraphicsStyle gstyle = Doc.GetElement(l.GraphicsStyleId) as GraphicsStyle;
                string layer = gstyle.GraphicsStyleCategory.Name;
                if (layer == Selectedlayer)
                {                   
                    using (Transaction transs = new Transaction(Doc, "create"))
                    {
                        transs.Start();
                        try
                        {
                            Autodesk.Revit.DB.Grid gg = Grid.Create(Doc, l);
                           
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show(ex.Message, ex.ToString());

                        }
                        transs.Commit();
                    }

                }
                
            }
            foreach (Arc arc in arcs)
            {
                GraphicsStyle gstyle = Doc.GetElement(arc.GraphicsStyleId) as GraphicsStyle;
                string layer = gstyle.GraphicsStyleCategory.Name;
                if (layer == Selectedlayer)
                {                   
                    using (Transaction transs = new Transaction(Doc, "create"))
                    {
                        transs.Start();
                        try
                        {
                            Autodesk.Revit.DB.Grid gg = Grid.Create(Doc, arc);                            
                        }
                        catch (Exception ex)
                        {
                            TaskDialog.Show(ex.Message, ex.ToString());

                        }
                        transs.Commit();
                    }
                }
            }
        }
        private static string ToRoman(int num)
        {
            string[] thousands = { "", "M", "MM", "MMM" };
            string[] hundreds = { "", "C", "CC", "CCC", "CD", "D", "DC", "DCC", "DCCC", "CM" };
            string[] tens = { "", "X", "XX", "XXX", "XL", "L", "LX", "LXX", "LXXX", "XC" };
            string[] ones = { "", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX" };

            int thousandsIndex = num / 1000;
            int hundredsIndex = (num % 1000) / 100;
            int tensIndex = (num % 100) / 10;
            int onesIndex = num % 10;

            return thousands[thousandsIndex] + hundreds[hundredsIndex] + tens[tensIndex] + ones[onesIndex];
        }


        #endregion
    }
}