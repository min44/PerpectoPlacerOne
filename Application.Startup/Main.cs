using System;
using System.Reflection;
using Autodesk.Revit.UI;
using BimGen.PerpectoPlacerOne.Utility;


namespace BimGen.PerpectoPlacerOne.Application.Startup
{
    public class Main : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            Logger.Debug("Entry point OnStratup");

            string tabName = "PerpectoPlacerOne";
            string panelName = "PerpectoPlacerOne";

            app.CreateRibbonTab(tabName);

            var ribbonPanel = app.CreateRibbonPanel(tabName, panelName);

            var buttonData = new PushButtonData(
                                            Guid.NewGuid().ToString(),
                                            "ShowWindow",
                                            Assembly.GetExecutingAssembly().Location,
                                            ShowWindow.GetPath());

            ribbonPanel.AddItem(buttonData);

            AppDomain.CurrentDomain.AssemblyResolve += ResolveItem;

            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;

        private Assembly ResolveItem(object sender, ResolveEventArgs args)
        {
            string[] trustedAssemblies = new string[] {
                "BimGen.PerpectoPlacerOne.Presentation.UI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                "BimGen.PerpectoPlacerOne.Core.Commander, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" };

            try
            {
                //Logger.Debug($"Requesting assembly {args.RequestingAssembly.FullName}");
                return Array.Exists(trustedAssemblies, x => x == args.RequestingAssembly.FullName) ?
                  Assembly.Load(args.Name) : null;
            }
            catch
            {
                //Logger.Debug($"Loading faild {args.Name}");
                return null;
            }
        }
    }
}
