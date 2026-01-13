using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public static class ActionManager
    {
        public static IEnumerable<AppActionTypeEnum> GetAvailableActions()
        {
            return Enum.GetValues(typeof(AppActionTypeEnum)).Cast<AppActionTypeEnum>();
        }

        public static IAction CreateAction(ActionModel model, Process? specificTarget = null)
        {
            return model.ActionType switch
            {
                AppActionTypeEnum.Launch => new LaunchAction(model),
                AppActionTypeEnum.Close => new CloseAction(model),
                AppActionTypeEnum.Restart => new RestartAction(model),
                AppActionTypeEnum.Focus => new FocusAction(model),
                AppActionTypeEnum.BringToFront => null == specificTarget ? new BringToFrontAction(model) : new BringToFrontAction(model, specificTarget),
                AppActionTypeEnum.Minimize => new MinimizeAction(model),
                _ => throw new Exception($"Action not found: {model.ActionType}")
            };
        }

        public static bool CanExecuteAction(ActionModel model)
        {
            try
            {
                var action = CreateAction(model);

                return action.CanExecute();
            }
            catch { return false; }
            
        }

        public static bool CanExecuteAction(AppActionTypeEnum actionName, string appName)
        {
            ActionModel model = new()
            {
                ActionType = actionName,
                AppName = appName
            };

            return CanExecuteAction(model);
        }

        public static void ExecuteAction(ActionModel model)
        {
            try
            {
                CreateAction(model).Execute();
            }
            catch (Exception e)
            {
                throw new Exception($"Error executing action {model.ActionType} on {model.AppName}.", e);
            }
            
        }

        public static void ExecuteAction(AppActionTypeEnum actionName, string appName)
        {
            ActionModel model = new()
            {
                ActionType = actionName,
                AppName = appName
            };

            ExecuteAction(model);
        }

        public static void ExecuteMultipleActions(IEnumerable<ActionModel> actions)
        {
            foreach (ActionModel model in actions)
            {
                ExecuteAction(model);
            }
        }
    }
}