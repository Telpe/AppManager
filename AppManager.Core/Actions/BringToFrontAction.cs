using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public partial class BringToFrontAction : BaseAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.BringToFront;
        public override string Description => "Brings an application window to the front and makes it topmost temporarily";

        public string? WindowTitle { get; set; }

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
        private Process? _TargetProcess;

        public BringToFrontAction(ActionModel model) : base(model)
        { 
            AppName = model.AppName;
            WindowTitle = model.WindowTitle;
        }

        protected override bool CanExecuteAction()
        {
            _TargetProcess = GetTargetProcess();

            return !string.IsNullOrEmpty(AppName) && !(_TargetProcess?.HasExited??true);
        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    IntPtr mainWindowHandle = _TargetProcess.MainWindowHandle;
                
                    if (mainWindowHandle == IntPtr.Zero)
                    {
                        Debug.WriteLine($"No main window found for process: {_TargetProcess.ProcessName}");
                        return false;
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
                    Task.Delay(100).Wait();
                    SetWindowPos(mainWindowHandle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);

                    Debug.WriteLine($"Successfully brought to front: {AppName}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to bring to front {AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        private Process? GetTargetProcess()
        {
            try
            {
                Process[]? processes = Process.GetProcessesByName(AppName);

                // If window title is specified, filter by it
                if (!string.IsNullOrEmpty(WindowTitle))
                {
                    processes = processes.Where(p => p.MainWindowTitle.IndexOf(WindowTitle, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                }
                else
                {
                    processes = processes.Where(p => p.MainWindowHandle != IntPtr.Zero).ToArray();
                }

                return processes.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding process to bring to front {AppName}: {ex.Message}");
                return null;
            }
        }

        public override ActionModel ToModel()
        {
            return new ActionModel()
            {
                ActionType = ActionType,
                AppName = AppName,
                WindowTitle = WindowTitle,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}