using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class CloseAction : BaseAction
    {
        public override string Description => "Closes an application gracefully";

        public CloseAction(ActionModel model) : base(model) { }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    var processes = GetProcesses(_Model);
                
                    if (!processes.Any())
                    {
                        Debug.WriteLine($"No processes found for: {_Model.AppName}");
                        return false;
                    }

                    bool allClosed = true;
                
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.CloseMainWindow();

                            if (!process.WaitForExit(_Model.TimeoutMs))
                            {
                                Debug.WriteLine($"Process failed to close gracefully: {process.ProcessName} (ID: {process.Id})");
                                if (_Model.ForceOperation)
                                {
                                    process.Kill();
                                    Debug.WriteLine($"Force closing: {process.ProcessName} (ID: {process.Id})");
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to close {_Model.AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        private Process[] GetProcesses(ActionModel model)
        {
            try
            {
                Process[] processes;
                
                if (model.IncludeSimilarNames)
                {
                    // Get all processes and filter by name pattern
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(model.AppName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }
                else
                {
                    // Get exact match processes
                    processes = Process.GetProcessesByName(model.AppName);
                }

                if (model.IncludeChildProcesses)
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
                Debug.WriteLine($"Error getting processes for {model.AppName}: {ex.Message}");
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