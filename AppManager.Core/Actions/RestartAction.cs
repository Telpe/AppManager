using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class RestartAction : BaseAction, IRestartAction
    {
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Restart;
        public override string Description => "Restarts an application by closing and launching it";
        public string? AppName { get; set; }
        public bool? IncludeChildProcesses { get; set; }
        public bool? IncludeSimilarNames { get; set; }
        public int? TimeoutMs { get; set; }
        public bool? ForceOperation { get; set; }
        public string? ExecutablePath { get; set; }
        public string? Arguments { get; set; }
        private CloseAction? _Close { get; set; }
        private LaunchAction? _Launch { get; set; }

        public RestartAction(ActionModel model) : base(model) 
        {
            AppName = model.AppName;
            IncludeChildProcesses = model.IncludeChildProcesses;
            IncludeSimilarNames = model.IncludeSimilarNames;
            TimeoutMs = model.TimeoutMs;
            ForceOperation = model.ForceOperation;
            ExecutablePath = model.ExecutablePath;
            Arguments = model.Arguments;
            
        }

        protected override bool CanExecuteAction()
        {
            var closeM = ToModel();
            var launchM = ToModel();

            closeM.ActionType = AppActionTypeEnum.Close;
            launchM.ActionType = AppActionTypeEnum.Launch;

            _Close = new CloseAction(closeM);
            _Launch = new LaunchAction(launchM);

            return _Launch.CanExecute();
        }

        protected override bool ExecuteAction()
        {
            if (_Close?.Execute() ?? false)
            {
                Thread.Sleep(CoreConstants.ProcessRestartDelay);
            }
            else
            {
                Log.WriteLine($"Close action can not be executed for {AppName}, proceeding to launch.");
            }

            bool result = _Launch!.Execute();

            Log.WriteLine($"{(result ? "Successive" : "Failed")} restart: {AppName}");

            return result;
        }

        public override ActionModel ToModel()
        {
            return ToActionModel<IRestartAction>();
        }
    }
}