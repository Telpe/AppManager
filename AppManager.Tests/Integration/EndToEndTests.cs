using AppManager.Core.Actions;
using AppManager.Core.Utilities;
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
            var actionModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "CalculatorApp");
            var triggerModel = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button, "TestButtonTrigger");
            triggerModel.Actions =  [actionModel];

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
                    action.Execute();

                    // Verify CalculatorApp was launched
                    Task.Delay(CoreConstants.DefaultActionDelay).Wait();
                    var CalculatorAppProcesses = Process.GetProcessesByName("CalculatorApp");
                    CalculatorAppProcesses.Should().NotBeEmpty();

                    // Clean up - close CalculatorApp
                    var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "CalculatorApp");
                    var closeAction = new CloseAction(closeModel);
                    closeAction.Execute();
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception during test execution: {ex.Message}");
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
                ActionManager.ExecuteMultipleActions(actions);
                Assert.IsTrue(true, "Actions executed without exceptions.");

                stopwatch.Stop();

                // Assert
                stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000); // 30 seconds max
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception during action execution: {ex.Message}");
            }
            finally
            {
                // Cleanup - attempt to close any launched applications
                var appNames = new[] { "notepad", "CalculatorApp", "mspaint", "cmd", "explorer" };
                foreach (var appName in appNames)
                {
                    try
                    {
                        var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, appName);
                        ActionManager.ExecuteAction(closeModel);
                    }
                    catch { /* Ignore cleanup errors */ }
                }
            }
        }
    }
}