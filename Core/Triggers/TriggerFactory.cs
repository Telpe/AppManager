using AppManager.Core.Triggers;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.Core.Triggers
{
    public static class TriggerFactory
    {
        private static readonly Dictionary<TriggerTypeEnum, Func<TriggerModel, ITrigger>> TriggerFactories = new()
        {
            {TriggerTypeEnum.Keybind, (model) => new KeybindTrigger(model)},
            {TriggerTypeEnum.Button, (model) => new ButtonTrigger(model)},
            {TriggerTypeEnum.AppLaunch, (model) => new AppLaunchTrigger(model)},
            {TriggerTypeEnum.AppClose, (model) => new AppCloseTrigger(model)},
            {TriggerTypeEnum.SystemEvent, (model) => new SystemEventTrigger(model)},
            {TriggerTypeEnum.NetworkPort, (model) => new NetworkPortTrigger(model)}
        };

        public static ITrigger CreateTrigger(TriggerModel model)
        {
            if (TriggerFactories.TryGetValue(model.TriggerType, out var factory))
            {
                return factory(model);
            }

            throw new NotSupportedException($"Trigger type {model.TriggerType} is not supported");
        }

        public static bool IsTriggerTypeSupported(TriggerTypeEnum conditionType)
        {
            return TriggerFactories.ContainsKey(conditionType);
        }

        public static void SetTriggerFactory(TriggerTypeEnum conditionType, Func<TriggerModel, ITrigger> factoryFunc)
        {
            TriggerFactories[conditionType] = factoryFunc;
        }

        public static IEnumerable<TriggerTypeEnum> GetSupportedTriggerTypes()
        {
            return TriggerFactories.Keys;
        }

    }
}
