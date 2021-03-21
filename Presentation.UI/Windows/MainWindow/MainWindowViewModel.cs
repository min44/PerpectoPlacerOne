using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimGen.PerpectoPlacerOne.Utility;
using DevExpress.Mvvm;
using Revit.Async;

namespace BimGen.PerpectoPlacerOne.Presentation.UI
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly Document doc;
        private readonly string externalAssemblyName = "BimGen.PerpectoPlacerOne.Core.Commander.dll";

        string _minWindowWidth = Utility.Properties.Settings.Default.MinWindowWidth;
        string _minWindowHeight = Utility.Properties.Settings.Default.MinWindowHeight;

        dynamic windowBasedExternalEventHandler;
        ExternalEvent windowBasedExternalEvent;

        public ObservableCollection<Family> Families { get; set; }
        public ObservableCollection<FamilySymbol> Symbols { get; set; }

        private ElementId _currentFamilyId;
        private ElementId _currentSymbolId;

        public MainWindowViewModel(Document doc)
        {
            this.doc = doc;

            windowBasedExternalEventHandler = Activator.CreateInstance(AssemblyStarter.GetObjectTypeByName(externalAssemblyName, "WindowBased"));
            windowBasedExternalEvent = ExternalEvent.Create((IExternalEventHandler)windowBasedExternalEventHandler);

            UpdateAll();
        }

        public void UpdateAll()
        {
            Families = GetFamilies();

            if (Families.Count() > 0)
            {
                var currentFamily = Families.FirstOrDefault();

                CurrentFamilyId = currentFamily.Id;

                Symbols = GetSymbols(currentFamily);

                if (Symbols.Count() > 0)
                    CurrentSymbolId = Symbols.FirstOrDefault().Id;

                windowBasedExternalEventHandler.MinWindowWidth = _minWindowWidth;
                windowBasedExternalEventHandler.MinWindowHeight = _minWindowHeight;

            }
            else
            {
                var td = new TaskDialog("Warning");

                td.MainInstruction = "Families is required";
                td.MainContent = "There are no families in the project. Do you want to load it?";
                td.MainIcon = TaskDialogIcon.TaskDialogIconInformation;

                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Yes, load families into the project");
                td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "No, don't load it");

                TaskDialogResult tdres = td.Show();

                switch (tdres)
                {
                    case TaskDialogResult.CommandLink1:
                        FamilyLoader.LoadFamilies(doc);
                        UpdateAll();
                        break;
                    case TaskDialogResult.CommandLink2:
                        break;
                    default:
                        break;
                }
            }
        }

        public ElementId CurrentFamilyId
        {
            get { return _currentFamilyId; }
            set
            {
                _currentFamilyId = value;

                Logger.Debug($"OnCurrentFamilyIdChange: {_currentFamilyId}");

                var family = doc.GetElement(_currentFamilyId) as Family;

                Symbols = GetSymbols(family);

                if (Symbols.Count() > 0) CurrentSymbolId = Symbols.FirstOrDefault().Id;
            }
        }

        public ElementId CurrentSymbolId
        {
            get => _currentSymbolId;
            set
            {
                _currentSymbolId = value;
                Logger.Debug($"OnCurrentSymbolIdChange: {_currentSymbolId}");
                if (_currentSymbolId != null)
                    windowBasedExternalEventHandler.SymbolId = _currentSymbolId;
            }
        }

        public string MinWindowWidth
        {
            get => _minWindowWidth;
            set
            {
                _minWindowWidth = value;
                windowBasedExternalEventHandler.MinWindowWidth = _minWindowWidth;
                Utility.Properties.Settings.Default.MinWindowWidth = _minWindowWidth;
            }
        }

        public string MinWindowHeight
        {
            get => _minWindowHeight;
            set
            {
                _minWindowHeight = value;
                windowBasedExternalEventHandler.MinWindowHeight = _minWindowHeight;
                Utility.Properties.Settings.Default.MinWindowHeight = _minWindowHeight;
            }
        }

        public ICommand WindowBased => new DelegateCommand(() => windowBasedExternalEvent.Raise());

        public ICommand RemoveAll => new AsyncCommand(async () =>
        {
            await RevitTask.RunAsync(
                app =>
                        new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_GenericModel)
                        .WhereElementIsNotElementType()
                        .ToElements()
                        .Where(x => ((FamilyInstance)x).SuperComponent == null)
                        .ToList()
                        .ForEach(x => ProcessingDocument.ExecuteTransaction(() => doc.Delete(x.Id), "Delete element"))
                    );
        });

        public string Count { get; set; }
        public ICommand GetCount => new AsyncCommand(async () =>
        {
            try
            {
                var objType = AssemblyStarter.GetObjectTypeByName(externalAssemblyName, "LibCommander");
                var method = objType.GetMethod("GetCount").MakeGenericMethod(typeof(string));
                var preresult = (Task<int>)method.Invoke(objType, new object[] { "Hello!" });
                var result = await preresult;
                Logger.Debug($"Result: {result}");
                Count = $"Count of elements: {result}";
            }
            catch (Exception ex)
            {
                Logger.Debug($"Error: {ex.Message}");
            }
        }
        );


        private ObservableCollection<Family> GetFamilies()
        {
            Logger.Debug("Get families");
            var collection = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .Where(x => x.Name.StartsWith(Utility.Properties.Settings.Default.CableTrayNameFamilyKey))
                .Cast<Family>();
            return new ObservableCollection<Family>(collection);
        }

        private ObservableCollection<FamilySymbol> GetSymbols(Family family)
        {
            ICollection<ElementId> symbolIds = family.GetFamilySymbolIds().ToList();
            var collection = new FilteredElementCollector(doc, symbolIds)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>();
            return new ObservableCollection<FamilySymbol>(collection);
        }
    }
}