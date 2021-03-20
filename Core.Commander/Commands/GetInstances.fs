namespace BimGen.PerpectoPlacerOne.Core.Commander

open Autodesk.Revit.UI
open Autodesk.Revit.DB
open BimGen.PerpectoPlacerOne.Utility

module A =
    let displayName(element: Element) = TaskDialog.Show("Info", $"Name: {element.Name}") |> ignore

    let displayLength(elementSet: List<Element>) =
        TaskDialog.Show("Info", $"Length: {elementSet.Length}") |> ignore

[<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>]
type GetInstances() =
    interface IExternalEventHandler with
        override this.Execute (app) =
            let doc = app.ActiveUIDocument.Document
            let fec = new FilteredElementCollector(doc)

            let len =
                fec.OfClass(typeof<FamilyInstance>).ToElements()
                |> Seq.toList
                |> List.iter A.displayName

            ()

        override this.GetName () = nameof GetInstances
