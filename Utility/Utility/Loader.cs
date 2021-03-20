using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.DB;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public static class FamilyLoader
    {
        public static void LoadFamilies(Document doc)
        {
            ProcessingDocument.CurrentDocument = doc;
            string[] famPaths = Directory.GetFiles(Properties.Settings.Default.FamiliesDirectory, "*.rfa", SearchOption.AllDirectories);

            bool isBackUp(string x) => Regex.Match(x, @"\.\d{4}\.rfa\z").Success;

            string[] famPathsFiltered = famPaths.Where(x => !isBackUp(x)).ToArray();

            try
            {
                ProcessingDocument.ExecuteTransaction(() =>
                {
                    foreach (string filePath in famPathsFiltered)
                    {
                        var result = doc.LoadFamily(filePath);
                        Logger.Debug($"Loading result: {result} path: {filePath}");
                    }
                }, "Load families");
            }
            catch (Exception ex) { Logger.Debug(ex.Message); Message.Display("Loading error", WindowType.Error); }
        }
    }
}
