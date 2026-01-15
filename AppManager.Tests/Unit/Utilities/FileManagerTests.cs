using AppManager.Core.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AppManager.Tests.Unit.Utilities
{
    [TestClass]
    public class FileManagerTests
    {
        private static string _testDirectory = string.Empty;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "AppManagerTests");
            Directory.CreateDirectory(_testDirectory);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                if (Directory.Exists(_testDirectory))
                {
                    Directory.Delete(_testDirectory, true);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FileExists_WithExistingFile_ShouldReturnTrue()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(testFile, "test content");

            // Act
            var result = FileManager.FileExists(testFile);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FileExists_WithNonExistentFile_ShouldReturnFalse()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "nonexistent.txt");

            // Act
            var result = FileManager.FileExists(testFile);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FileExists_WithEmptyPath_ShouldReturnFalse()
        {
            // Act
            var result = FileManager.FileExists(string.Empty);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FileExists_WithNullPath_ShouldReturnFalse()
        {
            // Act
            var result = FileManager.FileExists(null!);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FindExecutables_WithCommonExecutable_ShouldFindFiles()
        {
            // Act
            var results = FileManager.FindExecutables("notepad");

            // Assert
            results.Should().NotBeNull();
            results.Should().NotBeEmpty();
            results.Should().Contain(path => path.Contains("notepad.exe"));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void FindExecutables_WithNonExistentExecutable_ShouldReturnEmpty()
        {
            // Act
            var results = FileManager.FindExecutables("nonexistentapp12345");

            // Assert
            results.Should().NotBeNull();
            results.Should().BeEmpty();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void GetProfilePath_WithValidProfileName_ShouldReturnCorrectPath()
        {
            // Arrange
            string profileName = "TestProfile";

            // Act
            var path = FileManager.GetProfilePath(profileName);

            // Assert
            path.Should().NotBeNullOrEmpty();
            path.Should().EndWith($"{profileName}.json");
            path.Should().Contain("Profiles");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void GetProfilePath_WithInvalidProfileName_ShouldThrowArgumentException()
        {
            // Arrange
            string invalidProfileName = "Invalid<>Profile";

            // Act & Assert
            Action act = () => FileManager.GetProfilePath(invalidProfileName);
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Invalid profile name*");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void GetSettingsPath_ShouldReturnValidPath()
        {
            // Act
            var path = FileManager.GetSettingsPath();

            // Assert
            path.Should().NotBeNullOrEmpty();
            path.Should().EndWith("settings.json");
            path.Should().Contain("AppManager");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void LoadVersion_ShouldReturnVersionObject()
        {
            // Act
            var version = FileManager.LoadVersion();

            // Assert
            version.Should().NotBeNull();
            // Version should have some default values even if file doesn't exist
            (version.Exspansion >= 0).Should().BeTrue();
            (version.Patch >= 0).Should().BeTrue();
            (version.Hotfix >= 0).Should().BeTrue();
            (version.Work >= 0).Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void SaveJsonFile_WithValidData_ShouldCreateFile()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test.json");
            var testData = new { Name = "Test", Value = 42 };

            // Act
            var result = FileManager.SaveJsonFile(testData, testFile);

            // Assert
            result.Should().BeTrue();
            File.Exists(testFile).Should().BeTrue();
            
            // Verify content
            string content = File.ReadAllText(testFile);
            content.Should().Contain("Test");
            content.Should().Contain("42");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Utils")]
        public void LoadJsonFile_WithValidFile_ShouldDeserializeCorrectly()
        {
            // Arrange
            string testFile = Path.Combine(_testDirectory, "test_load.json");
            var testData = new { Name = "Test", Value = 42 };
            FileManager.SaveJsonFile(testData, testFile);

            // Act
            var loadedData = FileManager.LoadJsonFile<dynamic>(testFile);

            // Assert
            loadedData.Should().NotBeNull();
        }
    }
}