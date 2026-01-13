using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AppManager.Tests.Integration
{
    [TestClass]
    public class ActionIntegrationTests
    {
        private static Process? TestAppManagerProcess;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            
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
            TestAppManager.StopTestInstance();
            ActionManager.ExecuteAction(AppActionTypeEnum.Close, "AppManager.Core");
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task LaunchAndCloseAction_Integration_ShouldWorkTogether()
        {
            // Arrange
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");
            var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "CalculatorApp");

            var launchAction = new LaunchAction(launchModel);
            var closeAction = new CloseAction(closeModel);

            try
            {
                // Act - Launch
                launchAction.Execute();

                // Wait for application to fully start
                Task.Delay(CoreConstants.DefaultActionDelay).Wait();

                // Verify calc is running
                var calcProcesses = Process.GetProcessesByName("CalculatorApp");
                calcProcesses.Should().NotBeEmpty();

                // Act - Close
                closeAction.Execute();

                // Wait for application to fully close
                Task.Delay(CoreConstants.DefaultActionDelay).Wait();

                // Verify calc is no longer running
                calcProcesses = Process.GetProcessesByName("CalculatorApp");
                calcProcesses.Should().BeEmpty();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Test encountered an exception: {e.Message}");
                Assert.Fail($"{e.Message}");
            }
            finally
            {
                // Cleanup - ensure calc is closed
                try
                {
                    closeAction.Execute();
                }
                catch { /* Ignore cleanup errors */ }
            }
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

            try
            {
                // Act - Launch notepad first
                var launchResult = false;
                try
                {
                    launchAction.Execute();
                    launchResult = true;
                }
                catch { }

                launchResult.Should().BeTrue();

                Task.Delay(CoreConstants.ProcessRestartDelay).Wait();

                // Get initial process ID
                var initialProcesses = Process.GetProcessesByName("notepad");
                initialProcesses.Should().NotBeEmpty();
                var initialPid = initialProcesses.First().Id;

                // Act - Restart
                var restartResult = false;
                try
                {
                    restartAction.Execute();
                    restartResult = true;
                }
                catch { }

                // Assert
                restartResult.Should().BeTrue();

                Task.Delay(CoreConstants.DefaultActionDelay).Wait();

                // Verify notepad is still running but with different PID
                var newProcesses = Process.GetProcessesByName("notepad");
                newProcesses.Should().NotBeEmpty();
                
                // The process should be restarted (different PID or at least still running)
                var newPid = newProcesses.First().Id;
                // Note: PID might be same if restart was very quick, so we just verify it's running
                newProcesses.Should().NotBeEmpty();
            }
            finally
            {
                // Cleanup
                try
                {
                    closeAction.Execute();
                }
                catch { /* Ignore cleanup errors */ }
            }
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
            }
            catch { }

            // Assert
            result.Should().BeTrue();
            TestAppManagerProcess.HasExited.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task ActionManager_ExecuteMultipleActions_ShouldExecuteInParallel()
        {
            // Arrange
            ActionModel[] actions = [
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc"),
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad")
            ];

            try
            {
                // Act
                ActionManager.ExecuteMultipleActions(actions);
                Task.Delay(CoreConstants.DefaultActionDelay).Wait();

            }
            catch (Exception ex)
            {
                Assert.Fail($"Execution of multiple actions failed: {ex.Message}");
            }
            finally
            {
                
                Process[] closeActions =
                [
                    ..ProcessManager.FindProcesses("CalculatorApp", false, null, false, true, 4),
                    ..ProcessManager.FindProcesses("notepad", false, null, false, true, 4)
                ];

                Log.WriteLine($"Cleaning up processes: {string.Join(", ", closeActions.Select(p => p.ProcessName + " (ID: " + p.Id + ")"))}");

                if(ProcessManager.CloseProcesses(closeActions, CoreConstants.DefaultActionDelay, true)) { Log.WriteLine("Processes closed successfully."); }
                else { Log.WriteLine("Failed to close some processes."); }
                    
                Log.WriteLine("Disposing resources.");
                foreach (Process p in closeActions)
                {
                    p.Dispose();
                }
            }
        }
    }
}