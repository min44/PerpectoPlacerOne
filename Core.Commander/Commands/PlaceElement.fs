namespace BimGen.PerpectoPlacerOne.Core.Commander

open System.Threading
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.Exceptions
open BimGen.PerpectoPlacerOne.Utility

[<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>]
[<Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)>]
type PlaceElement() =
    member val SymbolId: ElementId = null with get, set

    interface IExternalEventHandler with
        override this.Execute (app) =
            let uidoc = app.ActiveUIDocument
            let doc = uidoc.Document

            let reference =
                try
                    uidoc.Selection.PickObject(
                        ObjectType.Element,
                        new SelectionFilterByCategory(BuiltInCategory.OST_Windows),
                        "Select one basic wall."
                    )
                with :? OperationCanceledException -> null

            if reference = null then
                TaskDialog.Show("Canceled", "Operation was canceled") |> ignore
            else
                let window = doc.GetElement(reference.ElementId) :?> FamilyInstance
                let wall = window.Host :?> Wall
                let opts = new Options()
                opts.ComputeReferences <- true
                let solid = wall.get_Geometry (opts) |> Seq.cast<Solid> |> Seq.exactlyOne
                let face = solid.Faces.get_Item (1)
                let location = (window.Location :?> LocationPoint).Point
                let symbl = doc.GetElement(this.SymbolId) :?> FamilySymbol
                let direction = ((wall.Location :?> LocationCurve).Curve :?> Line).Direction

                try
                    Logger.Debug($"ContextID: {Thread.CurrentContext.ContextID}")
                    use transaction = new Transaction(doc)
                    let status = transaction.Start("Create element")
                    symbl.Activate()
                    let inst = doc.Create.NewFamilyInstance(face, location, direction.Negate(), symbl)
                    let winwidth = window.Symbol.LookupParameter("Width").AsDouble()
                    let winheight = window.Symbol.LookupParameter("Height").AsDouble()
                    let preswinwidth = inst.LookupParameter("Ширина").Set(winwidth)
                    let preswinheight = inst.LookupParameter("Высота").Set(winheight)
                    let preswinoffset = inst.LookupParameter("Offset").Set(0)
                    transaction.Commit() |> ignore
                with ex ->
                    TaskDialog.Show("Something went wrong", $"Message: {ex.Message} Source: {ex.Source}")
                    |> ignore

        override this.GetName () = nameof PlaceElement
