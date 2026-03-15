using AppManager.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppManager.Tests.Unit.Models
{
    [TestClass]
    public class VersionTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ToString_ShouldReturnFormattedVersionString()
        {
            // Arrange
            var version = new AppManager.Core.Version
            {
                Exspansion = 1,
                Patch = 2,
                Hotfix = 3,
                Work = 4
            };

            // Act
            var result = version.ToString();

            // Assert
            result.Should().Be("1.2.3.4");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void Parse_WithValidVersionString_ShouldReturnCorrectVersion()
        {
            // Arrange
            string versionString = "1.2.3.4";

            // Act
            var version = AppManager.Core.Version.Parse(versionString);

            // Assert
            version.Exspansion.Should().Be(1);
            version.Patch.Should().Be(2);
            version.Hotfix.Should().Be(3);
            version.Work.Should().Be(4);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void Parse_WithPrefixedVersionString_ShouldReturnCorrectVersion()
        {
            // Arrange
            string versionString = "v1.2.3.4";

            // Act
            var version = AppManager.Core.Version.Parse(versionString);

            // Assert
            version.Exspansion.Should().Be(1);
            version.Patch.Should().Be(2);
            version.Hotfix.Should().Be(3);
            version.Work.Should().Be(4);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void GreaterThan_WithNewerVersion_ShouldReturnTrue()
        {
            // Arrange
            var v1 = new AppManager.Core.Version { Exspansion = 1, Patch = 2, Hotfix = 0, Work = 0 };
            var v2 = new AppManager.Core.Version { Exspansion = 1, Patch = 1, Hotfix = 0, Work = 0 };

            // Act & Assert
            (v1 > v2).Should().BeTrue();
            (v2 > v1).Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void Equals_WithSameVersion_ShouldReturnTrue()
        {
            // Arrange
            var v1 = new AppManager.Core.Version { Exspansion = 1, Patch = 2, Hotfix = 3, Work = 4 };
            var v2 = new AppManager.Core.Version { Exspansion = 1, Patch = 2, Hotfix = 3, Work = 4 };

            // Act & Assert
            (v1 == v2).Should().BeTrue();
            v1.Equals(v2).Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void ImplicitConversion_FromSystemVersion_ShouldConvertCorrectly()
        {
            // Arrange
            var systemVersion = new System.Version(1, 2, 3, 4);

            // Act
            AppManager.Core.Version appVersion = systemVersion;

            // Assert
            appVersion.Exspansion.Should().Be(1);
            appVersion.Patch.Should().Be(2);
            appVersion.Hotfix.Should().Be(3);
            appVersion.Work.Should().Be(4);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [TestCategory("Models")]
        public void GetHashCode_WithSameVersions_ShouldReturnSameHash()
        {
            // Arrange
            var v1 = new AppManager.Core.Version { Exspansion = 1, Patch = 2, Hotfix = 3, Work = 4 };
            var v2 = new AppManager.Core.Version { Exspansion = 1, Patch = 2, Hotfix = 3, Work = 4 };

            // Act & Assert
            v1.GetHashCode().Should().Be(v2.GetHashCode());
        }
    }
}