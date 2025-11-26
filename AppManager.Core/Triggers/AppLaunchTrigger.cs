using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using System.Collections.Generic;

namespace AppManager.Core.Triggers
{
    internal class AppLaunchTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.AppLaunch;

        private ManagementEventWatcher ProcessWatcherStored;
        private CancellationTokenSource CancellationTokenSourceStored;

        public string? ProcessName { get; set; }
        public string? ExecutablePath { get; set; }
        public bool? MonitorChildProcesses { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        public AppLaunchTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors for application launch events";
            
            ProcessName = model.ProcessName;
            ExecutablePath = model.ExecutablePath;
            MonitorChildProcesses = model.MonitorChildProcesses;
            CustomProperties = model.CustomProperties ?? [];
        }

        public override bool CanStart()
        {
            return !string.IsNullOrEmpty(ProcessName) || !string.IsNullOrEmpty(ExecutablePath);
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (!IsActive) { return false; }

                try
                {
                    CancellationTokenSourceStored = new CancellationTokenSource();

                    // Use WMI to monitor process creation events (most reliable method)
                    var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                    ProcessWatcherStored = new ManagementEventWatcher(query);
                    ProcessWatcherStored.EventArrived += OnProcessStarted;
                    
                    ProcessWatcherStored.Start();
                    
                    Debug.WriteLine($"App launch trigger '{Name}' started monitoring for '{ProcessName}'");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error starting app launch trigger '{Name}': {ex.Message}");
                    return false;
                }
            });
        }

        public override void Stop()
        {
            try
            {
                CancellationTokenSourceStored?.Cancel();
                if (ProcessWatcherStored != null)
                {
                    ProcessWatcherStored.Stop();
                    ProcessWatcherStored.Dispose();
                }
                
                Debug.WriteLine($"App launch trigger '{Name}' stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping app launch trigger '{Name}': {ex.Message}");
            }
        }

        private void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                var processName = e.NewEvent["ProcessName"]?.ToString();
                var processId = Convert.ToInt32(e.NewEvent["ProcessID"]);

                if (IsTargetProcess(processName))
                {
                    Debug.WriteLine($"App launch trigger '{Name}' detected launch of '{processName}' (PID: {processId})");
                    
                    // Trigger the configured action
                    OnTriggerActivated(ProcessName ?? "target_app", AppActionTypeEnum.Focus, null, new { ProcessName = processName, ProcessId = processId });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing app launch event in trigger '{Name}': {ex.Message}");
            }
        }

        private bool IsTargetProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return false;

            // Check by process name
            if (!string.IsNullOrEmpty(ProcessName))
            {
                return processName.Equals(ProcessName, StringComparison.OrdinalIgnoreCase) ||
                       processName.StartsWith(ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            // Check by executable path if provided
            if (!string.IsNullOrEmpty(ExecutablePath))
            {
                return processName.Contains(System.IO.Path.GetFileNameWithoutExtension(ExecutablePath));
            }

            return false;
        }

        public override void Dispose()
        {
            Stop();
            CancellationTokenSourceStored?.Dispose();
            base.Dispose();
        }

        public override TriggerModel ToModel()
        {
            return new TriggerModel
            {
                TriggerType = TriggerType,
                IsActive = IsActive,
                ProcessName = ProcessName,
                ExecutablePath = ExecutablePath,
                MonitorChildProcesses = MonitorChildProcesses,
                CustomProperties = CustomProperties
            };
        }
    }
}