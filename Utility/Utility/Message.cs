using Autodesk.Revit.UI;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public static class Message
    {
        public static void Display(string message, WindowType type)
        {
            string title = "";
            var icon = TaskDialogIcon.TaskDialogIconNone;

            switch (type)
            {
                case WindowType.Information:
                    title = "INFORMATION";
                    icon = TaskDialogIcon.TaskDialogIconInformation;
                    break;
                case WindowType.Warning:
                    title = "WARNING";
                    icon = TaskDialogIcon.TaskDialogIconWarning;
                    break;
                case WindowType.Error:
                    title = "EROOR";
                    icon = TaskDialogIcon.TaskDialogIconError;
                    break;
            }

            var window = new TaskDialog(title)
            {
                MainContent = message,
                MainIcon = icon,
                CommonButtons = TaskDialogCommonButtons.Ok
            };
            window.Show();
        }
    }

    public enum WindowType
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }
}
