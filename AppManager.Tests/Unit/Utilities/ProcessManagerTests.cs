using AppManager.Core.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AppManager.Tests.Unit.Utilities
{
    [TestClass]
    public class ProcessManagerTests
    {
        private Stopwatch? testStopwatch;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {
            testStopwatch = Stopwatch.StartNew();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            testStopwatch?.Stop();
            testStopwatch = null;
        }

        [TestMethod]
        [TestCategory("Units")]
        [TestCategory("Utilities")]
        public async Task FindProcess_StrictName_LooseWindow()
        {
            string[] processNames = ["discord", "Steam"];

            foreach (string processName in processNames)
            {
                Log.WriteLine($"\nStaring for: {processName}");

                Process[] processesCompare = [];
                Process[] processes = [];

                try
                {
                    processesCompare = Process.GetProcessesByName(processName).OrderBy(a => a.Id).ToArray();
                    processes = ProcessManager.FindProcesses(processName).OrderBy(a => a.Id).ToArray();
                    Log.WriteLine($"Found: {processes.Length} processes");

                    for (int i = 0; i < processes.Length; i++)
                    {
                        processes[i].Id.Should().Be(processesCompare[i].Id);
                    }

                }
                finally
                {
                    foreach (Process p in processes)
                    {
                        p.Dispose();
                    }
                    foreach (Process p in processesCompare)
                    {
                        p.Dispose();
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Units")]
        [TestCategory("Utilities")]
        public async Task FindProcess_LooseName_LooseWindow()
        {
            string processName = "Steam";
            Process[] processesCompare = [];
            Process[] processes = [];

            try
            {
                processesCompare = ((IEnumerable<Process>)[..Process.GetProcessesByName(processName), ..Process.GetProcessesByName("steamservice"), ..Process.GetProcessesByName("steamwebhelper")]).OrderBy(a => a.Id).ToArray();
                processes = ProcessManager.FindProcesses(processName, true).OrderBy(a => a.Id).ToArray();

                Log.WriteLine($"Found {processes.Length} processes from name {processName}");

                for (int i = 0; i < processes.Length; i++)
                {
                    processes[i].Id.Should().Be(processesCompare[i].Id);
                }

            }
            finally
            {
                foreach (Process p in processes)
                {
                    p.Dispose();
                }
                foreach (Process p in processesCompare)
                {
                    p.Dispose();
                }
            }


        }
    }
}
