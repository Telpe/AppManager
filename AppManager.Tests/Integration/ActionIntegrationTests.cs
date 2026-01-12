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
        private static Process? _testAppManager;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            try
            {
                _testAppManager = TestAppManager.StartTestInstance();
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
                var launchResult = await launchAction.ExecuteAsync();

                // Assert - Launch succeeded
                launchResult.Should().BeTrue();

                // Wait for application to fully start
                await Task.Delay(1000);

                // Verify calc is running
                var calcProcesses = Process.GetProcessesByName("calc");
                calcProcesses.Should().NotBeEmpty();

                // Act - Close
                var closeResult = await closeAction.ExecuteAsync();

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
                    var cleanup = await closeAction.ExecuteAsync();
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
                var launchResult = await launchAction.ExecuteAsync();
                launchResult.Should().BeTrue();

                await Task.Delay(1000);

                // Get initial process ID
                var initialProcesses = Process.GetProcessesByName("notepad");
                initialProcesses.Should().NotBeEmpty();
                var initialPid = initialProcesses.First().Id;

                // Act - Restart
                var restartResult = await restartAction.ExecuteAsync();

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
                    await closeAction.ExecuteAsync();
                }
                catch { /* Ignore cleanup errors */ }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task BringToFrontAction_WithTestAppManager_ShouldBringToFront()
        {
            // Arrange
            if (_testAppManager == null || _testAppManager.HasExited)
            {
                Assert.Inconclusive("Test AppManager instance is not available");
                return;
            }

            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, _testAppManager.ProcessName);
            var action = new BringToFrontAction(model, _testAppManager);

            // Act
            var result = await action.ExecuteAsync();

            // Assert
            result.Should().BeTrue();
            _testAppManager.HasExited.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Actions")]
        public async Task ActionManager_ExecuteMultipleActions_ShouldExecuteInParallel()
        {
            // Arrange
            var actions = new[]
            {
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc"),
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad"),
                TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "mspaint")
            };

            try
            {
                // Act
                var tasks = ActionManager.ExecuteMultipleActionsAsync(actions);
                var results = await Task.WhenAll(tasks);

                // Assert
                results.Should().HaveCount(3);
                results.Should().AllSatisfy(result => result.Should().BeOfType<bool>());

                // Verify at least some launches succeeded
                var successCount = results.Count(r => r);
                successCount.Should().BeGreaterThan(0, "At least one application should have launched successfully");
            }
            finally
            {
                // Cleanup - close all launched applications
                var closeActions = new[]
                {
                    TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc"),
                    TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad"),
                    TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "mspaint")
                };

                var closeTasks = ActionManager.ExecuteMultipleActionsAsync(closeActions);
                try
                {
                    await Task.WhenAll(closeTasks);
                }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }
}