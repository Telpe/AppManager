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
                    ActionFactory.CreateAction(new()
                    {
                        ActionType = ActionTypeEnum.BringToFront,
                        AppName = notSelf!.ProcessName,
                        ProcessLastId = notSelf.Id

                    }).Execute();

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

        public static string GuidToShortString(Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())[0..^2].Replace('+', '-').Replace('/', '_');
        }

        public static Guid GuidFromShortString(string str)
        {
            str = str.Replace('_', '/').Replace('-', '+');
            return new Guid(Convert.FromBase64String(str + "=="));
        }

    }
}
