using AppManager.OsApi.Abstractions;
using AppManager.OsApi.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppManager.OsApi.Tests.Abstractions
{
    [TestClass]
    public class OsApiLoaderTests
    {
        [TestMethod]
        public void Load_ShouldReturnIOsApiInstance()
        {
            // Act
            IOsApi result = OsApiLoader.Load();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(IOsApi));
        }

        [TestMethod]
        public void Load_ShouldReturnWindows11Implementation()
        {
            // Act
            IOsApi result = OsApiLoader.Load();

            // Assert
            Assert.IsNotNull(result);
            string typeName = result.GetType().Name;
            Assert.IsTrue(typeName.Contains("Windows11"), $"Expected Windows11 implementation, but got {typeName}");
        }

        [TestMethod]
        public void Load_ShouldReturnSameTypeOnMultipleCalls()
        {
            // Act
            IOsApi first = OsApiLoader.Load();
            IOsApi second = OsApiLoader.Load();

            // Assert
            Assert.AreEqual(first.GetType(), second.GetType());
        }
    }
}