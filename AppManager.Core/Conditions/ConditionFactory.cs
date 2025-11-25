using AppManager.Core.Models;
using System;
using System.Collections.Generic;

namespace AppManager.Core.Conditions
{
    public static class ConditionFactory
    {
        private static readonly Dictionary<ConditionTypeEnum, Func<ConditionModel, ICondition>> _conditionFactories = new Dictionary<ConditionTypeEnum, Func<ConditionModel, ICondition>>
        {
            { ConditionTypeEnum.ProcessRunning, (model) => new ProcessRunningCondition(){ Model=model } },
            { ConditionTypeEnum.ProcessNotRunning, (model) => new ProcessNotRunningCondition(){ Model=model } },
            { ConditionTypeEnum.FileExists, (model) => new FileExistsCondition(){ Model=model } },
            { ConditionTypeEnum.FileNotExists, (model) => new FileNotExistsCondition(){ Model=model } }
        };

        public static ICondition CreateCondition(ConditionModel model)
        {
            model ??= new ConditionModel { ConditionType = ConditionTypeEnum.None };

            if (_conditionFactories.TryGetValue(model.ConditionType, out var factory))
            {
                return factory(model);
            }

            throw new NotSupportedException($"Condition type {model.ConditionType} is not supported");
        }

        public static IEnumerable<ConditionTypeEnum> GetSupportedConditionTypes()
        {
            return _conditionFactories.Keys;
        }
    }
}