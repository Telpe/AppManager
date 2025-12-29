using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;
using System.Collections.Generic;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal class AppLaunchTrigger : BaseTrigger, IAppLaunchTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.AppLaunch;

        private ManagementEventWatcher ProcessWatcherValue = new ManagementEventWatcher();
        private CancellationTokenSource CancellationTokenSourceValue = new CancellationTokenSource();

        public string? ProcessName { get; set; }
        public string? ExecutablePath { get; set; }
        public bool? MonitorChildProcesses { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        public AppLaunchTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors for application launch events";
            
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

                    // Use WMI to monitor process creation events (most reliable method)
                    var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                    ProcessWatcherValue = new ManagementEventWatcher(query);
                    ProcessWatcherValue.EventArrived += OnProcessStarted;
                    
                    ProcessWatcherValue.Start();
                    
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
                CancellationTokenSourceValue.Cancel();
                
                ProcessWatcherValue.Stop();
                ProcessWatcherValue.Dispose();
                
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
                    TriggerActivated();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing app launch event in trigger '{Name}': {ex.Message}");
            }
        }

        private bool IsTargetProcess(string? processName)
        {
            if (string.IsNullOrEmpty(processName)){ return false; }

            bool isTargetProcess = false;

            // Check by process name
            if (!string.IsNullOrEmpty(ProcessName))
            {
                isTargetProcess = processName.Equals(ProcessName, StringComparison.OrdinalIgnoreCase) || processName.StartsWith(ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            // Check by executable path if provided
            if (!isTargetProcess && !string.IsNullOrEmpty(ExecutablePath))
            {
                isTargetProcess = processName.Contains(Path.GetFileNameWithoutExtension(ExecutablePath));
            }

            return isTargetProcess;
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