namespace BimGen.PerpectoPlacerOne.Core.Commander

open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.ApplicationServices
open Autodesk.Revit.Exceptions
open BimGen.PerpectoPlacerOne.Utility

[<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>]
type CreateElement() =
    interface IExternalEventHandler with
        override this.Execute (app) =
            let uidoc = app.ActiveUIDocument
            let doc = uidoc.Document

            let reference =
                try
                    uidoc.Selection.PickObject(
                        ObjectType.Element,
                        new SelectionFilterByCategory(BuiltInCategory.OST_Walls),
                        "Select one basic wall."
                    )
                with :? OperationCanceledException -> null

            if reference = null then
                TaskDialog.Show("Canceled", "Operation was canceled") |> ignore
            else
                let wall = doc.GetElement(reference.ElementId) :?> Wall
                let locationCurve = wall.Location :?> LocationCurve 
                let line = locationCurve.Curve :?> Line
                TaskDialog.Show("Info", $"{wall.Width}") |> ignore
                ()

        override this.GetName () = nameof CreateElement
