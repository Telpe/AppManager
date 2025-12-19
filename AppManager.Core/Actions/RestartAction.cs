using AppManager.Core.Models;
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

        protected override Task<bool> ExecuteActionAsync()
        {
            return Task<bool>.Run(() =>
            {
                try
                {
                    if (_Close?.CanExecute()??false)
                    {
                        // First close the application
                        var closed = _Close.ExecuteAsync();
                        closed.Wait();

                        if (!(closed?.Result ?? false))
                        {
                            Debug.WriteLine($"Failed to close {AppName} for restart");
                        }

                        // Wait a moment before launching
                        Task.Delay(1000).Wait();
                    }
                    else
                    {
                        Debug.WriteLine($"Close action can not be executed for {AppName}, proceeding to launch.");
                    }

                    var launched = _Launch?.ExecuteAsync();
                    launched?.Wait();

                    if (!(launched?.Result ?? false))
                    {
                        Debug.WriteLine($"Failed to launch {AppName} after close");
                        return false;
                    }

                    Debug.WriteLine($"Successfully restarted: {AppName}");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to restart {AppName}: {ex.Message}");
                    return false;
                }
            });
        }

        public override ActionModel ToModel()
        {
            return new ActionModel
            {
                AppName = AppName,
                ActionType = ActionType,
                IncludeChildProcesses = IncludeChildProcesses,
                IncludeSimilarNames = IncludeSimilarNames,
                TimeoutMs = TimeoutMs,
                ForceOperation = ForceOperation,
                ExecutablePath = ExecutablePath,
                Arguments = Arguments,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray()
            };
        }
    }
}