using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Actions
{
    [TestClass]
    public class CloseActionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithValidModel_ShouldCreateInstance()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close);

            // Act
            var action = new CloseAction(model);

            // Assert
            action.Should().NotBeNull();
            action.ActionType.Should().Be(AppActionTypeEnum.Close);
            action.Description.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithWrongActionType_ShouldThrowArgumentException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);

            // Act & Assert
            Action act = () => new CloseAction(model);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*model type 'Launch' does not match trigger type 'Close'*");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void CanExecute_WithValidConfiguration_ShouldReturnTrue()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad");
            var action = new CloseAction(model);

            // Act
            var canExecute = action.CanExecute();

            // Assert
            canExecute.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithRunningProcess_ShouldCloseProcess()
        {
            // Arrange - First launch notepad
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad");
            var launchAction = new LaunchAction(launchModel);
            launchAction.Execute();
            
            // Wait for notepad to start
            await Task.Delay(1000);
            
            var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad");
            var closeAction = new CloseAction(closeModel);

            try
            {
                // Act
                closeAction.Execute();

                // Assert
                await Task.Delay(1000);
                var notepadProcesses = System.Diagnostics.Process.GetProcessesByName("notepad");
                notepadProcesses.Should().BeEmpty();
            }
            catch(Exception e)
            {
                Assert.Fail($"{e.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithNonExistentProcess_ShouldReturnFalse()
        {
            try
            {
                // Arrange
                var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "nonexistentapp");
                var action = new CloseAction(model);

                // Act
                action.Execute();
                Assert.IsTrue(true);
            }
            catch(Exception e)
            {
                // Assert
                Assert.Fail($"{e.Message}");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void ToModel_ShouldReturnCorrectActionModel()
        {
            // Arrange
            var originalModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "notepad");
            originalModel.TimeoutMs = 5000;
            originalModel.ForceOperation = true;
            var action = new CloseAction(originalModel);

            // Act
            var returnedModel = action.ToModel();

            // Assert
            returnedModel.Should().NotBeNull();
            returnedModel.ActionType.Should().Be(AppActionTypeEnum.Close);
            returnedModel.AppName.Should().Be(originalModel.AppName);
            returnedModel.TimeoutMs.Should().Be(originalModel.TimeoutMs);
            returnedModel.ForceOperation.Should().Be(originalModel.ForceOperation);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Properties_ShouldBeSetCorrectlyFromModel()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "test");
            model.TimeoutMs = 3000;
            model.ForceOperation = true;
            model.IncludeChildProcesses = true;
            model.IncludeSimilarNames = false;

            // Act
            var action = new CloseAction(model);

            // Assert
            action.AppName.Should().Be("test");
            action.TimeoutMs.Should().Be(3000);
            action.ForceOperation.Should().BeTrue();
            action.IncludeChildProcesses.Should().BeTrue();
            action.IncludeSimilarNames.Should().BeFalse();
        }
    }
}