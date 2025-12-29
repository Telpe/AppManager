using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using System.Collections.Generic;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal class AppCloseTrigger : BaseTrigger, IAppCloseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.AppClose;

        private ManagementEventWatcher? ProcessWatcherValue;
        private CancellationTokenSource? CancellationTokenSourceValue;

        public string? ProcessName { get; set; }
        public string? ExecutablePath { get; set; }
        public bool? MonitorChildProcesses { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        public AppCloseTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors for application close/exit events";
            
            ProcessName = model.ProcessName;
            ExecutablePath = model.ExecutablePath;
            MonitorChildProcesses = model.MonitorChildProcesses;
            CustomProperties = model.CustomProperties ?? [];
        }

        protected override bool CanStartTrigger()
        {
            return !string.IsNullOrEmpty(ProcessName) || !string.IsNullOrEmpty(ExecutablePath);
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (Inactive) { return false; }

                try
                {
                    CancellationTokenSourceValue = new CancellationTokenSource();

                    // Use WMI to monitor process termination events
                    var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
                    ProcessWatcherValue = new ManagementEventWatcher(query);
                    ProcessWatcherValue.EventArrived += OnProcessStopped;
                    
                    ProcessWatcherValue.Start();
                    
                    Debug.WriteLine($"App close trigger '{Name}' started monitoring for '{ProcessName}'");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting app close trigger '{Name}': {ex.Message}");
                    return false;
                }
            });
        }

        public override void Stop()
        {
            try
            {
                CancellationTokenSourceValue?.Cancel();
                if (ProcessWatcherValue != null)
                {
                    ProcessWatcherValue.Stop();
                    ProcessWatcherValue.Dispose();
                }
                
                Debug.WriteLine($"App close trigger '{Name}' stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping app close trigger '{Name}': {ex.Message}");
            }
        }

        private void OnProcessStopped(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processName = e.NewEvent["ProcessName"]?.ToString();
                var processId = Convert.ToInt32(e.NewEvent["ProcessID"]);

                if (IsTargetProcess(processName))
                {
                    Debug.WriteLine($"App close trigger '{Name}' detected close of '{processName}' (PID: {processId})");
                    
                    // Trigger the configured action
                    TriggerActivated();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing app close event in trigger '{Name}': {ex.Message}");
            }
        }

        private bool IsTargetProcess(string? processName)
        {
            if (string.IsNullOrEmpty(processName)){ return false; }
            

            // Check by process name
            if (!string.IsNullOrEmpty(ProcessName))
            {
                return processName.Equals(ProcessName, StringComparison.OrdinalIgnoreCase) ||
                       processName.StartsWith(ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            // Check by executable path if provided
            if (!string.IsNullOrEmpty(ExecutablePath))
            {
                return processName.Contains(Path.GetFileNameWithoutExtension(ExecutablePath));
            }

            return false;
        }

        public override void Dispose()
        {
            Stop();
            CancellationTokenSourceValue?.Dispose();
            base.Dispose();
        }

        public override TriggerModel ToModel()
        {
            TriggerModel model = base.ToModel();
            model.ProcessName = ProcessName;
            model.ExecutablePath = ExecutablePath;
            model.MonitorChildProcesses = MonitorChildProcesses;
            model.CustomProperties = CustomProperties;

            return model;
        }
    }
}