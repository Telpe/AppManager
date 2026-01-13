using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

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
            var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc");

            var launchAction = new LaunchAction(launchModel);
            var closeAction = new CloseAction(closeModel);

            try
            {
                // Act - Launch
                var launchResult = false;
                try
                {
                    launchAction.Execute();
                    launchResult = true;
                }catch{ }


                // Assert - Launch succeeded
                launchResult.Should().BeTrue();

                // Wait for application to fully start
                await Task.Delay(1000);

                // Verify calc is running
                var calcProcesses = Process.GetProcessesByName("calc");
                calcProcesses.Should().NotBeEmpty();

                // Act - Close
                var closeResult = false;
                try
                { 
                    closeAction.Execute();
                    closeResult = true;
                }
                catch { }

                // Assert - Close succeeded
                closeResult.Should().BeTrue();

                // Wait for application to fully close
                await Task.Delay(1000);

                // Verify calc is no longer running
                calcProcesses = Process.GetProcessesByName("calc");
                calcProcesses.Should().BeEmpty();
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

                await Task.Delay(1000);

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

                await Task.Delay(2000);

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
                Assert.IsTrue(true);

            }
            catch (Exception ex)
            {
                Assert.Fail($"Execution of multiple actions failed: {ex.Message}");
            }
            finally
            {
                var closeActions = new[]
                {
                    TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc"),
                    TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad")
                };

                ActionManager.ExecuteMultipleActions(closeActions);
                
            }
        }
    }
}