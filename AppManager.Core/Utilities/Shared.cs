using AppManager.Core.Actions;
using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AppManager.Core.Utilities
{
    public static class Shared
    {

        public static bool ShouldITerminate()
        {
            Process? notSelf = null;
            try
            {
                return CheckSelfRunning(out notSelf);
            }
            finally
            {
                notSelf?.Dispose();
            }
        }

        public static bool ShouldITerminateBringingOtherToFront()
        {
            Process? notSelf = null;

            try
            {
                if (Shared.CheckSelfRunning(out notSelf))
                {
                    ActionManager.CreateAction(new ActionModel
                    {
                        ActionType = AppActionTypeEnum.BringToFront,
                        AppName = notSelf!.ProcessName,

                    }, notSelf).Execute();

                    return true;
                }
            }
            finally
            {
                notSelf?.Dispose();
            }

            return false;
        }

        public static bool CheckSelfRunning(out Process? notSelf)
        {
            var currentProcess = Process.GetCurrentProcess();

            try
            {
                notSelf = ProcessManager.FindProcess(currentProcess.ProcessName, false, false, null, currentProcess.Id);
                return null != notSelf;
            }
            finally
            {
                currentProcess.Dispose();
            }
        }
    }
}
