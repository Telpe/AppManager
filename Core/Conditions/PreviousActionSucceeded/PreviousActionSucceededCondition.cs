using System;
using System.Collections.Generic;
using System.Text;
using AppManager.Core.Models;

namespace AppManager.Core.Conditions.PreviousActionSucceeded
{
    public class PreviousActionSucceededCondition(ConditionModel model) : BaseCondition(model), IPreviousActionSucceededCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.PreviousActionSuccess;

        public override string Description { get; set; } = "Should be filled with the result of previous action.";

        public bool ActionSucceeded { get; set; } = false;


        public override bool Execute()
        {
            return ActionSucceeded;
        }

        public override ConditionModel ToModel()
        {
            return ToConditionModel<IPreviousActionSucceededCondition>();
        }
    }
}
