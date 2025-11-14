using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppManager.Utils;

namespace AppManager.Actions
{
    public class LaunchAction : BaseAction
    {
        public override string Description => "Launches an application";

        public LaunchAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            if (string.IsNullOrEmpty(_Model?.AppName))
                return false;

            // Check if executable path is provided and exists
            if (_Model.ExecutablePath != null)
                return FileManager.FileExists(_Model.ExecutablePath);

            // Try to find the executable in common locations using FileManager
            return FileManager.FindExecutable(_Model.AppName) != null;
        }

        public override async Task<bool> ExecuteAsync()
        {
            if (_Model == null || string.IsNullOrEmpty(_Model.AppName))
            {
                Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            // Check conditions before executing
            if (!CheckConditions())
            {
                Debug.WriteLine($"Conditions not met for launching {_Model.AppName}");
                return false;
            }

            try
            {
                string executablePath = _Model.ExecutablePath ?? FileManager.FindExecutable(_Model.AppName);
                
                if (string.IsNullOrEmpty(executablePath))
                {
                    Debug.WriteLine($"Could not find executable for: {_Model.AppName}");
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = _Model.Arguments ?? string.Empty,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                
                if (process != null)
                {
                    Debug.WriteLine($"Successfully launched: {_Model.AppName}");
                    return true;
                }

                Debug.WriteLine($"Failed to launch {_Model.AppName}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to launch {_Model.AppName}: {ex.Message}");
                return false;
            }
        }
    }
}