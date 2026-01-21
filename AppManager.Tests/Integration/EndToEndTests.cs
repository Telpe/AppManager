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

            var trigger = TriggerFactory.CreateTrigger(triggerModel);

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
            var trigger = TriggerFactory.CreateTrigger(triggerModel);

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
            var recreatedTrigger = TriggerFactory.CreateTrigger(triggerModel);

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
    }
}