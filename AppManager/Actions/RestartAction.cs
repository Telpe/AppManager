using System.Threading.Tasks;

namespace AppManager.Actions
{
    public class RestartAction : IAppAction
    {
        private readonly CloseAction _closeAction;
        private readonly LaunchAction _launchAction;

        public AppActionEnum ActionName => AppActionEnum.Restart;
        public string Description => "Restarts an application by closing and launching it";

        public RestartAction()
        {
            _closeAction = new CloseAction();
            _launchAction = new LaunchAction();
        }

        public bool CanExecute(string appName, ActionParameters parameters = null)
        {
            return _closeAction.CanExecute(appName, parameters) && 
                   _launchAction.CanExecute(appName, parameters);
        }

        public async Task<bool> ExecuteAsync(string appName, ActionParameters parameters = null)
        {
            // First close the application
            bool closed = await _closeAction.ExecuteAsync(appName, parameters);
            
            if (!closed)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to close {appName} for restart");
                return false;
            }

            // Wait a moment before launching
            await Task.Delay(1000);

            // Then launch it again
            bool launched = await _launchAction.ExecuteAsync(appName, parameters);
            
            if (!launched)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch {appName} after close");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Successfully restarted: {appName}");
            return true;
        }
    }
}