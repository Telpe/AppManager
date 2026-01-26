using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public partial class BringToFrontAction : BaseAction, IBringToFrontAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.BringToFront;
        public override string Description => "Brings an application window to the front and makes it topmost temporarily";

        public string? WindowTitle { get; set; }
        public int? ProcessLastId { get; set; }

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool IsIconic(IntPtr hWnd);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public BringToFrontAction(ActionModel model) : base(model)
        { 
            AppName = model.AppName;
            WindowTitle = model.WindowTitle;
            ProcessLastId = model.ProcessLastId;
        }

        protected override bool CanExecuteAction()
        {
            return !String.IsNullOrEmpty(AppName);
        }

        protected override bool ExecuteAction()
        {
            try
            {
                Process[] processes = ProcessManager.FindProcesses(
                            AppName!,
                            includeSimilarNames: false,
                            requireMainWindow: true,
                            WindowTitle);
                try
                {
                    Process? targetProcess;

                    if (-1 < (ProcessLastId ?? -1))
                    {
                        targetProcess = processes.FirstOrDefault(p => p.Id == ProcessLastId);
                    }
                    else
                    {
                        targetProcess = processes.FirstOrDefault();
                    }

                    if (targetProcess is null) 
                    { 
                        Log.WriteLine($"No running process found for: {AppName}");
                        return false; 
                    }

                    IntPtr mainWindowHandle = targetProcess.MainWindowHandle;
                
                    if (mainWindowHandle == IntPtr.Zero)
                    {
                        throw new Exception($"No main window found for process: {targetProcess.ProcessName}");
                    }

                    // If window is minimized, restore it first
                    if (IsIconic(mainWindowHandle))
                    {
                        ShowWindow(mainWindowHandle, SW_RESTORE);
                    }
                    else
                    {
                        ShowWindow(mainWindowHandle, SW_SHOW);
                    }

                    // Make window topmost temporarily
                    SetWindowPos(mainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                
                    // Bring to foreground
                    SetForegroundWindow(mainWindowHandle);
                
                    // Wait a moment, then remove topmost flag
                    Thread.Sleep(CoreConstants.StandardUIDelay);
                    SetWindowPos(mainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                    Log.WriteLine($"Successfully brought to front: {AppName}");
                    return true;
                }
                finally
                {
                    foreach (var p in processes)
                    {
                        p.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to bring to front {AppName}", ex);
            }
        }

        public override ActionModel ToModel()
        {
            return ToActionModel<IBringToFrontAction>();
        }
    }
}