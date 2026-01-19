using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.Settings.UI
{
    public class InputEditEventArgs : EventArgs
    {
        public ActionModel? ActionModel { get; set; }
        public TriggerModel? TriggerModel { get; set; }
        public ConditionModel? ConditionModel { get; set; }

        public InputEditEventArgs(ActionModel? actionModel = null)
        {
            ActionModel = actionModel;
        }

        public InputEditEventArgs(TriggerModel? triggerModel = null)
        {
            TriggerModel = triggerModel;
        }

        public InputEditEventArgs(ConditionModel? conditionModel = null)
        {
            ConditionModel = conditionModel;
        }
    }
}
