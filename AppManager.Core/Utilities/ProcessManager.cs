using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace AppManager.Core.Utilities
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
            string appName, 
            bool includeSimilarNames = false,
            bool requireMainWindow = false,
            string? windowTitle = null, 
            bool includeChildProcesses = false,
            int excludeId = -1)
        {
            ValidateProcessName(appName, nameof(appName));

            IEnumerable<Process> processes = Enumerable.Empty<Process>();
            IEnumerable<Process> processesTemp = [];

            try
            {
                /*if (includeSimilarNames)
                {
                    processesTemp = Process.GetProcesses();

                    foreach (var p in processesTemp)
                    {
                        if ((includeSimilarNames && - 1 < p.ProcessName.IndexOf(appName, StringComparison.OrdinalIgnoreCase)) || p.ProcessName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                        {
                            processes = processes.Append(p);
                        }
                        else
                        {
                            p.Dispose();
                        }
                    }

                    processesTemp = [];
                }
                else
                {
                    processes = Process.GetProcessesByName(appName);
                }*/

                processesTemp = Process.GetProcesses();

                foreach (var p in processesTemp)
                {
                    if ((includeSimilarNames && -1 < p.ProcessName.IndexOf(appName, StringComparison.OrdinalIgnoreCase)) || p.ProcessName.Equals(appName, StringComparison.OrdinalIgnoreCase))
                    {
                        processes = processes.Append(p);
                    }
                    else
                    {
                        p.Dispose();
                    }
                }

                processesTemp = processes;

                processes = Enumerable.Empty<Process>();

                foreach (var p in processesTemp)
                {
                    if (p.Id == excludeId)
                    {
                        p.Dispose();
                        continue;
                    }

                    if (!requireMainWindow
                        || ( !p.HasExited
                        && IntPtr.Zero != p.MainWindowHandle 
                        && (string.IsNullOrEmpty(windowTitle)
                            || -1 < p.MainWindowTitle.IndexOf(windowTitle, StringComparison.OrdinalIgnoreCase))))
                    {
                        processes = processes.Append(p);
                    }
                    else
                    {
                        p.Dispose();
                    }

                    if (includeChildProcesses)
                    {
                        IEnumerable<Process> children = GetChildProcesses(p.Id);
                        try
                        {
                            foreach (var child in children)
                            {
                                if (!processes.Any(b => b.Id == child.Id) && excludeId != child.Id)
                                {
                                    processes = processes.Append(child);
                                }
                                else
                                {
                                    child.Dispose();
                                }
                            }
                        }
                        catch
                        {
                            foreach (var child in children)
                            {
                                child.Dispose();
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    foreach (var p in processes)
                    {
                        p.Dispose();
                    }
                    foreach (var p in processesTemp)
                    {
                        p.Dispose();
                    }
                }
                catch { }
                Log.WriteLine($"Error in finding processes for {appName}: {ex.Message}");
                return Array.Empty<Process>();
            }

            return processes.ToArray();
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
            string appName, 
            bool includeSimilarNames = false,
            bool requireMainWindow = true,
            string? windowTitle = null,
            int excludeId = -1)
        {
            var processes = FindProcesses(appName, includeSimilarNames, requireMainWindow, windowTitle, false, excludeId);
            try
            {
                return processes.FirstOrDefault();
            }
            finally
            {
                if (1 < processes.Length)
                {
                    for(int i = 1; i < processes.Length; i++)
                    {
                        processes[i].Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Validates a process name parameter
        /// </summary>
        /// <param name="processName">The process name to validate</param>
        /// <param name="parameterName">The name of the parameter for exception messages.</param>
        /// <exception cref="ArgumentException">Thrown when the process name is invalid</exception>
        public static void ValidateProcessName(string? processName, string parameterName = "processName")
        {
            if (null == processName)
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be null");
            }

            if (String.Empty == processName)
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(processName))
            {
                throw new ArgumentException($"Argument '{parameterName}' cannot be whitespace only.");
            }

            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            if (-1 < processName.IndexOfAny(invalidChars))
            {
                throw new ArgumentException($"Argument '{parameterName}' contains invalid characters.");
            }
        }

        /// <summary>
        /// Checks if a process with the specified name is currently running
        /// </summary>
        /// <param name="processName">The process name to check</param>
        /// <returns>True if the process is running, false otherwise</returns>
        public static bool IsProcessRunning(string processName)
        {
            ValidateProcessName(processName, nameof(processName));

            try
            {
                Process[] processes = Process.GetProcessesByName(processName);
                try
                {
                    return processes.Any();
                }
                finally
                {
                    foreach (var process in processes)
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error checking if process is running for {processName}: {ex.Message}");
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
            IEnumerable<Process> processes = Enumerable.Empty<Process>();
            try
            {
                /*processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    p.
                    if (GetParentProcessId(p) == parentId)
                    {
                        processes = processes.Append(p);
                    }
                    else
                    {
                        p.Dispose();
                    }
                }
                if ()
                {

                }*/

                return Process.GetProcesses()
                    .Where(p => GetParentProcessId(p) == parentId)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error getting child processes for parent ID {parentId}: {ex.Message}");
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
                Log.WriteLine($"Failed to get parent process ID for {process.ProcessName}: {ex.Message}");
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
        public static bool CloseProcesses(Process[] processes, int timeoutMs = CoreConstants.DefaultActionDelay, bool forceKill = false)
        {
            if (processes is null || processes.Length == 0)
            {
                return true;
            }

            Task<bool>[] closers = [];

            foreach (var process in processes)
            {
                closers = [..closers, Task<bool>.Run(() => 
                {
                    try
                    {
                        if (process.HasExited)
                        {
                            Log.WriteLine($"Process has already exited: {process.ProcessName} (ID: {process.Id})");
                            return true;
                        }

                        Log.WriteLine($"Process {process.ProcessName} main window close request: {(process.CloseMainWindow() ? "Accepted" : "Denied")}");

                        if(process.WaitForExit(CoreConstants.DefaultActionDelay))
                        {
                            Log.WriteLine($"Process kindly closed.: {process.ProcessName} (ID: {process.Id})");
                            return true;
                        }

                        Log.WriteLine($"Process failed to close gracefully: {process.ProcessName} (ID: {process.Id})");

                        if (forceKill)
                        {
                            Log.WriteLine($"Attempting force kill on: {process.ProcessName} (ID: {process.Id})");
                            process.Kill();

                            if (process.HasExited)
                            {
                                Log.WriteLine($"Force killed: {process.ProcessName} (ID: {process.Id})");
                                return true;
                            }

                            Log.WriteLine($"Failed to kill process {process.ProcessName}: {process.Id}");
                            return false;
                        }

                        return false;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"Failed to close process {process.ProcessName}: {ex.Message}");
                        return false;
                    }

                })];
            }

            Task.WaitAll(closers);

            return !closers.Any(a=> !a.Result);
        }
    }
}