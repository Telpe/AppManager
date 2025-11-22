using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Core.Actions;

namespace AppManager.Core.Triggers
{
    internal class AppCloseTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.AppClose;
        public override string Description => "Monitors for application close/exit events";

        private TriggerModel _parameters;
        private ManagementEventWatcher _processWatcher;
        private CancellationTokenSource _cancellationTokenSource;

        public AppCloseTrigger(string name = null) : base(name)
        {
        }

        public override bool CanStart(TriggerModel parameters = null)
        {
            return !string.IsNullOrEmpty(parameters?.ProcessName) || !string.IsNullOrEmpty(parameters?.ExecutablePath);
        }

        public override async Task<bool> StartAsync(TriggerModel parameters = null)
        {
            if (IsActive || parameters == null)
                return false;

            try
            {
                _parameters = parameters;
                _cancellationTokenSource = new CancellationTokenSource();

                // Use WMI to monitor process termination events
                var query = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
                _processWatcher = new ManagementEventWatcher(query);
                _processWatcher.EventArrived += OnProcessStopped;
                
                _processWatcher.Start();
                IsActive = true;
                
                Debug.WriteLine($"App close trigger '{Name}' started monitoring for '{parameters.ProcessName}'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting app close trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        public override async Task<bool> StopAsync()
        {
            if (!IsActive)
                return true;

            try
            {
                _cancellationTokenSource?.Cancel();
                _processWatcher?.Stop();
                _processWatcher?.Dispose();
                
                IsActive = false;
                Debug.WriteLine($"App close trigger '{Name}' stopped");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping app close trigger '{Name}': {ex.Message}");
                return false;
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
                    OnTriggerActivated(_parameters.ProcessName, AppActionEnum.Launch);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing app close event in trigger '{Name}': {ex.Message}");
            }
        }

        private bool IsTargetProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return false;

            // Check by process name
            if (!string.IsNullOrEmpty(_parameters.ProcessName))
            {
                return processName.Equals(_parameters.ProcessName, StringComparison.OrdinalIgnoreCase) ||
                       processName.StartsWith(_parameters.ProcessName, StringComparison.OrdinalIgnoreCase);
            }

            // Check by executable path if provided
            if (!string.IsNullOrEmpty(_parameters.ExecutablePath))
            {
                return processName.Contains(System.IO.Path.GetFileNameWithoutExtension(_parameters.ExecutablePath));
            }

            return false;
        }

        public override void Dispose()
        {
            _ = StopAsync();
            _cancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }
}