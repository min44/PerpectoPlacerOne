namespace BimGen.PerpectoPlacerOne.Core.Commander

open System
open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.Exceptions
open BimGen.PerpectoPlacerOne.Utility
open Revit.Async

module LibCommander =
    let GetCount app =
        async {
            Logger.Debug($"app: {app}")

            let! value =
                RevitTask.RunAsync
                    (fun (app: UIApplication) ->
                        (new FilteredElementCollector(app.ActiveUIDocument.Document))
                            .OfCategory(
                            BuiltInCategory.OST_GenericModel
                        )
                            .WhereElementIsNotElementType()
                            .ToElements()
                            .Count)
                |> Async.AwaitTask

            return value
        }
        |> Async.StartAsTask
