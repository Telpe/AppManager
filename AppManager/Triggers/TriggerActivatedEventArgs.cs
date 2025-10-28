using System;
using AppManager.Actions;

namespace AppManager.Triggers
{
    public class TriggerActivatedEventArgs : EventArgs
    {
        public string TriggerName { get; set; }
        public TriggerTypeEnum TriggerType { get; set; }
        public string TargetAppName { get; set; }
        public AppActionEnum ActionToExecute { get; set; }
        public ActionParameters ActionParameters { get; set; }
        public DateTime ActivatedAt { get; set; } = DateTime.Now;
        public object TriggerData { get; set; }
    }
}