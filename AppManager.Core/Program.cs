using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AppManager.Core
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (ShouldITerminate())
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Create and run the tray application
            using (var trayApp = new TrayApplication())
            {
                Application.Run();
            }

            
        }

        protected static bool ShouldITerminate()
        {
            if (CheckSelfRunning(out Process? notSelf) && null != notSelf)
            {
                return true;
            }

            return false;
        }

        protected static bool CheckSelfRunning(out Process? notSelf)
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = ProcessManager.FindProcesses(currentProcess.ProcessName, false, null, false, false, currentProcess.Id);

            if (processes.Length > 0)
            {
                notSelf = processes[0];
                return true;
            }

            notSelf = null;
            return false;
        }
    }

}
