using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Actions
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

        private IAppAction CreateAction(AppActionEnum actionName, ActionModel model)
        {
            return actionName switch
            {
                AppActionEnum.Launch => new LaunchAction(model),
                AppActionEnum.Close => new CloseAction(model),
                AppActionEnum.Restart => new RestartAction(model),
                AppActionEnum.Focus => new FocusAction(model),
                AppActionEnum.BringToFront => new BringToFrontAction(model),
                AppActionEnum.Minimize => new MinimizeAction(model),
                _ => throw new Exception($"Action not found: {actionName}")
            };
        }

        public bool CanExecuteAction(ActionModel model)
        {
            if (model == null)
                return false;
                
            var action = CreateAction(model.ActionName, model);
            return action?.CanExecute() ?? false;
        }

        public bool CanExecuteAction(AppActionEnum actionName, string appName, ActionModel parameters = null)
        {
            var model = parameters ?? new ActionModel();
            model.ActionName = actionName;
            model.AppName = appName;
            
            return CanExecuteAction(model);
        }

        public async Task<bool> ExecuteActionAsync(ActionModel model)
        {
            if (model == null)
            {
                System.Diagnostics.Debug.WriteLine("ActionModel is null");
                return false;
            }

            try
            {
                var action = CreateAction(model.ActionName, model);
                return await action.ExecuteAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing action {model.ActionName} on {model.AppName}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExecuteActionAsync(AppActionEnum actionName, string appName, ActionModel parameters = null)
        {
            var model = parameters ?? new ActionModel();
            model.ActionName = actionName;
            model.AppName = appName;
            
            return await ExecuteActionAsync(model);
        }

        public async Task<Dictionary<string, bool>> ExecuteMultipleActionsAsync(IEnumerable<ActionModel> actions)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (var model in actions)
            {
                var key = $"{model.ActionName}_{model.AppName}";
                results[key] = await ExecuteActionAsync(model);
            }

            return results;
        }

        public async Task<Dictionary<string, bool>> ExecuteMultipleActionsAsync(
            IEnumerable<(AppActionEnum actionName, string appName, ActionModel parameters)> actions)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (var (actionName, appName, parameters) in actions)
            {
                var key = $"{actionName}_{appName}";
                results[key] = await ExecuteActionAsync(actionName, appName, parameters);
            }

            return results;
        }
    }
}