using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AppManager.Core.Conditions.PreviousActionSucceeded
{
    [Description("This condition can only succeed when used on an action, that is atleast second in a list.")]
    [ConditionCategory("Action Management")]
    public interface IPreviousActionSucceededCondition
    {
    }
}
