using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;

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
            try
            {
                // Arrange
                var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront);
                var currentProcess = Process.GetCurrentProcess();
                var action = new BringToFrontAction(model, currentProcess);

                // Act
                action.Execute();
            }
            catch
            {
                // Assert
                Assert.Fail("Bringing the process to front failed.");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Actions")]
        public async Task ExecuteAsync_WithRunningApplication_ShouldBringToFront()
        {
            // Arrange - Launch an application first
            var launchModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Launch, "CalculatorApp");
            var launchAction = new LaunchAction(launchModel);
            launchAction.Execute();
            
            // Wait for CalculatorApp to start
            Task.Delay(CoreConstants.DefaultActionDelay).Wait();

            var model = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.BringToFront, "CalculatorApp");
            var action = new BringToFrontAction(model);

            try
            {
                // Act
                action.Execute();

                // Assert
                //TODO: Verify that the application is actually brought to front.
            }
            finally
            {
                // Cleanup
                var closeModel = TestDataBuilder.CreateBasicActionModel(AppActionTypeEnum.Close, "CalculatorApp");
                var closeAction = new CloseAction(closeModel);
                closeAction.Execute();
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