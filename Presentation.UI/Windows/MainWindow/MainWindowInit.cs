using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BimGen.PerpectoPlacerOne.Utility;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BimGen.PerpectoPlacerOne.Presentation.UI
{
    public class MainWindowInit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var process = Process.GetCurrentProcess();
            var winname = typeof(MainWindow).Namespace + "." + nameof(MainWindow);

            if (WindowAlreadyExist(process, winname))
            {
                Message.Display("Main window already exist", WindowType.Information);
                return Result.Failed;
            }

            RevitTask.Initialize();

            var mainWindow = new MainWindow(commandData.Application.ActiveUIDocument.Document);

            mainWindow.Closed += (s, e) => Utility.Properties.Settings.Default.Save();

            new WindowInteropHelper(mainWindow) { Owner = process.MainWindowHandle };

            if (!mainWindow.IsVisible) mainWindow.Show();
            if (mainWindow.WindowState == WindowState.Minimized) mainWindow.WindowState = WindowState.Normal;

            mainWindow.Activate();
            mainWindow.Topmost = true;
            mainWindow.Topmost = false;
            mainWindow.Focus();

            return Result.Succeeded;
        }

        #region Single instance of window and correct expand/collapse behavor

        public delegate bool Win32Callback(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.Dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr parentHandle, Win32Callback callback, IntPtr lParam);

        private static bool WindowAlreadyExist(Process process, string windowName)
        {
            IEnumerable<IntPtr> handleWindows = GetRootWindowsOfProcess(process.Id);

            var window = handleWindows
                .Where(x => HwndSource.FromHwnd(x) != null)
                .Select(x => HwndSource.FromHwnd(x).RootVisual as Window)
                .FirstOrDefault(x => x.ToString() == windowName);

            return window != null;
        }

        private static IEnumerable<IntPtr> GetRootWindowsOfProcess(int pid)
        {
            IEnumerable<IntPtr> rootWindows = GetChildWindows(IntPtr.Zero);
            var dsProcRootWindows = new List<IntPtr>();
            foreach (IntPtr hWnd in rootWindows)
            {
                uint lpdwProcessId;
                GetWindowThreadProcessId(hWnd, out lpdwProcessId);
                if (lpdwProcessId == pid)
                    dsProcRootWindows.Add(hWnd);
            }
            return dsProcRootWindows;
        }

        private static IEnumerable<IntPtr> GetChildWindows(IntPtr parent)
        {
            var result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                var childProc = new Win32Callback(EnumWindow);
                EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            var list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            return true;
        }

        #endregion
    }
}
