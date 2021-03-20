namespace BimGen.PerpectoPlacerOne.Core.Commander

open Autodesk.Revit.UI
open Autodesk.Revit.DB
open Autodesk.Revit.UI.Selection
open Autodesk.Revit.ApplicationServices
open BimGen.PerpectoPlacerOne.Utility

type SelectionFilterByCategory(category) =
    let mutable _category = BuiltInCategory.OST_Walls
    do _category <- category
    interface ISelectionFilter with
        override this.AllowElement (element) =
            let intValue = element.Category.Id.IntegerValue
            enum<BuiltInCategory> intValue = _category

        override this.AllowReference (reference, position) = false

type SelectionFilter(instype) =
    let mutable _instype = typeof<RevitLinkInstance>
    do _instype <- instype
    interface ISelectionFilter with
        override this.AllowElement (element) =
            element.GetType() = _instype

        override this.AllowReference (reference, position) = false
