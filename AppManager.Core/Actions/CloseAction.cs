using AppManager.Core.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class CloseAction : BaseAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Close;
        public override string Description => "Closes an application gracefully";

        public int? TimeoutMs { get; private set; }
        public bool? ForceOperation { get; private set; }
        public bool? IncludeSimilarNames { get; private set; }
        public bool? IncludeChildProcesses { get; private set; }

        public CloseAction(ActionModel model) : base(model)
        {
            AppName = model.AppName;
            IncludeChildProcesses = model.IncludeChildProcesses;
            IncludeSimilarNames = model.IncludeSimilarNames;
            TimeoutMs = model.TimeoutMs;
            ForceOperation = model.ForceOperation;
        }

        protected override Task<bool> ExecuteAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    var processes = GetProcesses();
                
                    if (!processes.Any())
                    {
                        Debug.WriteLine($"No processes found for: {AppName}");
                        return false;
                    }

                    bool allClosed = true;
                
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.CloseMainWindow();

                            if (!process.WaitForExit(TimeoutMs??1000))
                            {
                                Debug.WriteLine($"Process failed to close gracefully: {process.ProcessName} (ID: {process.Id})");
                                if (ForceOperation??false)
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
                    Debug.WriteLine($"Failed to close {AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        private Process[] GetProcesses()
        {
            try
            {
                Process[] processes;
                
                if (IncludeSimilarNames??false)
                {
                    // Get all processes and filter by name pattern
                    processes = Process.GetProcesses()
                        .Where(p => p.ProcessName.IndexOf(AppName, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToArray();
                }
                else
                {
                    // Get exact match processes
                    processes = Process.GetProcessesByName(AppName);
                }

                if (IncludeChildProcesses ?? false)
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
                Debug.WriteLine($"Error getting processes for {AppName}: {ex.Message}");
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

        public override ActionModel ToModel()
        {
            return new ActionModel()
            {
                AppName = AppName,
                ActionType = ActionType,
                IncludeChildProcesses = IncludeChildProcesses,
                IncludeSimilarNames = IncludeSimilarNames,
                TimeoutMs = TimeoutMs,
                ForceOperation = ForceOperation,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}