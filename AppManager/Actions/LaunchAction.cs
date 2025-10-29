using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class LaunchAction : IAppAction
    {
        public string Description => "Launches an application";

        public bool CanExecute(ActionModel model)
        {
            if (string.IsNullOrEmpty(model?.AppName))
                return false;

            // Check if executable path is provided and exists
            if (model.ExecutablePath != null)
                return File.Exists(model.ExecutablePath);

            // Try to find the executable in common locations
            return FindExecutable(model.AppName) != null;
        }

        public async Task<bool> ExecuteAsync(ActionModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.AppName))
            {
                Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            try
            {
                string executablePath = model.ExecutablePath ?? FindExecutable(model.AppName);
                
                if (string.IsNullOrEmpty(executablePath))
                {
                    Debug.WriteLine($"Could not find executable for: {model.AppName}");
                    return false;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = model.Arguments ?? string.Empty,
                    UseShellExecute = true
                };

                var process = Process.Start(startInfo);
                
                if (process != null)
                {
                    Debug.WriteLine($"Successfully launched: {model.AppName}");
                    return true;
                }

                Debug.WriteLine($"Failed to launch {model.AppName}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to launch {model.AppName}: {ex.Message}");
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