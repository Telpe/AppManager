using AppManager.Core.Actions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public static class ActionFactory
    {
        private static readonly Dictionary<AppActionTypeEnum, Func<ActionModel, IAction>> ActionFactories = new Dictionary<AppActionTypeEnum, Func<ActionModel, IAction>>
        {
            {AppActionTypeEnum.Launch, (model) => new LaunchAction(model)},
            {AppActionTypeEnum.Close, (model) => new CloseAction(model)},
            {AppActionTypeEnum.Restart, (model) => new RestartAction(model)},
            {AppActionTypeEnum.Focus, (model) => new FocusAction(model)},
            {AppActionTypeEnum.BringToFront, (model) => new BringToFrontAction(model)},
            {AppActionTypeEnum.Minimize, (model) => new MinimizeAction(model)}
        };

        public static IAction CreateAction(ActionModel model)
        {
            if (ActionFactories.TryGetValue(model.ActionType, out var factory))
            {
                return factory(model);
            }

            throw new NotSupportedException($"Action type {model.ActionType} is not supported");
        }

        public static bool IsActionTypeSupported(AppActionTypeEnum conditionType)
        {
            return ActionFactories.ContainsKey(conditionType);
        }

        public static void SetActionFactory(AppActionTypeEnum conditionType, Func<ActionModel, IAction> factoryFunc)
        {
            ActionFactories[conditionType] = factoryFunc;
        }

        public static IEnumerable<AppActionTypeEnum> GetSupportedActionTypes()
        {
            return ActionFactories.Keys;
        }

    }
}