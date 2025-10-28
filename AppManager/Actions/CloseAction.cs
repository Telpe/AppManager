using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class CloseAction : IAppAction
    {
        public AppActionEnum ActionName => AppActionEnum.Close;
        public string Description => "Closes an application gracefully";

        public bool CanExecute(string appName, ActionParameters parameters = null)
        {
            return !string.IsNullOrEmpty(appName) && GetProcesses(appName, parameters).Any();
        }

        public async Task<bool> ExecuteAsync(string appName, ActionParameters parameters = null)
        {
            try
            {
                var processes = GetProcesses(appName, parameters);
                
                if (!processes.Any())
                {
                    Debug.WriteLine($"No processes found for: {appName}");
                    return false;
                }

                bool allClosed = true;
                
                foreach (var process in processes)
                {
                    try
                    {
                        if (parameters?.ForceOperation == true)
                        {
                            process.Kill();
                            Debug.WriteLine($"Force killed process: {process.ProcessName} (ID: {process.Id})");
                        }
                        else
                        {
                            if (!process.CloseMainWindow())
                            {
                                // If graceful close fails, wait a bit then force close
                                await Task.Delay(2500);
                                if (!process.HasExited)
                                {
                                    process.Kill();
                                    Debug.WriteLine($"Force killed process after graceful close failed: {process.ProcessName} (ID: {process.Id})");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Gracefully closed process: {process.ProcessName} (ID: {process.Id})");
                            }
                        }

                        // Wait for process to exit
                        if (!process.WaitForExit(parameters?.TimeoutMs ?? 5000))
                        {
                            Debug.WriteLine($"Process {process.ProcessName} did not exit within timeout");
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to close {appName}: {ex.Message}");
                return false;
            }
        }

        private Process[] GetProcesses(string appName, ActionParameters parameters)
        {
            try
            {
                Process[] processes;
                
                if (parameters?.IncludeSimilarNames == true)
                {
                    // Get all processes and filter by name pattern
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(appName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }
                else
                {
                    // Get exact match processes
                    processes = Process.GetProcessesByName(appName);
                }

                if (parameters?.IncludeChildProcesses == true)
                {
                    // Add child processes
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
                Debug.WriteLine($"Error getting processes for {appName}: {ex.Message}");
                return new Process[0];
            }
        }

        private Process[] GetChildProcesses(int parentId)
        {
            return Process.GetProcesses()
                .Where(p => GetParentProcessId(p) == parentId)
                .ToArray();
        }

        private int GetParentProcessId(Process process)
        {
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
                Debug.WriteLine($"Failed to get parent process ID: {ex.Message}");
            }
            return -1;
        }
    }
}