using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AppManager.Core.Models;
using AppManager.Core.Utils;

namespace AppManager.Core.Actions
{
    public class LaunchAction : BaseAction
    {
        public override string Description => "Launches an application";

        public LaunchAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            if (String.IsNullOrEmpty(_Model?.AppName)) { return false; }

            SetExecutablePath();

            return !String.IsNullOrEmpty(_Model.ExecutablePath);
        }

        private void SetExecutablePath()
        {
            if (File.Exists(_Model.ExecutablePath)){ return; }

            string[] executablePaths = Array.Empty<string>();

            if (String.IsNullOrEmpty(_Model.ExecutablePath))
            {
                executablePaths = FileManager.FindExecutables(_Model.AppName);

            }
            else
            {
                executablePaths = FileManager.FindExecutables(_Model.AppName, [_Model.ExecutablePath]);
            }

            if (executablePaths.Length == 1)
            {
                _Model.ExecutablePath = executablePaths[0];
                return;
            }

            
            if (executablePaths.Length > 1)
            {
                string[] newExecutablePaths = executablePaths.Where(p => _Model.AppName == Path.GetFileNameWithoutExtension(p)).ToArray();

                if (0 < newExecutablePaths.Length) 
                { 
                    _Model.ExecutablePath = newExecutablePaths[0]; 
                    return;
                }

                _Model.ExecutablePath = String.Empty;
                Debug.WriteLine($"Could not find executable for: {_Model.AppName}");
            }

        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = _Model.ExecutablePath,
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
            });
        }
    }
}