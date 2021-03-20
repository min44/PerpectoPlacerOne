using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimGen.PerpectoPlacerOne.Utility;

namespace BimGen.PerpectoPlacerOne.Application.Startup
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class ShowWindow : IExternalCommand
    {
        readonly string assemblyName = "BimGen.PerpectoPlacerOne.Presentation.UI.dll";
        readonly string className = "MainWindowInit";
        readonly string methodName = "Execute";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // MainWindowInit.Show();
            // AssemblyStarter.RunStatic(assemblyName, className, methodName);
            AssemblyStarter.RunAssembly(assemblyName, className, methodName, commandData, ref message, elements);

            return Result.Succeeded;
        }

        public static string GetPath() => typeof(ShowWindow).Namespace + "." + nameof(ShowWindow);
    }
}
