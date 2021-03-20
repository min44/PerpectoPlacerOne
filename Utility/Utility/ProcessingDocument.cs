using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public static class ProcessingDocument
    {
        public static Document CurrentDocument { get; set; }
        public static UIDocument CurrentUIDocument => new UIDocument(CurrentDocument);
        public static Selection Selection => CurrentUIDocument.Selection;

        public static void ExecuteTransactionGroup(Action action, string transactionGroupName) =>
            ExecuteTransactionGroup(CurrentDocument, action, transactionGroupName);

        public static void ExecuteTransactionGroup(Document document, Action action, string transactionGroupName)
        {
            using (var transactionGroup = new TransactionGroup(document, transactionGroupName))
            {
                try
                {
                    transactionGroup.Start();
                    action();
                    transactionGroup.Assimilate();
                }
                catch
                {
                    if (transactionGroup.HasStarted())
                        transactionGroup.RollBack();
                    throw;
                }
            }
        }

        public static void ExecuteTransaction(Action action, string transactionName, bool? showWarnings = null) =>
            ExecuteTransaction(CurrentDocument, action, transactionName, showWarnings);

        public static void ExecuteTransaction(Document document, Action action, string transactionName, bool? showWarnings = null)
        {
            if (TransactionIsOpened(document))
            {
                action?.Invoke();
            }
            else
            {
                using (var transaction = new Transaction(document, transactionName))
                {
                    try
                    {
                        if (transaction.Start() == TransactionStatus.Started)
                        {
                            action?.Invoke();
                        }

                        if (transaction.Commit() != TransactionStatus.Committed)
                        {
                            transaction.RollBack();
                        }
                    }
                    catch (Exception)
                    {
                        if (transaction.HasStarted())
                        {
                            transaction.RollBack();
                        }
                        throw;
                    }
                }
            }
        }

        public static void ExecuteSubTransaction(Document document, Action action)
        {
            using (var subTransaction = new SubTransaction(document))
            {
                try
                {
                    if (subTransaction.Start() == TransactionStatus.Started)
                    {
                        action?.Invoke();
                    }

                    if (subTransaction.Commit() != TransactionStatus.Committed)
                    {
                        subTransaction.RollBack();
                    }
                }
                catch (Exception)
                {
                    if (subTransaction.HasStarted())
                    {
                        subTransaction.RollBack();
                    }
                    throw;
                }
            }
        }

        public static bool TransactionIsOpened() =>
            TransactionIsOpened(CurrentDocument);

        public static bool TransactionIsOpened(Document document) =>
            document.IsReadOnly || document.IsModifiable;

        public static FilteredElementCollector CreateFilteredElementCollector() => new FilteredElementCollector(CurrentDocument);

        public static Element GetElement(ElementId elementId) =>
            GetElement(CurrentDocument, elementId);
        public static Element GetElement(Document document, ElementId elementId) =>
            document.GetElement(elementId);

        public static Element GetElement(int id) =>
            GetElement(new ElementId(id));
        public static Element GetElement(Document document, int id) =>
            document.GetElement(new ElementId(id));

        public static T GetElementAs<T>(Element element) =>
            GetElementAs<T>(CurrentDocument, element);
        public static T GetElementAs<T>(Document document, Element element) =>
            document.GetElement(element.Id) is T t ? t : default;

        public static T GetElementAs<T>(ElementId elementId) =>
            GetElementAs<T>(CurrentDocument, elementId);
        public static T GetElementAs<T>(Document document, ElementId elementId) =>
            document.GetElement(elementId) is T t ? t : default;

        public static T GetElementAs<T>(int id) =>
            GetElementAs<T>(CurrentDocument, new ElementId(id));
        public static T GetElementAs<T>(Document document, int id) =>
            document.GetElement(new ElementId(id)) is T t ? t : default;

        public static void DeleteById(int id) => Delete(new ElementId(id));
        public static void Delete(ElementId elementId)
        {
            try
            {
                if (CurrentDocument.GetElement(elementId) != null)
                    CurrentDocument.Delete(elementId);
            }
            catch
            {
                Logger.Error($"[{elementId}]");
                throw;
            }
        }
        public static void Delete(Document document, int id)
        {
            try
            {
                if (document.GetElement(new ElementId(id)) != null)
                    document.Delete(new ElementId(id));
            }
            catch
            {
                Logger.Error($"[{id}]");
                throw;
            }
        }

        public static void SelectElements(IEnumerable<int> elementIds)
        {
            var idsElements = elementIds.Select(n => new ElementId(n)).ToList();
            Selection.SetElementIds(idsElements);
        }

        public static void SelectElements(IEnumerable<Element> elements)
        {
            var idsElements = elements.Select(n => n.Id).ToList();
            Selection.SetElementIds(idsElements);
        }
    }
}