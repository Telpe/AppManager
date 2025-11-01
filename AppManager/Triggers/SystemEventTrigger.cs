using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using AppManager.Actions;

namespace AppManager.Triggers
{
    internal class SystemEventTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.SystemEvent;
        public override string Description => "Monitors Windows system events (login, logout, lock, unlock, etc.)";

        private TriggerModel _parameters;
        private EventLogWatcher _eventWatcher;
        private CancellationTokenSource _cancellationTokenSource;

        public SystemEventTrigger(string name = null) : base(name)
        {
        }

        public override bool CanStart(TriggerModel parameters = null)
        {
            return !string.IsNullOrEmpty(parameters?.EventName);
        }

        public override async Task<bool> StartAsync(TriggerModel parameters = null)
        {
            if (IsActive || parameters == null)
                return false;

            try
            {
                _parameters = parameters;
                _cancellationTokenSource = new CancellationTokenSource();

                // Monitor Windows security events (logon/logoff events)
                string logName = "Security";
                string queryString = "*[System[EventID=4624 or EventID=4634 or EventID=4647]]"; // Logon/Logoff events
                
                // For workstation lock/unlock events
                if (parameters.EventName.ToLower().Contains("lock") || parameters.EventName.ToLower().Contains("unlock"))
                {
                    logName = "System";
                    queryString = "*[System[EventID=4800 or EventID=4801]]"; // Lock/Unlock events
                }

                var query = new EventLogQuery(logName, PathType.LogName, queryString);
                _eventWatcher = new EventLogWatcher(query);
                _eventWatcher.EventRecordWritten += OnSystemEvent;
                
                _eventWatcher.Enabled = true;
                IsActive = true;
                
                Debug.WriteLine($"System event trigger '{Name}' started monitoring for '{parameters.EventName}'");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting system event trigger '{Name}': {ex.Message}");
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
                if (null != _eventWatcher)
                {
                    _eventWatcher.Enabled = false;
                    _eventWatcher.Dispose();
                }
                
                IsActive = false;
                Debug.WriteLine($"System event trigger '{Name}' stopped");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping system event trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        private void OnSystemEvent(object sender, EventRecordWrittenEventArgs e)
        {
            try
            {
                if (e.EventRecord != null)
                {
                    var eventId = e.EventRecord.Id;
                    var eventDescription = GetEventDescription(eventId);

                    if (IsTargetEvent(eventDescription))
                    {
                        Debug.WriteLine($"System event trigger '{Name}' detected event: {eventDescription}");
                        
                        // Trigger the configured action
                        OnTriggerActivated("system", AppActionEnum.Launch, null, new { EventId = eventId, Description = eventDescription });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing system event in trigger '{Name}': {ex.Message}");
            }
        }

        private string GetEventDescription(int eventId)
        {
            return eventId switch
            {
                4624 => "User Logon",
                4634 => "User Logoff", 
                4647 => "User Initiated Logoff",
                4800 => "Workstation Locked",
                4801 => "Workstation Unlocked",
                _ => $"Unknown Event {eventId}"
            };
        }

        private bool IsTargetEvent(string eventDescription)
        {
            if (string.IsNullOrEmpty(eventDescription) || string.IsNullOrEmpty(_parameters.EventName))
                return false;

            return eventDescription.Contains(_parameters.EventName, StringComparison.OrdinalIgnoreCase);
        }

        public override void Dispose()
        {
            _ = StopAsync();
            _cancellationTokenSource?.Dispose();
            base.Dispose();
        }
    }
}