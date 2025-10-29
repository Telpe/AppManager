using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class ActionManager
    {
        private readonly Dictionary<AppActionEnum, IAppAction> _Actions;

        public ActionManager()
        {
            _Actions = new Dictionary<AppActionEnum, IAppAction>() 
            {
                { AppActionEnum.Launch, new LaunchAction() },
                { AppActionEnum.Close, new CloseAction() },
                { AppActionEnum.Restart, new RestartAction() },
                { AppActionEnum.Focus, new FocusAction() },
                { AppActionEnum.BringToFront, new BringToFrontAction() },
                { AppActionEnum.Minimize, new MinimizeAction() }
            };
        }

        public IEnumerable<AppActionEnum> GetAvailableActions()
        {
            return Enum.GetValues(typeof(AppActionEnum)).Cast<AppActionEnum>();
        }

        public IAppAction GetAction(AppActionEnum actionName)
        {
            if(!_Actions.TryGetValue(actionName, out IAppAction action))
            {
                throw new Exception($"Action not found: {actionName}");
            }
            return action;
        }

        public bool CanExecuteAction(ActionModel model)
        {
            if (model == null)
                return false;
                
            var action = GetAction(model.ActionName);
            return action?.CanExecute(model) ?? false;
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

            var action = GetAction(model.ActionName);
            if (action == null)
            {
                System.Diagnostics.Debug.WriteLine($"Action not found: {model.ActionName}");
                return false;
            }

            try
            {
                return await action.ExecuteAsync(model);
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