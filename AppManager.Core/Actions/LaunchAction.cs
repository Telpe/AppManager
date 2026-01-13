using AppManager.Core.Models;
using AppManager.Core.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AppManager.Core.Actions
{
    public class LaunchAction : BaseAction, ILaunchAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Launch;
        public override string Description => "Launches an application";

        public string? ExecutablePath { get; set; }
        public string? Arguments { get; set; }

        public LaunchAction(ActionModel model) : base(model)
        {
            AppName = model.AppName ?? String.Empty;
            ExecutablePath = model.ExecutablePath;
            Arguments = model.Arguments;
        }

        protected override bool CanExecuteAction()
        {
            return CheckExecutablePath();
        }

        private bool CheckExecutablePath()
        {
            if (File.Exists(ExecutablePath)) { return true; }

            if (String.IsNullOrEmpty(AppName)) { throw new Exception($"{ActionType} require an AppName when the executeable path is not a file"); }

            string[] executablePaths = Directory.Exists(ExecutablePath) ? FileManager.FindExecutables(AppName, [ExecutablePath]) : FileManager.FindExecutables(AppName);

            if (executablePaths.Length == 1)
            {
                ExecutablePath = executablePaths[0];
                return true;
            }

            if (executablePaths.Length > 1)
            {
                string[] newExecutablePaths = executablePaths.Where(p => AppName == Path.GetFileNameWithoutExtension(p)).ToArray();

                if (0 < newExecutablePaths.Length)
                {
                    ExecutablePath = newExecutablePaths[0];
                    return true;
                }

                
            }

            Log.WriteLine($"Could not find executable for: {AppName}");
            return false;
        }

        protected override void ExecuteAction()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ExecutablePath,
                Arguments = Arguments ?? string.Empty,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);

            if (process != null)
            {
                Log.WriteLine($"Successfully launched: {AppName}");
                return;
            }

            throw new Exception($"Failed to launch {AppName}");
        }

        public override ActionModel ToModel()
        {
            return new ActionModel
            {
                ActionType = ActionType,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray(),
                AppName = AppName,
                ExecutablePath = ExecutablePath,
                Arguments = Arguments
            };
        }
    }
}