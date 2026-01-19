using System;
using System.Collections.Generic;
using System.Text;
using AppManager.Core.Models;

namespace AppManager.Core.Conditions
{
    public class PreviousActionSuccessCondition(ConditionModel model) : BaseCondition(model), ICondition
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
            return new ConditionModel
            {
                ConditionType = ConditionType,
                IsNot = IsNot
            };
        }
    }
}
