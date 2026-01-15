using AppManager.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;

namespace AppManager.Tests.TestUtilities
{
    /// <summary>
    /// Utility class for managing AppManager.exe test instance
    /// </summary>
    public static class TestAppManager
    {
        private static Process? _appManagerProcess;
        private static readonly string AppManagerPath = GetAppManagerPath();

        /// <summary>
        /// Starts AppManager.exe for testing purposes
        /// </summary>
        public static Process StartTestInstance()
        {
            if (_appManagerProcess?.HasExited == false)
            {
                return _appManagerProcess;
            }

            if (!File.Exists(AppManagerPath))
            {
                throw new FileNotFoundException($"AppManager.exe not found at: {AppManagerPath}");
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = AppManagerPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            _appManagerProcess = Process.Start(startInfo);
            
            if (_appManagerProcess == null)
            {
                throw new InvalidOperationException("Failed to start AppManager.exe test instance");
            }

            // Wait a moment for the process to initialize
            //Thread.Sleep(2000);
            
            return _appManagerProcess;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestAppManager.StopTestInstance();
        }

        /// <summary>
        /// Stops the test AppManager instance
        /// </summary>
        public static void StopTestInstance()
        {
            try
            {
                if (_appManagerProcess?.HasExited == false)
                {
                    _appManagerProcess.CloseMainWindow();
                    
                    // Give it time to close gracefully
                    if (!_appManagerProcess.WaitForExit(3000))
                    {
                        _appManagerProcess.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping test AppManager instance: {ex.Message}");
            }
            finally
            {
                _appManagerProcess?.Dispose();
                _appManagerProcess = null;
            }
        }

        /// <summary>
        /// Checks if the test instance is running
        /// </summary>
        public static bool IsTestInstanceRunning()
        {
            return _appManagerProcess?.HasExited == false;
        }

        /// <summary>
        /// Gets the test instance process
        /// </summary>
        public static Process? GetTestInstance()
        {
            return _appManagerProcess;
        }

        private static string GetAppManagerPath()
        {
            // Look for AppManager.exe in common locations
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            // Check in the same directory as tests
            string localPath = Path.Combine(baseDirectory, "AppManager.exe");
            if (File.Exists(localPath))
            {
                return localPath;
            }
            
            // Check in parent directories (common in development)
            string? parentDir = Directory.GetParent(baseDirectory)?.FullName;
            while (parentDir != null)
            {
                string searchPath = Path.Combine(parentDir, "AppManager", "bin", "Debug", "net10.0-windows", "AppManager.exe");
                if (File.Exists(searchPath))
                {
                    return searchPath;
                }
                
                searchPath = Path.Combine(parentDir, "AppManager", "bin", "Release", "net10.0-windows", "AppManager.exe");
                if (File.Exists(searchPath))
                {
                    return searchPath;
                }
                
                parentDir = Directory.GetParent(parentDir)?.FullName;
            }
            
            // Fallback to searching in the solution directory
            string solutionDir = GetSolutionDirectory();
            if (!string.IsNullOrEmpty(solutionDir))
            {
                string[] searchPatterns = {
                    Path.Combine(solutionDir, "AppManager", "bin", "Debug", "net10.0-windows", "AppManager.exe"),
                    Path.Combine(solutionDir, "AppManager", "bin", "Release", "net10.0-windows", "AppManager.exe"),
                    Path.Combine(solutionDir, "*", "AppManager.exe")
                };
                
                foreach (string pattern in searchPatterns)
                {
                    if (File.Exists(pattern))
                    {
                        return pattern;
                    }
                }
            }
            
            throw new FileNotFoundException("AppManager.exe could not be located for testing");
        }

        private static string GetSolutionDirectory()
        {
            string currentDir = Directory.GetCurrentDirectory();
            
            while (currentDir != null)
            {
                if (Directory.GetFiles(currentDir, "*.sln").Any())
                {
                    return currentDir;
                }
                
                currentDir = Directory.GetParent(currentDir)?.FullName ?? string.Empty;
            }
            
            return string.Empty;
        }
    }
}