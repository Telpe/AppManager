using System;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    public class TriggerActivatedEventArgs : EventArgs
    {
        public string TriggerName { get; set; }
        public TriggerTypeEnum TriggerType { get; set; }
        public string TargetAppName { get; set; }
        public AppActionEnum ActionToExecute { get; set; }
        public ActionModel ActionParameters { get; set; }
        public DateTime ActivatedAt { get; set; } = DateTime.Now;
        public object TriggerData { get; set; }
    }
}