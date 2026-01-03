using AppManager.Core.Actions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AppManager.Core.Triggers
{
    internal class SystemEventTrigger : BaseTrigger, ISystemEventTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.SystemEvent;

        private EventLogWatcher EventWatcherValue;
        private CancellationTokenSource CancellationTokenSourceValue;

        public string? EventName { get; set; }
        public string? EventSource { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }

        public SystemEventTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors Windows system events (login, logout, lock, unlock, etc.)";
            
            EventName = model.EventName;
            EventSource = model.EventSource;
            CustomProperties = model.CustomProperties ?? [];
        }

        protected override bool CanStartTrigger()
        {
            return !string.IsNullOrEmpty(EventName);
        }

        public override void Start()
        {
            if (!CanStart()) { return; }

            CancellationTokenSourceValue = new CancellationTokenSource();

            // Monitor Windows security events (logon/logoff events)
            string logName = "Security";
            string queryString = "*[System[EventID=4624 or EventID=4634 or EventID=4647]]"; // Logon/Logoff events
                    
            // For workstation lock/unlock events
            if (null != EventName && (EventName.ToLower().Contains("lock") || EventName.ToLower().Contains("unlock")))
            {
                logName = "System";
                queryString = "*[System[EventID=4800 or EventID=4801]]"; // Lock/Unlock events
            }

            var query = new EventLogQuery(logName, PathType.LogName, queryString);
            EventWatcherValue = new EventLogWatcher(query);
            EventWatcherValue.EventRecordWritten += OnSystemEvent;
                    
            EventWatcherValue.Enabled = true;

            System.Diagnostics.Debug.WriteLine($"System event trigger '{Name}' started.");
        }

        public override void Stop()
        {
            try
            {
                CancellationTokenSourceValue?.Cancel();
                if (null != EventWatcherValue)
                {
                    EventWatcherValue.Enabled = false;
                    EventWatcherValue.Dispose();
                }
                
                Debug.WriteLine($"System event trigger '{Name}' stopped");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping system event trigger '{Name}': {ex.Message}");
            }
        }

        private void OnSystemEvent(object? sender, EventRecordWrittenEventArgs e)
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
                        TriggerActivated();
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
            if (string.IsNullOrEmpty(eventDescription) || string.IsNullOrEmpty(EventName))
                return false;

            return eventDescription.Contains(EventName, StringComparison.OrdinalIgnoreCase);
        }

        public override void Dispose()
        {
            base.Dispose();
            CancellationTokenSourceValue?.Dispose();
        }

        public override TriggerModel ToModel()
        {
            TriggerModel model = base.ToModel();
            model.EventName = EventName;
            model.EventSource = EventSource;
            model.CustomProperties = CustomProperties;

            return model;
        }
    }
}