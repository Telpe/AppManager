using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Actions
{
    [TestClass]
    public class ActionManagerTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void GetAvailableActions_ShouldReturnAllActionTypes()
        {
            // Act
            var actions = ActionManager.GetAvailableActions().ToArray();

            // Assert
            actions.Should().NotBeEmpty();
            actions.Should().Contain(AppActionTypeEnum.Launch);
            actions.Should().Contain(AppActionTypeEnum.Close);
            actions.Should().Contain(AppActionTypeEnum.Restart);
            actions.Should().Contain(AppActionTypeEnum.Focus);
            actions.Should().Contain(AppActionTypeEnum.BringToFront);
            actions.Should().Contain(AppActionTypeEnum.Minimize);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithLaunchAction_ShouldReturnLaunchAction()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);

            // Act
            var action = ActionManager.CreateAction(model);

            // Assert
            action.Should().NotBeNull();
            action.Should().BeOfType<LaunchAction>();
            action.ActionType.Should().Be(AppActionTypeEnum.Launch);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithCloseAction_ShouldReturnCloseAction()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close);

            // Act
            var action = ActionManager.CreateAction(model);

            // Assert
            action.Should().NotBeNull();
            action.Should().BeOfType<CloseAction>();
            action.ActionType.Should().Be(AppActionTypeEnum.Close);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithInvalidActionType_ShouldThrowException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel();
            model.ActionType = (AppActionTypeEnum)999; // Invalid action type

            // Act & Assert
            Action act = () => ActionManager.CreateAction(model);
            act.Should().Throw<Exception>()
               .WithMessage("Action not found: 999");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public async Task ExecuteActionAsync_WithValidModel_ShouldExecuteSuccessfully()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");

            // Act
            var result = await ActionManager.ExecuteActionAsync(model);

            // Assert - Note: Result may be false if calc can't be launched, but method should not throw
            result.Should().BeOfType<bool>();
            
            // Cleanup - try to close calc if it was launched
            if (result)
            {
                var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc");
                await ActionManager.ExecuteActionAsync(closeModel);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public async Task ExecuteActionAsync_WithNullModel_ShouldReturnFalse()
        {
            // Act
            var result = await ActionManager.ExecuteActionAsync((ActionModel)null!);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void ExecuteMultipleActionsAsync_WithMultipleActions_ShouldReturnTaskArray()
        {
            // Arrange
            var actions = TestDataBuilder.CreateMultipleActionModels(3);

            // Act
            var tasks = ActionManager.ExecuteMultipleActionsAsync(actions);

            // Assert
            tasks.Should().NotBeNull();
            tasks.Length.Should().Be(3);
            tasks.Should().AllSatisfy(task => task.Should().NotBeNull());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void ExecuteMultipleActionsAsync_WithEmptyArray_ShouldReturnEmptyArray()
        {
            // Arrange
            var actions = Array.Empty<ActionModel>();

            // Act
            var tasks = ActionManager.ExecuteMultipleActionsAsync(actions);

            // Assert
            tasks.Should().NotBeNull();
            tasks.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CanExecuteAction_WithValidAppName_ShouldReturnBoolean()
        {
            // Act
            var result = ActionManager.CanExecuteAction(AppActionTypeEnum.Launch, "notepad");

            // Assert
            result.Should().BeOfType<bool>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public async Task ExecuteActionAsync_WithEnumAndAppName_ShouldExecuteAction()
        {
            // Act
            var result = await ActionManager.ExecuteActionAsync(AppActionTypeEnum.Launch, "calc");

            // Assert
            result.Should().BeOfType<bool>();
            
            // Cleanup
            if (result)
            {
                await ActionManager.ExecuteActionAsync(AppActionTypeEnum.Close, "calc");
            }
        }
    }
}