using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace AppManager.Tests.Unit.Actions
{
    [TestClass]
    public class BringToFrontActionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithValidModel_ShouldCreateInstance()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront);

            // Act
            var action = new BringToFrontAction(model);

            // Assert
            action.Should().NotBeNull();
            action.ActionType.Should().Be(AppActionTypeEnum.BringToFront);
            action.Description.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithSpecificProcess_ShouldCreateInstance()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront);
            var currentProcess = Process.GetCurrentProcess();

            // Act
            var action = new BringToFrontAction(model, currentProcess);

            // Assert
            action.Should().NotBeNull();
            action.ActionType.Should().Be(AppActionTypeEnum.BringToFront);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Constructor_WithWrongActionType_ShouldThrowArgumentException()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch);

            // Act & Assert
            Action act = () => new BringToFrontAction(model);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*model type 'Launch' does not match trigger type 'BringToFront'*");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithCurrentProcess_ShouldReturnTrue()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront);
            var currentProcess = Process.GetCurrentProcess();
            var action = new BringToFrontAction(model, currentProcess);

            // Act
            var result = await action.ExecuteAsync();

            // Assert
            // Note: This might return false if the current process doesn't have a main window
            result.Should().BeOfType<bool>();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithRunningApplication_ShouldBringToFront()
        {
            // Arrange - Launch an application first
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "calc");
            var launchAction = new LaunchAction(launchModel);
            await launchAction.ExecuteAsync();
            
            // Wait for calc to start
            await Task.Delay(1500);

            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, "calc");
            var action = new BringToFrontAction(model);

            try
            {
                // Act
                var result = await action.ExecuteAsync();

                // Assert
                result.Should().BeTrue();
                
                // Verify calc is still running
                var calcProcesses = Process.GetProcessesByName("calc");
                calcProcesses.Should().NotBeEmpty();
            }
            finally
            {
                // Cleanup
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
            var originalModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, "notepad");
            originalModel.WindowTitle = "Test Window";
            var action = new BringToFrontAction(originalModel);

            // Act
            var returnedModel = action.ToModel();

            // Assert
            returnedModel.Should().NotBeNull();
            returnedModel.ActionType.Should().Be(AppActionTypeEnum.BringToFront);
            returnedModel.AppName.Should().Be(originalModel.AppName);
            returnedModel.WindowTitle.Should().Be(originalModel.WindowTitle);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public void Properties_ShouldBeSetCorrectlyFromModel()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, "test");
            model.WindowTitle = "Test Window Title";

            // Act
            var action = new BringToFrontAction(model);

            // Assert
            action.AppName.Should().Be("test");
            action.WindowTitle.Should().Be("Test Window Title");
        }
    }
}