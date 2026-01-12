using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Triggers
{
    [TestClass]
    public class TriggerManagerTests
    {
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up all triggers after each test
            TriggerManager.Dispose();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void CreateTrigger_WithKeybindType_ShouldReturnKeybindTrigger()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Keybind);

            // Act
            var trigger = TriggerManager.CreateTrigger(model);

            // Assert
            trigger.Should().NotBeNull();
            trigger.Should().BeOfType<KeybindTrigger>();
            trigger.TriggerType.Should().Be(TriggerTypeEnum.Keybind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void CreateTrigger_WithUnsupportedType_ShouldThrowArgumentException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel();
            model.TriggerType = (TriggerTypeEnum)999; // Invalid trigger type

            // Act & Assert
            Action act = () => TriggerManager.CreateTrigger(model);
            act.Should().Throw<ArgumentException>()
               .WithMessage("Unsupported trigger type: 999");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void RegisterTrigger_WithValidTrigger_ShouldReturnTrue()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button); // Use Button instead of Keybind to avoid hooking issues in tests
            var trigger = TriggerManager.CreateTrigger(model);

            // Act
            var result = TriggerManager.RegisterTrigger(trigger);

            // Assert
            result.Should().BeTrue();
            TriggerManager.Triggers.Should().Contain(trigger);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void RegisterTrigger_WithDuplicateTrigger_ShouldReturnFalse()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            var trigger = TriggerManager.CreateTrigger(model);
            TriggerManager.RegisterTrigger(trigger);

            // Act
            var result = TriggerManager.RegisterTrigger(trigger);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void UnregisterTrigger_WithExistingTrigger_ShouldRemoveTrigger()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            var trigger = TriggerManager.CreateTrigger(model);
            TriggerManager.RegisterTrigger(trigger);
            string triggerName = trigger.Name;

            // Act
            TriggerManager.UnregisterTrigger(triggerName);

            // Assert
            TriggerManager.Triggers.Should().NotContain(t => t.Name == triggerName);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void GetActiveTriggers_WithMixedTriggers_ShouldReturnOnlyActiveTriggers()
        {
            // Arrange
            var activeModel = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            activeModel.Inactive = false;
            var inactiveModel = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            inactiveModel.Inactive = true;

            var activeTrigger = TriggerManager.CreateTrigger(activeModel);
            var inactiveTrigger = TriggerManager.CreateTrigger(inactiveModel);

            TriggerManager.RegisterTrigger(activeTrigger);
            TriggerManager.RegisterTrigger(inactiveTrigger);

            // Act
            var activeTriggers = TriggerManager.GetActiveTriggers().ToArray();

            // Assert
            activeTriggers.Should().Contain(activeTrigger);
            activeTriggers.Should().NotContain(inactiveTrigger);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void GetTriggerNames_WithMultipleTriggers_ShouldReturnAllNames()
        {
            // Arrange
            var model1 = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            var model2 = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            
            var trigger1 = TriggerManager.CreateTrigger(model1);
            var trigger2 = TriggerManager.CreateTrigger(model2);

            TriggerManager.RegisterTrigger(trigger1);
            TriggerManager.RegisterTrigger(trigger2);

            // Act
            var triggerNames = TriggerManager.GetTriggerNames().ToArray();

            // Assert
            triggerNames.Should().Contain(trigger1.Name);
            triggerNames.Should().Contain(trigger2.Name);
            triggerNames.Length.Should().Be(2);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void GetTrigger_WithExistingName_ShouldReturnTrigger()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            var trigger = TriggerManager.CreateTrigger(model);
            TriggerManager.RegisterTrigger(trigger);

            // Act
            var foundTrigger = TriggerManager.GetTrigger(trigger.Name);

            // Assert
            foundTrigger.Should().Be(trigger);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void GetTrigger_WithNonExistingName_ShouldReturnNull()
        {
            // Act
            var foundTrigger = TriggerManager.GetTrigger("NonExistentTrigger");

            // Assert
            foundTrigger.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Triggers")]
        public void Dispose_ShouldClearAllTriggers()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicTriggerModel(TriggerTypeEnum.Button);
            var trigger = TriggerManager.CreateTrigger(model);
            TriggerManager.RegisterTrigger(trigger);

            // Act
            TriggerManager.Dispose();

            // Assert
            TriggerManager.Triggers.Should().BeEmpty();
        }
    }
}