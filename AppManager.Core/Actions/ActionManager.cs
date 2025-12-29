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

        public static Task<bool> ExecuteActionAsync(ActionModel model)
        {
            if (model == null)
            {
                System.Diagnostics.Debug.WriteLine("ActionModel is null");
                return Task.FromResult(false);
            }

            try
            {
                var action = CreateAction(model);
                return action.ExecuteAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing action {model.ActionType} on {model.AppName}: {ex.Message}");
                return Task.FromResult(false);
            }
            
        }

        public static Task<bool> ExecuteActionAsync(AppActionTypeEnum actionName, string appName)
        {
            ActionModel model = new()
            {
                ActionType = actionName,
                AppName = appName
            };

            return ExecuteActionAsync(model);
        }

        public static Task<bool>[] ExecuteMultipleActionsAsync(IEnumerable<ActionModel> actions)
        {
            int n = actions.Count();
            if (n == 0) { return Array.Empty<Task<bool>>(); }
            var results = new Task<bool>[n];

            int i = 0;
            foreach (ActionModel model in actions)
            {
                results[i] = ExecuteActionAsync(model);
                i++;
            }

            return results;
        }
    }
}