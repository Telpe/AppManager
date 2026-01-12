using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Actions
{
    [TestClass]
    public class LaunchActionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithValidModel_ShouldCreateInstance()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);

            // Act
            var action = new LaunchAction(model);

            // Assert
            action.Should().NotBeNull();
            action.ActionType.Should().Be(AppActionTypeEnum.Launch);
            action.Description.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithWrongActionType_ShouldThrowArgumentException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close);

            // Act & Assert
            Action act = () => new LaunchAction(model);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*model type 'Close' does not match trigger type 'Launch'*");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void CanExecute_WithValidExecutablePath_ShouldReturnTrue()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad");
            var action = new LaunchAction(model);

            // Act
            var canExecute = action.CanExecute();

            // Assert
            canExecute.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void CanExecute_WithInvalidExecutablePath_ShouldReturnFalse()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);
            model.ExecutablePath = "nonexistent.exe";
            model.AppName = "nonexistent";
            var action = new LaunchAction(model);

            // Act
            var canExecute = action.CanExecute();

            // Assert
            canExecute.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithValidApplication_ShouldLaunchApplication()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");
            var action = new LaunchAction(model);

            try
            {
                // Act
                var result = await action.ExecuteAsync();

                // Assert
                result.Should().BeTrue();
                
                // Verify calc is running
                var calcProcesses = System.Diagnostics.Process.GetProcessesByName("calc");
                calcProcesses.Should().NotBeEmpty();
            }
            finally
            {
                // Cleanup - close calc
                var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "calc");
                var closeAction = new CloseAction(closeModel);
                await closeAction.ExecuteAsync();
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void ToModel_ShouldReturnCorrectActionModel()
        {
            // Arrange
            var originalModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "notepad");
            var action = new LaunchAction(originalModel);

            // Act
            var returnedModel = action.ToModel();

            // Assert
            returnedModel.Should().NotBeNull();
            returnedModel.ActionType.Should().Be(AppActionTypeEnum.Launch);
            returnedModel.AppName.Should().Be(originalModel.AppName);
            returnedModel.ExecutablePath.Should().Be(originalModel.ExecutablePath);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WhenCannotExecute_ShouldReturnFalse()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);
            model.ExecutablePath = "invalid_path.exe";
            model.AppName = "invalid";
            var action = new LaunchAction(model);

            // Act
            var result = await action.ExecuteAsync();

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Properties_ShouldBeSetCorrectlyFromModel()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "test");
            model.Arguments = "--test-mode";
            model.WorkingDirectory = @"C:\Test";

            // Act
            var action = new LaunchAction(model);

            // Assert
            action.AppName.Should().Be("test");
            action.Arguments.Should().Be("--test-mode");
            action.WorkingDirectory.Should().Be(@"C:\Test");
        }
    }
}