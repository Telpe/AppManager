using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AppManager.Actions
{
    public class ProcessRunning
    {
        private readonly List<string> _ResponseMessages = new List<string>();
        private readonly string _ErrorMessage = "";
        private bool _Running = false;
        private int _ClosingAttempts = 0;

        public string AppName { get; set; }
        public List<string> ResponseMessages { get { return _ResponseMessages; } }
        public bool Running { get { return _Running; } }
        public int ClosingAttempts { get { return _ClosingAttempts; } }
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
                _Running = true;
                _ClosingAttempts++;
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
                    _ResponseMessages.Add(process.StandardOutput.ReadLine());
                }

                process.WaitForExit();
                _Running = false;
            }
            catch (Exception ev)
            {
                _ResponseMessages.Add( ev.Message );
                _Running = false;
            }
        }
    }
}
