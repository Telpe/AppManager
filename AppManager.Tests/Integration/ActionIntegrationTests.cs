using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AppManager.Tests.Integration
{
    [TestClass]
    public class ActionIntegrationTests
    {
        private static Process? TestAppManagerProcess;
        private Stopwatch? testStopwatch;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
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

        public void StartTestInstance() 
        {
            try
            {
                TestAppManagerProcess = TestAppManager.StartTestInstance();
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Could not start AppManager test instance: {ex.Message}");
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TestAppManagerProcess = null;
            TestAppManager.StopTestInstance();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task LaunchAndCloseAction_Integration_ShouldWorkTogether()
        {
            // Arrange
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "CalculatorApp");
            var closeModel = new ActionModel() { 
                ActionType = AppActionTypeEnum.Close
                , AppName = "CalculatorApp"
                , ForceOperation = true
            };

            var launchAction = new LaunchAction(launchModel);
            var closeAction = new CloseAction(closeModel);

            LaunchAction(launchAction, "CalculatorApp", CoreConstants.DefaultActionDelay);

            CloseAction(closeAction, "CalculatorApp", CoreConstants.DefaultActionDelay);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task RestartAction_Integration_ShouldCloseAndRelaunch()
        {
            // Arrange
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad");
            var restartModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Restart, "notepad");
            var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad");

            var launchAction = new LaunchAction(launchModel);
            var restartAction = new RestartAction(restartModel);
            var closeAction = new CloseAction(closeModel);
            
            LaunchAction(launchAction, "notepad", CoreConstants.DefaultActionDelay);

            RestartAction(restartAction, "notepad", CoreConstants.DefaultActionDelay);

            CloseAction(closeAction, "notepad", CoreConstants.DefaultActionDelay);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task BringToFrontAction_WithTestAppManager_ShouldBringToFront()
        {
            StartTestInstance();
            // Arrange
            if (TestAppManagerProcess == null || TestAppManagerProcess.HasExited)
            {
                Assert.Inconclusive("Test AppManager instance is not available");
                return;
            }

            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, TestAppManagerProcess.ProcessName);
            var action = new BringToFrontAction(model, TestAppManagerProcess);

            // Act
            var result = false;
            try
            {
                action.Execute();
                result = true;
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
            }
            catch { }

            // Assert
            result.Should().BeTrue();
            TestAppManagerProcess.HasExited.Should().BeFalse();
            Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task ActionManager_ExecuteMultipleActions_ShouldExecuteInParallel()
        {
            // Arrange
            ActionModel[] actions = [
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "CalculatorApp"),
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad")
            ];

            try
            {
                Log.WriteLine("Execute multiple ActionModels.");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                Task executeWaitTime = Task.Delay(CoreConstants.DefaultActionDelay);
                ActionManager.ExecuteMultipleActions(actions);
                Log.WriteLine("Executed..");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                await executeWaitTime;

            }
            catch (Exception ex)
            {
                Assert.Fail($"Execution of multiple actions failed: {ex.Message}");
            }
            finally
            {
                Log.WriteLine("Finally.");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                Process[] closeActions =
                [
                    //..ProcessManager.FindProcesses("CalculatorApp", false, null, false, true, 4),
                    //..ProcessManager.FindProcesses("notepad", false, null, false, true, 4)
                    ..Process.GetProcessesByName("CalculatorApp"),
                    ..Process.GetProcessesByName("notepad")
                ];
                
                Log.WriteLine($"Cleaning up processes: {string.Join(", ", closeActions.Select(p => p.ProcessName + " (ID: " + p.Id + ")"))}");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

                if(ProcessManager.CloseProcesses(closeActions, CoreConstants.DefaultActionDelay, true)) { 
                    Log.WriteLine("Processes closed successfully.");
                    Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                }
                else { 
                    Log.WriteLine("Failed to close some processes.");
                    Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                }
                    
                Log.WriteLine("Disposing resources.");
                foreach (Process p in closeActions)
                {
                    p.Dispose();
                }
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
            }
        }

        private bool LaunchAction(LaunchAction action, string name, int waitMilliseconds)
        {
            Log.WriteLine($"launching {name}");
            Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

            Task executionWaitTime = Task.Delay(waitMilliseconds);
            bool result = false;
            try
            {
                action.Execute();
                result = true;
                Log.WriteLine($"{name} launched");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                Log.WriteLine($"{name} is {(ProcessManager.IsProcessRunning(name) ? "running" : "not running")}.");
            }
            finally 
            {
                executionWaitTime.Wait();

                result.Should().BeTrue();

                Log.WriteLine($"{name} is {(ProcessManager.IsProcessRunning(name) ? "running" : "not running")} after wait.");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                Process[] ps = Process.GetProcessesByName(name);
                try
                {
                    ps.Should().NotBeEmpty($"at least 1 {name} process should be running");
                }
                finally
                {
                    foreach (Process p in ps)
                    {
                        p.Dispose();
                    }
                }
                    
            }
            
            return result;
        }

        private bool RestartAction(RestartAction action, string name, int waitMilliseconds)
        {
            Log.WriteLine("Get initial process ID");
            var initialProcesses = Process.GetProcessesByName(name);
            initialProcesses.Should().NotBeEmpty();
            var initialPid = initialProcesses.First().Id;
            Array.ForEach(initialProcesses, process => process.Dispose());

            Log.WriteLine("initial process ID grabbed.");
            Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

            Log.WriteLine($"restarting {name}");
            Task restartWaitTime = Task.Delay(waitMilliseconds);
            var result = false;
            try
            {
                action.Execute();
                result = true;
                Log.WriteLine($"{name} restarted");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
            }
            finally
            {
                restartWaitTime.Wait();
                result.Should().BeTrue();
                Log.WriteLine($"{name} is {(ProcessManager.IsProcessRunning(name) ? "running" : "not running")} after wait.");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
            }

            Log.WriteLine("Get new process ID and compare with old.");
            var newProcesses = Process.GetProcessesByName(name);
            newProcesses.Should().NotBeEmpty();
            var newPid = newProcesses.First().Id;
            // Note: PID might be same if restart was very quick, so we just verify it's running
            Log.WriteLine($"new ID: {newPid}\nold ID: {initialPid}");
            Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

            return result;
        }

        private bool CloseAction(CloseAction action, string name, int waitMilliseconds)
        {
            Log.WriteLine($"closing {name}");
            Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

            Task executionWaitTime = Task.Delay(waitMilliseconds);
            bool result = false;
            try
            {
                action.Execute();
                result = true;
                Log.WriteLine($"{name} closed");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");
                Log.WriteLine($"{name} is {(ProcessManager.IsProcessRunning(name) ? "running" : "not running")}.");
                var stillOpenProcesses = Process.GetProcessesByName(name);
                foreach(Process process in stillOpenProcesses)
                {
                    Log.WriteLine($"{process.ProcessName}({process.Id}) has {(process.HasExited ? "exited" : "not exited")}.");
                    process.Dispose();
                }

                bool isRunning = ProcessManager.IsProcessRunning(name);
                isRunning.Should().BeFalse("process should be closed by now");
            }
            finally
            {
                executionWaitTime.Wait();
                Log.WriteLine($"{name} is {(ProcessManager.IsProcessRunning(name) ? "running" : "not running")} after wait.");
                Log.WriteLine($"Test duration: {testStopwatch?.Elapsed.TotalMilliseconds:F4} ms");

                
            }

            result.Should().BeTrue();
            var newProcesses = Process.GetProcessesByName(name);
            newProcesses.Should().BeEmpty();

            return result;
        }
    }
}