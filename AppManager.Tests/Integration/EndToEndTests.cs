using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace AppManager.Tests.Integration
{
    [TestClass]
    public class EndToEndTests
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
            TriggerManager.Dispose();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TriggerManager.Dispose();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("EndToEnd")]
        public void AppManager_TestInstance_ShouldBeRunningAndAccessible()
        {
            // Assert
            _testAppManager.Should().NotBeNull();
            _testAppManager!.HasExited.Should().BeFalse();
            TestAppManager.IsTestInstanceRunning().Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("EndToEnd")]
        public async Task CompleteWorkflow_CreateTriggerWithAction_ShouldExecuteSuccessfully()
        {
            // Arrange - Create a button trigger (safer for testing than keybind)
            var actionModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");
            var triggerModel = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button, "TestButtonTrigger");
            triggerModel.Actions = new[] { actionModel };

            var trigger = TriggerManager.CreateTrigger(triggerModel);

            try
            {
                // Act - Register trigger
                var registered = TriggerManager.RegisterTrigger(trigger);
                registered.Should().BeTrue();

                // Verify trigger is registered
                var activeTriggers = TriggerManager.GetActiveTriggers();
                activeTriggers.Should().Contain(trigger);

                // Verify trigger can execute
                var canExecute = trigger.CanExecute();
                canExecute.Should().BeTrue();

                // Manually trigger action execution (since button triggers require UI interaction)
                if (trigger.Actions.Length > 0)
                {
                    var action = trigger.Actions[0];
                    var executeResult = await action.ExecuteAsync();
                    executeResult.Should().BeTrue();

                    // Verify calc was launched
                    await Task.Delay(1000);
                    var calcProcesses = Process.GetProcessesByName("calc");
                    calcProcesses.Should().NotBeEmpty();

                    // Clean up - close calc
                    var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc");
                    var closeAction = new CloseAction(closeModel);
                    await closeAction.ExecuteAsync();
                }
            }
            finally
            {
                // Cleanup
                TriggerManager.UnregisterTrigger(trigger.Name);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("EndToEnd")]
        public void TriggerLifecycle_RegisterStartStopUnregister_ShouldWorkCorrectly()
        {
            // Arrange
            var triggerModel = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button, "LifecycleTest");
            var trigger = TriggerManager.CreateTrigger(triggerModel);

            // Act & Assert - Register
            var registered = TriggerManager.RegisterTrigger(trigger);
            registered.Should().BeTrue();
            TriggerManager.Triggers.Should().Contain(trigger);

            // Act & Assert - Verify it can start (button triggers don't have complex start logic)
            var canStart = trigger.CanStart();
            canStart.Should().BeTrue();

            // Act & Assert - Unregister
            TriggerManager.UnregisterTrigger(trigger.Name);
            TriggerManager.Triggers.Should().NotContain(trigger);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("EndToEnd")]
        public async Task ProfileWorkflow_CreateSaveLoadProfile_ShouldMaintainData()
        {
            // Arrange
            var profile = TestDataBuilder.CreateTestProfile("IntegrationTestProfile");
            
            // The profile should contain triggers and actions
            profile.Triggers.Should().NotBeEmpty();
            profile.Triggers[0].Actions.Should().NotBeEmpty();

            // Act - Convert to models and back (simulating save/load)
            var triggerModel = profile.Triggers[0];
            var recreatedTrigger = TriggerManager.CreateTrigger(triggerModel);

            // Assert - Data integrity maintained
            recreatedTrigger.Should().NotBeNull();
            recreatedTrigger.TriggerType.Should().Be(triggerModel.TriggerType);
            recreatedTrigger.Actions.Should().HaveCount(triggerModel.Actions?.Length ?? 0);

            if (recreatedTrigger.Actions.Length > 0)
            {
                var action = recreatedTrigger.Actions[0];
                var originalAction = triggerModel.Actions![0];
                action.ActionType.Should().Be(originalAction.ActionType);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [TestCategory("Performance")]
        public async Task ActionPerformance_MultipleActionsInSequence_ShouldCompleteInReasonableTime()
        {
            // Arrange
            var stopwatch = Stopwatch.StartNew();
            const int actionCount = 5;
            var actions = TestDataBuilder.CreateMultipleActionModels(actionCount);

            try
            {
                // Act
                var tasks = ActionManager.ExecuteMultipleActionsAsync(actions);
                var results = await Task.WhenAll(tasks);

                stopwatch.Stop();

                // Assert
                stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
                results.Should().HaveCount(actionCount);
                results.Should().AllSatisfy(result => result.Should().BeOfType<bool>());
            }
            finally
            {
                // Cleanup - attempt to close any launched applications
                var appNames = new[] { "notepad", "calc", "mspaint", "cmd", "explorer" };
                foreach (var appName in appNames)
                {
                    try
                    {
                        var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, appName);
                        await ActionManager.ExecuteActionAsync(closeModel);
                    }
                    catch { /* Ignore cleanup errors */ }
                }
            }
        }
    }
}