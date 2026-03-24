using AppManager.OsApi.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppManager.OsApi.Tests.Exceptions
{
    [TestClass]
    public class NotImplementedForOsExceptionTests
    {
        [TestMethod]
        public void DefaultConstructor_ShouldHaveDefaultMessage()
        {
            // Act
            var exception = new NotImplementedForOsException();

            // Assert
            Assert.AreEqual("This method has not been implemented or tested for the current operating system.", exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void MessageConstructor_ShouldUseProvidedMessage()
        {
            // Arrange
            string customMessage = "Custom error message";

            // Act
            var exception = new NotImplementedForOsException(customMessage);

            // Assert
            Assert.AreEqual(customMessage, exception.Message);
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void MessageAndInnerExceptionConstructor_ShouldUseProvidedValues()
        {
            // Arrange
            string customMessage = "Custom error message";
            var innerException = new ArgumentException("Inner exception");

            // Act
            var exception = new NotImplementedForOsException(customMessage, innerException);

            // Assert
            Assert.AreEqual(customMessage, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [TestMethod]
        public void InnerExceptionOnlyConstructor_ShouldUseDefaultMessageWithInnerException()
        {
            // Arrange
            var innerException = new ArgumentException("Inner exception");

            // Act
            var exception = new NotImplementedForOsException(innerException);

            // Assert
            Assert.AreEqual("This method has not been implemented or tested for the current operating system. See inner exception for details.", exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [TestMethod]
        public void MethodNameAndOsConstructor_ShouldCreateFormattedMessage()
        {
            // Arrange
            string methodName = "TestMethod";
            string osName = "Linux";

            // Act
            var exception = new NotImplementedForOsException(methodName, osName);

            // Assert
            Assert.AreEqual($"Method '{methodName}' has not been implemented or tested for {osName}.", exception.Message);
            Assert.IsNull(exception.InnerException);
        }
    }
}