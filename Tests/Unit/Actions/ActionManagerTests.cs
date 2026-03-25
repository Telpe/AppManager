using AppManager.Core.Actions;
using AppManager.Core.Actions.Close;
using AppManager.Core.Actions.Launch;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            actions.Should().Contain(ActionTypeEnum.Launch);
            actions.Should().Contain(ActionTypeEnum.Close);
            actions.Should().Contain(ActionTypeEnum.Restart);
            actions.Should().Contain(ActionTypeEnum.Focus);
            actions.Should().Contain(ActionTypeEnum.BringToFront);
            actions.Should().Contain(ActionTypeEnum.Minimize);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithLaunchAction_ShouldReturnLaunchAction()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Launch);

            // Act
            var action = ActionFactory.CreateAction(model);

            // Assert
            action.Should().NotBeNull();
            action.Should().BeOfType<LaunchAction>();
            action.ActionType.Should().Be(ActionTypeEnum.Launch);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithCloseAction_ShouldReturnCloseAction()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Close);

            // Act
            var action = ActionFactory.CreateAction(model);

            // Assert
            action.Should().NotBeNull();
            action.Should().BeOfType<CloseAction>();
            action.ActionType.Should().Be(ActionTypeEnum.Close);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CreateAction_WithInvalidActionType_ShouldThrowException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel();
            model.ActionType = (ActionTypeEnum)999; // Invalid action type

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
            var result = ActionFactory.CreateAction(new() { ActionType = ActionTypeEnum.Launch, AppName = "notepad" }).CanExecute();

            // Assert
            result.Should().Be(true);
        }
    }
}