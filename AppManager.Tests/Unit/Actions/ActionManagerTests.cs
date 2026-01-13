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
            try
            {
                // Arrange
                var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");

                // Act
                ActionManager.ExecuteAction(model);
                Assert.IsTrue(true, "Action executed successfully.");
            }
            catch (Exception ex) 
            {
                Assert.Fail($"Execution failed with exception: {ex.Message}");
            }
            finally
            {
                var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc");
                ActionManager.ExecuteAction(closeModel);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public async Task ExecuteActionAsync_WithNullModel_ShouldReturnFalse()
        {
            try
            {
                // Act
                ActionManager.ExecuteAction((ActionModel)null!);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentNullException, "Expected ArgumentNullException was thrown.");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void ExecuteMultipleActionsAsync_WithMultipleActions_ShouldReturnTaskArray()
        {
            try
            {
                // Arrange
                var actions = TestDataBuilder.CreateMultipleActionModels(3);

                // Act
                ActionManager.ExecuteMultipleActions(actions);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Execution failed with exception: {ex.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void ExecuteMultipleActionsAsync_WithEmptyArray_ShouldReturnEmptyArray()
        {
            try
            {
                // Arrange
                var actions = Array.Empty<ActionModel>();

                // Act
                ActionManager.ExecuteMultipleActions(actions);
                Assert.IsTrue(true, "No actions to execute.");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Execution failed with exception: {ex.Message}");


            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public void CanExecuteAction_WithValidAppName_ShouldReturnBoolean()
        {
            // Act
            var result = ActionManager.CanExecuteAction(AppActionTypeEnum.Launch, "notepad");

            // Assert
            result.Should().Be(true);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Core")]
        public async Task ExecuteActionAsync_WithEnumAndAppName_ShouldExecuteAction()
        {
            try
            {
                ActionManager.ExecuteAction(AppActionTypeEnum.Launch, "calc");
            }
            catch(Exception e) 
            {
                Assert.Fail($"Execution failed with exception: {e.Message}");
            } 
            finally
            {
                ActionManager.ExecuteAction(AppActionTypeEnum.Close, "calc");
            }
        }
    }
}