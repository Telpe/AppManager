using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class ActionManager
    {
        public ActionManager()
        {
        }

        public IEnumerable<AppActionEnum> GetAvailableActions()
        {
            return Enum.GetValues(typeof(AppActionEnum)).Cast<AppActionEnum>();
        }

        private IAppAction CreateAction(ActionModel model)
        {
            return model.ActionName switch
            {
                AppActionEnum.Launch => new LaunchAction(model),
                AppActionEnum.Close => new CloseAction(model),
                AppActionEnum.Restart => new RestartAction(model),
                AppActionEnum.Focus => new FocusAction(model),
                AppActionEnum.BringToFront => new BringToFrontAction(model),
                AppActionEnum.Minimize => new MinimizeAction(model),
                _ => throw new Exception($"Action not found: {model.ActionName}")
            };
        }

        public bool CanExecuteAction(ActionModel model)
        {

            if (null == model) { return false; }
            try
            {
                var action = CreateAction(model);

                return action?.CanExecute() ?? false;
            }
            catch (Exception e) { return false; }
            
        }

        public bool CanExecuteAction(AppActionEnum actionName, string appName, ActionModel? parameters = null)
        {
            var model = parameters ?? new ActionModel();
            model.ActionName = actionName;
            model.AppName = appName;
            
            return CanExecuteAction(model);
        }

        public Task<bool> ExecuteActionAsync(ActionModel model)
        {
            if (model == null)
            {
                System.Diagnostics.Debug.WriteLine("ActionModel is null");
                return Task.FromResult(false);
            }

            try
            {
                var action = CreateAction(model);
                return action.ExecuteActionAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing action {model.ActionName} on {model.AppName}: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> ExecuteActionAsync(AppActionEnum actionName, string appName, ActionModel? parameters = null)
        {
            var model = parameters ?? new ActionModel();
            model.ActionName = actionName;
            model.AppName = appName;
            
            return ExecuteActionAsync(model);
        }

        public Dictionary<string, Task<bool>> ExecuteMultipleActionsAsync(IEnumerable<ActionModel> actions)
        {
            var results = new Dictionary<string, Task<bool>>();
            
            foreach (var model in actions)
            {
                var key = $"{model.ActionName}_{model.AppName}";
                results[key] = ExecuteActionAsync(model);
            }

            return results;
        }
    }
}