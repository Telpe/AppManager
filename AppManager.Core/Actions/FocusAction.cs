using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public partial class FocusAction : BaseAction, IFocusAction
    {
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Focus;
        public override string Description => "Brings an application window to the foreground";
        public string? AppName { get; set; }

        public bool? IncludeSimilarNames { get; set; }
        public string? WindowTitle { get; set; }

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool SetForegroundWindow(IntPtr hWnd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [return: MarshalAs(UnmanagedType.Bool)]
        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        public FocusAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeSimilarNames = model.IncludeSimilarNames;
            WindowTitle = model.WindowTitle;

        }

        protected override bool CanExecuteAction()
        {
            return !string.IsNullOrEmpty(AppName);
        }

        protected override bool ExecuteAction()
        {
            Process process = ProcessManager.FindProcess(
                AppName!, 
                IncludeSimilarNames ?? false, 
                requireMainWindow: true,
                WindowTitle) ?? throw new Exception($"No process found to focus: {AppName}");
            
            IntPtr mainWindowHandle = process.MainWindowHandle;
                
            if (mainWindowHandle == IntPtr.Zero)
            {
                throw new Exception($"No main window found for process: {process.ProcessName}");
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

            if (SetForegroundWindow(mainWindowHandle))
            {
                Log.WriteLine($"Successfully focused window for: {AppName}");
                return true;
            }
            else
            {
                throw new Exception($"Failed to focus window for: {AppName}");
            }
        }

        public override ActionModel ToModel()
        {
            return new ActionModel
            {
                AppName = AppName,
                ActionType = ActionType,
                IncludeSimilarNames = IncludeSimilarNames,
                WindowTitle = WindowTitle,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}