using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Auto_numbering_of_premises
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            List<Room> rooms = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .ToList();

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .ToList();

            List<double> floor = new List<double>();


            foreach (var level in levels)
            {
                for (int i = 0; i < levels.Count + 1; i++)
                {
                    if (level.Name.Equals($"Этаж {i}"))
                    {
                        floor.Add(i);
                        break;
                    }
                    else if (level.Name.Equals("Подвал"))
                    {
                        floor.Add(-1);
                        break;
                    }
                    else if (level.Name.Equals("План кровли"))
                    {
                        floor.Add(levels.Count);
                        break;
                    }
                }
            }


            List<string> levelsName = new List<string>();


            foreach (var level in levels)
            {
                levelsName.Add(level.Name);
            }

            Transaction transaction = new Transaction(doc);
            transaction.Start("Расстановка отверстий");
            List<Room> rooms1 = new List<Room>();

            string l = null;

            int num = 0;
            foreach (var room in rooms)
            {

                Level level = room.Level;
                Parameter numPar = room.LookupParameter("Номер");
                int index = 0;
                if (level != null)
                {
                    index = levelsName.FindIndex(a => a.Contains(level.Name));
                    var t = numPar.Set($"{floor[index]}.{num}");
                    if (level.Name == l || l == null)
                    {
                        num++;
                        index = levelsName.FindIndex(a => a.Contains(level.Name));
                        numPar.Set($"{floor[index]}.{num}");
                    }
                    else
                    {
                        num = 1;
                        index = levelsName.FindIndex(a => a.Contains(level.Name));
                        numPar.Set($"{floor[index]}.{num}");

                    }
                }
                l = level.Name;
            }
            transaction.Commit();
            return Result.Succeeded;
        }
    }
}
