using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Models
{
    [TestClass]
    public class ActionModelTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ActionModel_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var model = new ActionModel();

            // Assert
            model.ActionType.Should().Be(default(AppActionTypeEnum));
            model.AppName.Should().BeNull();
            model.ExecutablePath.Should().BeNull();
            model.Arguments.Should().BeNull();
            model.WindowTitle.Should().BeNull();
            model.ForceOperation.Should().BeNull();
            model.IncludeChildProcesses.Should().BeNull();
            model.IncludeSimilarNames.Should().BeNull();
            model.TimeoutMs.Should().BeNull();
            model.Conditions.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ActionModel_WithAllProperties_ShouldSetCorrectly()
        {
            // Arrange & Act
            var model = new ActionModel
            {
                ActionType = AppActionTypeEnum.Launch,
                AppName = "TestApp",
                ExecutablePath = @"C:\Test\app.exe",
                Arguments = "--test",
                WindowTitle = "Test Window",
                ForceOperation = true,
                IncludeChildProcesses = false,
                IncludeSimilarNames = true,
                TimeoutMs = 5000,
                WorkingDirectory = @"C:\Test"
            };

            // Assert
            model.ActionType.Should().Be(AppActionTypeEnum.Launch);
            model.AppName.Should().Be("TestApp");
            model.ExecutablePath.Should().Be(@"C:\Test\app.exe");
            model.Arguments.Should().Be("--test");
            model.WindowTitle.Should().Be("Test Window");
            model.ForceOperation.Should().BeTrue();
            model.IncludeChildProcesses.Should().BeFalse();
            model.IncludeSimilarNames.Should().BeTrue();
            model.TimeoutMs.Should().Be(5000);
            model.WorkingDirectory.Should().Be(@"C:\Test");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ActionModel_WithConditions_ShouldSetCorrectly()
        {
            // Arrange
            var condition = TestDataBuilder.CreateBasicConditionModel();
            var conditions = new[] { condition };

            // Act
            var model = new ActionModel
            {
                ActionType = AppActionTypeEnum.Launch,
                AppName = "TestApp",
                Conditions = conditions
            };

            // Assert
            model.Conditions.Should().NotBeNull();
            model.Conditions.Should().HaveCount(1);
            model.Conditions![0].Should().Be(condition);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ActionModel_PropertyAssignments_ShouldMaintainValues()
        {
            // Arrange
            var model = TestDataBuilder.CreateBasicActionModel();

            // Act
            model.AppName = "UpdatedApp";
            model.ActionType = AppActionTypeEnum.Close;
            model.TimeoutMs = 10000;

            // Assert
            model.AppName.Should().Be("UpdatedApp");
            model.ActionType.Should().Be(AppActionTypeEnum.Close);
            model.TimeoutMs.Should().Be(10000);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ActionModel_NullableProperties_ShouldHandleNullValues()
        {
            // Arrange & Act
            var model = new ActionModel
            {
                ForceOperation = null,
                IncludeChildProcesses = null,
                IncludeSimilarNames = null,
                TimeoutMs = null
            };

            // Assert
            model.ForceOperation.Should().BeNull();
            model.IncludeChildProcesses.Should().BeNull();
            model.IncludeSimilarNames.Should().BeNull();
            model.TimeoutMs.Should().BeNull();
        }
    }
}