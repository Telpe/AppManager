using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class RestartAction : IAppAction
    {
        private readonly CloseAction _closeAction;
        private readonly LaunchAction _launchAction;

        public string Description => "Restarts an application by closing and launching it";

        public RestartAction()
        {
            _closeAction = new CloseAction();
            _launchAction = new LaunchAction();
        }

        public bool CanExecute(ActionModel model)
        {
            return _closeAction.CanExecute(model) && _launchAction.CanExecute(model);
        }

        public async Task<bool> ExecuteAsync(ActionModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.AppName))
            {
                System.Diagnostics.Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            // First close the application
            bool closed = await _closeAction.ExecuteAsync(model);
            
            if (!closed)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to close {model.AppName} for restart");
                return false;
            }

            // Wait a moment before launching
            await Task.Delay(1000);

            // Then launch it again
            bool launched = await _launchAction.ExecuteAsync(model);
            
            if (!launched)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch {model.AppName} after close");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Successfully restarted: {model.AppName}");
            return true;
        }
    }
}