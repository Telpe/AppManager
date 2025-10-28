using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class ActionManager
    {
        private readonly Dictionary<AppActionEnum, IAppAction> _actions;

        public ActionManager()
        {
            var actions = new IAppAction[]
            {
                new LaunchAction(),
                new CloseAction(),
                new RestartAction(),
                new FocusAction(),
                new BringToFrontAction(),
                new MinimizeAction()
            };

            _actions = new Dictionary<AppActionEnum, IAppAction>();
            
            foreach (var action in actions)
            {
                _actions[action.ActionName] = action;
            }
        }

        public IEnumerable<AppActionEnum> GetAvailableActions()
        {
            return _actions.Keys;
        }

        public IEnumerable<string> GetAvailableActionNames()
        {
            return _actions.Keys.Select(a => a.ToString());
        }

        public IAppAction GetAction(AppActionEnum actionName)
        {
            _actions.TryGetValue(actionName, out IAppAction action);
            return action;
        }

        public IAppAction GetAction(string actionName)
        {
            if (Enum.TryParse<AppActionEnum>(actionName, true, out AppActionEnum actionEnum))
            {
                return GetAction(actionEnum);
            }
            return null;
        }

        public bool CanExecuteAction(AppActionEnum actionName, string appName, ActionParameters parameters = null)
        {
            var action = GetAction(actionName);
            return action?.CanExecute(appName, parameters) ?? false;
        }

        public bool CanExecuteAction(string actionName, string appName, ActionParameters parameters = null)
        {
            var action = GetAction(actionName);
            return action?.CanExecute(appName, parameters) ?? false;
        }

        public async Task<bool> ExecuteActionAsync(AppActionEnum actionName, string appName, ActionParameters parameters = null)
        {
            var action = GetAction(actionName);
            if (action == null)
            {
                System.Diagnostics.Debug.WriteLine($"Action not found: {actionName}");
                return false;
            }

            try
            {
                return await action.ExecuteAsync(appName, parameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing action {actionName} on {appName}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExecuteActionAsync(string actionName, string appName, ActionParameters parameters = null)
        {
            if (Enum.TryParse<AppActionEnum>(actionName, true, out AppActionEnum actionEnum))
            {
                return await ExecuteActionAsync(actionEnum, appName, parameters);
            }

            System.Diagnostics.Debug.WriteLine($"Invalid action name: {actionName}");
            return false;
        }

        public async Task<Dictionary<string, bool>> ExecuteMultipleActionsAsync(
            IEnumerable<(AppActionEnum actionName, string appName, ActionParameters parameters)> actions)
        {
            var results = new Dictionary<string, bool>();
            
            foreach (var (actionName, appName, parameters) in actions)
            {
                var key = $"{actionName}_{appName}";
                results[key] = await ExecuteActionAsync(actionName, appName, parameters);
            }

            return results;
        }

        public async Task<Dictionary<string, bool>> ExecuteMultipleActionsAsync(
            IEnumerable<(string actionName, string appName, ActionParameters parameters)> actions)
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