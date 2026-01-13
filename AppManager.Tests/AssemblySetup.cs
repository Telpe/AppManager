using AppManager.Core.Utils;
using AppManager.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;

namespace AppManager.Tests
{
    [TestClass]
    public class AssemblySetup
    {
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Log.Dispose();
        }

        
    }
}