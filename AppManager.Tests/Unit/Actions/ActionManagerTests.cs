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
            var actions = ActionFactory.GetSupportedActionTypes();

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
            var action = ActionFactory.CreateAction(model);

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
            var action = ActionFactory.CreateAction(model);

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
            Action act = () => ActionFactory.CreateAction(model);
            act.Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CanExecuteAction_WithValidAppName_ShouldReturnBoolean()
        {
            // Act
            var result = ActionFactory.CreateAction(new() { ActionType = AppActionTypeEnum.Launch, AppName = "notepad" }).CanExecute();

            // Assert
            result.Should().Be(true);
        }
    }
}