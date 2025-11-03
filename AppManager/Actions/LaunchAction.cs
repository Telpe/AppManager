using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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
                return File.Exists(_Model.ExecutablePath);

            // Try to find the executable in common locations
            return FindExecutable(_Model.AppName) != null;
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
                string executablePath = _Model.ExecutablePath ?? FindExecutable(_Model.AppName);
                
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

        private string FindExecutable(string appName)
        {
            // Try common executable extensions
            string[] extensions = { ".exe", ".bat", ".cmd", ".msi" };
            
            // Search in PATH environment variable
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (pathEnv != null)
            {
                foreach (string path in pathEnv.Split(Path.PathSeparator))
                {
                    foreach (string ext in extensions)
                    {
                        string fullPath = Path.Combine(path, appName + ext);
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            // Search in common program directories
            string[] commonPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            foreach (string basePath in commonPaths)
            {
                try
                {
                    foreach (string ext in extensions)
                    {
                        string pattern = $"*{appName}*{ext}";
                        var files = Directory.GetFiles(basePath, pattern, SearchOption.AllDirectories);
                        if (files.Length > 0)
                            return files[0];
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error searching in {basePath}: {ex.Message}");
                }
            }

            return null;
        }
    }
}