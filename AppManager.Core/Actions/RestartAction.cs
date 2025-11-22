using AppManager.Core.Models;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class RestartAction : BaseAction
    {
        public override string Description => "Restarts an application by closing and launching it";

        public RestartAction(ActionModel model) : base(model) { }

        protected override bool CanExecuteAction()
        {
            var closeAction = new CloseAction(_Model);
            var launchAction = new LaunchAction(_Model);
            
            return closeAction.CanExecute() && launchAction.CanExecute();
        }

        public override async Task<bool> ExecuteAsync()
        {
            if (_Model == null || string.IsNullOrEmpty(_Model.AppName))
            {
                System.Diagnostics.Debug.WriteLine("Invalid ActionModel or AppName is null/empty");
                return false;
            }

            // Check conditions before executing
            if (!CheckConditions())
            {
                System.Diagnostics.Debug.WriteLine($"Conditions not met for restarting {_Model.AppName}");
                return false;
            }

            var closeAction = new CloseAction(_Model);
            var launchAction = new LaunchAction(_Model);

            // First close the application
            bool closed = await closeAction.ExecuteAsync();
            
            if (!closed)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to close {_Model.AppName} for restart");
                return false;
            }

            // Wait a moment before launching
            await Task.Delay(1000);

            // Then launch it again
            bool launched = await launchAction.ExecuteAsync();
            
            if (!launched)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to launch {_Model.AppName} after close");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Successfully restarted: {_Model.AppName}");
            return true;
        }
    }
}