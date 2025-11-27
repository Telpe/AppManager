using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace AppManager.Core.Utils
{
    /// <summary>
    /// Utility class for process management operations including finding, filtering, and managing processes
    /// </summary>
    public static class ProcessManager
    {
        /// <summary>
        /// Finds processes based on the specified criteria
        /// </summary>
        /// <param name="appName">The application name to search for</param>
        /// <param name="includeSimilarNames">Include processes with similar names (substring match)</param>
        /// <param name="windowTitle">Filter by window title (substring match)</param>
        /// <param name="requireMainWindow">Only include processes with main windows</param>
        /// <param name="includeChildProcesses">Include child processes of found processes</param>
        /// <returns>Array of processes matching the criteria</returns>
        public static Process[] FindProcesses(
            string? appName, 
            bool includeSimilarNames = false, 
            string? windowTitle = null, 
            bool requireMainWindow = true,
            bool includeChildProcesses = false)
        {
            if (string.IsNullOrEmpty(appName))
            {
                return Array.Empty<Process>();
            }

            try
            {
                Process[] processes;
                
                if (includeSimilarNames)
                {
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }
                else
                {
                    processes = Process.GetProcessesByName(appName);
                }

                // Filter by main window if required
                if (requireMainWindow)
                {
                    processes = processes
                        .Where(p => p.MainWindowHandle != IntPtr.Zero)
                        .ToArray();
                }

                // Filter by window title if specified
                if (!string.IsNullOrEmpty(windowTitle))
                {
                    processes = processes
                        .Where(p => p.MainWindowTitle.IndexOf(windowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }

                // Include child processes if requested
                if (includeChildProcesses)
                {
                    var allProcesses = processes.ToList();
                    foreach (var parent in processes)
                    {
                        var children = GetChildProcesses(parent.Id);
                        allProcesses.AddRange(children);
                    }
                    processes = allProcesses.Distinct().ToArray();
                }

                return processes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding processes for {appName}: {ex.Message}");
                return Array.Empty<Process>();
            }
        }

        /// <summary>
        /// Finds a single process based on the specified criteria
        /// </summary>
        /// <param name="appName">The application name to search for</param>
        /// <param name="includeSimilarNames">Include processes with similar names (substring match)</param>
        /// <param name="windowTitle">Filter by window title (substring match)</param>
        /// <param name="requireMainWindow">Only include processes with main windows</param>
        /// <returns>First matching process or null if none found</returns>
        public static Process? FindProcess(
            string? appName, 
            bool includeSimilarNames = false, 
            string? windowTitle = null, 
            bool requireMainWindow = true)
        {
            var processes = FindProcesses(appName, includeSimilarNames, windowTitle, requireMainWindow, false);
            return processes.FirstOrDefault();
        }

        /// <summary>
        /// Checks if a process with the specified name is currently running
        /// </summary>
        /// <param name="processName">The process name to check</param>
        /// <returns>True if the process is running, false otherwise</returns>
        public static bool IsProcessRunning(string? processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return false;
            }

            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Any();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking if process is running for {processName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets child processes of the specified parent process ID
        /// </summary>
        /// <param name="parentId">The parent process ID</param>
        /// <returns>Array of child processes</returns>
        public static Process[] GetChildProcesses(int parentId)
        {
            try
            {
                return Process.GetProcesses()
                    .Where(p => GetParentProcessId(p) == parentId)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting child processes for parent ID {parentId}: {ex.Message}");
                return Array.Empty<Process>();
            }
        }

        /// <summary>
        /// Gets the parent process ID of the specified process
        /// </summary>
        /// <param name="process">The process to get the parent ID for</param>
        /// <returns>Parent process ID or -1 if not found</returns>
        public static int GetParentProcessId(Process process)
        {
            if (process == null)
            {
                return -1;
            }

            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    $"SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {process.Id}"))
                {
                    var results = searcher.Get().GetEnumerator();
                    if (results.MoveNext())
                    {
                        return Convert.ToInt32(results.Current["ParentProcessId"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to get parent process ID for {process.ProcessName}: {ex.Message}");
            }
            return -1;
        }

        /// <summary>
        /// Closes processes gracefully with optional timeout and force kill
        /// </summary>
        /// <param name="processes">Array of processes to close</param>
        /// <param name="timeoutMs">Timeout in milliseconds for graceful close</param>
        /// <param name="forceKill">Whether to force kill if graceful close fails</param>
        /// <returns>True if all processes were closed successfully</returns>
        public static bool CloseProcesses(Process[] processes, int timeoutMs = 1000, bool forceKill = false)
        {
            if (processes == null || processes.Length == 0)
            {
                return true;
            }

            bool allClosed = true;

            foreach (var process in processes)
            {
                try
                {
                    if (process.HasExited)
                    {
                        continue;
                    }

                    process.CloseMainWindow();

                    if (!process.WaitForExit(timeoutMs))
                    {
                        Debug.WriteLine($"Process failed to close gracefully: {process.ProcessName} (ID: {process.Id})");
                        if (forceKill)
                        {
                            process.Kill();
                            Debug.WriteLine($"Force killed: {process.ProcessName} (ID: {process.Id})");
                        }
                        else
                        {
                            allClosed = false;
                        }
                    }

                    if (!process.HasExited)
                    {
                        Debug.WriteLine($"Failed to close process {process.ProcessName}: {process.Id}");
                        allClosed = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to close process {process.ProcessName}: {ex.Message}");
                    allClosed = false;
                }
            }

            return allClosed;
        }
    }
}