namespace BimGen.PerpectoPlacerOne.Core.Commander

open Autodesk.Revit.UI
open BimGen.PerpectoPlacerOne.Utility


[<Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)>]
type ShowMessage() =
    interface IExternalEventHandler with
        override this.Execute (app) =
            let doc = app.ActiveUIDocument.Document
            TaskDialog.Show("Info", "Message was shown from F#") |> ignore
            Logger.Debug("hello from logger F#")

        override this.GetName () = nameof ShowMessage
