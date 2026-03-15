using AppManager.Core.Actions;
using AppManager.Core.Actions.BringToFront;
using AppManager.Core.Actions.Close;
using AppManager.Core.Actions.Focus;
using AppManager.Core.Actions.Launch;
using AppManager.Core.Actions.Minimize;
using AppManager.Core.Actions.Restart;
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
        private static readonly Dictionary<ActionTypeEnum, Func<ActionModel, IAction>> ActionFactories = new Dictionary<ActionTypeEnum, Func<ActionModel, IAction>>
        {
            {ActionTypeEnum.Launch, (model) => new LaunchAction(model)},
            {ActionTypeEnum.Close, (model) => new CloseAction(model)},
            {ActionTypeEnum.Restart, (model) => new RestartAction(model)},
            {ActionTypeEnum.Focus, (model) => new FocusAction(model)},
            {ActionTypeEnum.BringToFront, (model) => new BringToFrontAction(model)},
            {ActionTypeEnum.Minimize, (model) => new MinimizeAction(model)}
        };

        public static IAction CreateAction(ActionModel model)
        {
            if (ActionFactories.TryGetValue(model.ActionType, out var factory))
            {
                return factory(model);
            }

            throw new NotSupportedException($"Action type {model.ActionType} is not supported");
        }

        public static bool IsActionTypeSupported(ActionTypeEnum conditionType)
        {
            return ActionFactories.ContainsKey(conditionType);
        }

        public static void SetActionFactory(ActionTypeEnum conditionType, Func<ActionModel, IAction> factoryFunc)
        {
            ActionFactories[conditionType] = factoryFunc;
        }

        public static IEnumerable<ActionTypeEnum> GetSupportedActionTypes()
        {
            return ActionFactories.Keys;
        }

    }
}