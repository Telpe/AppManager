using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AppManager.Core.Actions
{
    public class ProcessRunning
    {
        private readonly List<string> ResponseMessagesStored = new List<string>();
        private readonly string ErrorMessageStored = "";
        private bool RunningStored = false;
        private int ClosingAttemptsStored = 0;

        public string AppName { get; set; }
        public List<string> ResponseMessages { get { return ResponseMessagesStored; } }
        public bool Running { get { return RunningStored; } }
        public int ClosingAttempts { get { return ClosingAttemptsStored; } }
        public ProcessRunningOptions Options { get; set; }

        public ProcessRunning(string inputName)
        {
            AppName = inputName;
            Options = new ProcessRunningOptions();
        }

        public ProcessRunning(string inputName, ProcessRunningOptions ProcessRunningrOptions)
        {
            AppName = inputName;
            Options = ProcessRunningrOptions;
        }

        private string ComposeArguments()
        {
            string args = "";

            if ((bool)Options.ForceKill)
            {
                args += "/F ";
            }
            if ((bool)Options.IncludeChildren)
            {
                args += "/T ";
            }
            
            args += "/IM " + AppName;
            
            if ((bool)Options.IncludeTasksLikeGiven)
            {
                args += "*";
            }

            return args;
        }

        public void DoProcessRunning()
        {
            try
            {
                RunningStored = true;
                ClosingAttemptsStored++;
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "C:\\Windows\\System32\\taskkill.exe",
                        Arguments = ComposeArguments(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    ResponseMessagesStored.Add(process.StandardOutput.ReadLine());
                }

                process.WaitForExit();
                RunningStored = false;
            }
            catch (Exception ev)
            {
                ResponseMessagesStored.Add( ev.Message );
                RunningStored = false;
            }
        }
    }
}
