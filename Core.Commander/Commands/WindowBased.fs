namespace BimGen.PerpectoPlacerOne.Core.Commander

open System
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.Exceptions
open BimGen.PerpectoPlacerOne.Utility

module funlib =
    let getParameter (instance: FamilyInstance) (parameterId: BuiltInParameter) =
        let parameter = instance.Symbol.get_Parameter (parameterId)

        match parameter.HasValue with
        | true -> parameter.AsDouble()
        | false -> instance.get_Parameter(parameterId).AsDouble()

    let getDirection(wall: Wall) =
        let direction = ((wall.Location :?> LocationCurve).Curve :?> Line).Direction

        match wall.Flipped with
        | true -> direction
        | false -> direction.Negate()

    let getLocation(window: FamilyInstance) = (window.Location :?> LocationPoint).Point

[<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>]
[<Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)>]
type WindowBased() =
    let mutable minWindowWidth = 0.0
    let mutable minWindowHeight = 0.0

    member this.MinWindowWidth
        with set value =
            UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), UnitType.UT_Length, value, &minWindowWidth)
            |> ignore

    member this.MinWindowHeight
        with set value =
            UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), UnitType.UT_Length, value, &minWindowHeight)
            |> ignore

    member val SymbolId: ElementId = null with get, set

    interface IExternalEventHandler with
        override this.Execute (app) =
            try
                let uidoc = app.ActiveUIDocument
                let doc = uidoc.Document

                let ``Ссылка на экземлпяр связи`` as reference =
                    try
                        uidoc.Selection.PickObject(
                            ObjectType.Element,
                            new SelectionFilter(typeof<RevitLinkInstance>),
                            "Select one basic wall."
                        )
                    with :? OperationCanceledException -> null

                if reference = null then
                    TaskDialog.Show("Canceled", "Operation was canceled") |> ignore
                else
                    let linkInstace = doc.GetElement(reference.ElementId) :?> RevitLinkInstance
                    let linkdoc = linkInstace.GetLinkDocument()
                    let collector = new FilteredElementCollector(linkdoc)

                    let windows =
                        collector
                            .OfCategory(BuiltInCategory.OST_Windows)
                            .WhereElementIsNotElementType()
                            .ToElements()

                    windows
                    |> Seq.cast<FamilyInstance>
                    |> Seq.iter
                        (fun window ->
                            try
                                let winWidth = funlib.getParameter window BuiltInParameter.FAMILY_WIDTH_PARAM
                                let winHeight = funlib.getParameter window BuiltInParameter.FAMILY_HEIGHT_PARAM
                                
                                let rounddigits = 14
                                let minWindowWidthRounded = Math.Round(minWindowWidth, rounddigits)
                                let minWindowHeightRounded = Math.Round(minWindowHeight, rounddigits)
                                let winWidthRounded = Math.Round(winWidth, rounddigits)
                                let winHeightRounded = Math.Round(winHeight, rounddigits)

                                Logger.Debug($"winWidth: {winWidthRounded} >= {minWindowWidthRounded}")
                                |> ignore

                                Logger.Debug($"winHeight: {winHeightRounded} >= {minWindowHeightRounded}")
                                |> ignore

                                if (winWidthRounded >= minWindowWidthRounded
                                    && winHeightRounded >= minWindowHeightRounded) then
                                    let wall = window.Host :?> Wall

                                    let face =
                                        HostObjectUtils
                                            .GetSideFaces(wall, ShellLayerType.Exterior)
                                            .Item(0)
                                            .CreateLinkReference(linkInstace)

                                    let location = funlib.getLocation window
                                    let direction = funlib.getDirection wall
                                    let symbol = doc.GetElement(this.SymbolId) :?> FamilySymbol
                                    use transaction = new Transaction(doc)
                                    let status = transaction.Start("Create element")
                                    symbol.Activate()
                                    let inst = doc.Create.NewFamilyInstance(face, location, direction, symbol)
                                    let preswinwidth = inst.LookupParameter("Ширина").Set(winWidth)
                                    let preswinheight = inst.LookupParameter("Высота").Set(winHeight)

                                    let preswinoffset =
                                        inst
                                            .get_Parameter(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM)
                                            .Set(0)

                                    Logger.Debug($"Success {window.Name} winwidth={winWidth} winheight={winHeight}")
                                    transaction.Commit() |> ignore
                            with _ -> Logger.Debug($"Success {window.Name} id={window.Id}"))
            with ex ->
                TaskDialog.Show("Something went wrong", $"Message: {ex.Message} Source: {ex.Source}")
                |> ignore

        override this.GetName () = nameof WindowBased
