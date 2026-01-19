using AppManager.Core.Models;
using AppManager.Core.Utilities;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace AppManager.Core.Actions
{
    public class LaunchAction : BaseAction, ILaunchAction
    {
        public string? AppName { get; set; }
        public override AppActionTypeEnum ActionType => AppActionTypeEnum.Launch;
        public override string Description => "Launches an application";

        public string? ExecutablePath { get; set; }
        public string? Arguments { get; set; }
        public int? TimeoutMs { get; set; }

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

            RegistryKey? key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"SOFTWARE\MICROSOFT\WINDOWS\CURRENTVERSION\APPMODEL\STATEREPOSITORY\CACHE\APPLICATION\DATA");
            RegistryKey? fileInfo = null;

            try
            {
                var possibleNames = FileManager.ExecuteableExtensions.Select(ext => AppName + ext);

                if (null == key64)
                {
                    Log.WriteLine($"Could not open registry key for finding UWP apps: SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\APPMODEL\\STATEREPOSITORY\\CACHE\\APPLICATION\\DATA");
                    return false;
                }

                foreach (string keyName in key64!.GetSubKeyNames())
                {
                    RegistryKey? key = key64.OpenSubKey(keyName);
                    bool found = false;
                    if (null == key) { continue; }

                    try
                    {
                        string executable = key.GetValue("Executable") as string ?? throw new Exception("no value for Executable");

                        if (possibleNames.Any(executable.EndsWith))
                        {
                            fileInfo = key;
                            found = true;
                            break;
                        }
                    }
                    catch { continue; }
                    finally
                    {
                        if (!found)
                        {
                            key.Dispose();
                        }
                    }
                }

                if (null != fileInfo)
                {
                    ExecutablePath = $"explorer.exe";
                    Arguments = @"shell:AppsFolder\" + (fileInfo.GetValue("ApplicationUserModelId") as string);

                    return true;
                }
            }
            finally
            {
                key64?.Dispose();
                fileInfo?.Dispose();
            }
            
            Log.WriteLine($"Could not find executable for: {AppName}");
            return false;
        }

        protected override bool ExecuteAction()
        {
            Process? process = null;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = ExecutablePath,
                    Arguments = Arguments ?? string.Empty,
                    UseShellExecute = true
                };

                process = Process.Start(startInfo);

                Thread.Sleep(TimeoutMs ?? 0);

                if (process is not null)
                {
                    Log.WriteLine($"Successfully launched: {AppName}");

                    if (process.HasExited)
                    {
                        Log.WriteLine($"{AppName} exited immediately.");
                    }

                    return true;
                }
                else
                {
                    throw new Exception($"Failed to launch {AppName}");
                }
            }
            finally
            {
                process?.Dispose();
            }

        }

        public override ActionModel ToModel()
        {
            return new ActionModel
            {
                ActionType = ActionType,
                Conditions = Conditions.Select(c => c.ToModel()).ToArray(),
                AppName = AppName,
                ExecutablePath = ExecutablePath,
                Arguments = Arguments,
                TimeoutMs = TimeoutMs
            };
        }
    }
}