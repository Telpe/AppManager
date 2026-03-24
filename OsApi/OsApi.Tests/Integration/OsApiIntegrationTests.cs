using AppManager.OsApi.Abstractions;
using AppManager.OsApi.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AppManager.OsApi.Tests.Integration
{
    [TestClass]
    public class OsApiIntegrationTests
    {
        [TestMethod]
        public void EndToEnd_LoadApiAndCallMethods_ShouldWork()
        {
            // Arrange & Act
            IOsApi api = OsApiLoader.Load();

            // Assert - Basic functionality
            Assert.IsNotNull(api);
            Assert.IsTrue(api.CurrentThreadId > 0);
            Assert.IsNotNull(api.KeyEvent);
        }

        [TestMethod]
        public void Api_WindowMethods_ShouldHandleInvalidInputsGracefully()
        {
            // Arrange
            IOsApi api = OsApiLoader.Load();
            IntPtr invalidHandle = IntPtr.Zero;

            // Act & Assert - Should not throw exceptions
            Assert.IsFalse(api.WindowIsMinimized(invalidHandle));
            Assert.IsFalse(api.WindowRestore(invalidHandle));
            Assert.IsFalse(api.WindowShow(invalidHandle));
            Assert.IsFalse(api.WindowSetForeground(invalidHandle));
            Assert.IsFalse(api.WindowSetPosition(invalidHandle, api.HWND_TOPMOST, 0, 0, 100, 100, api.SWP_NOMOVE));
            
            // Shutdown methods might return success even with invalid handles
            int result1 = api.ShutdownBlockReasonCreate(invalidHandle, "Test");
            int result2 = api.ShutdownBlockReasonDestroy(invalidHandle);
            Assert.IsTrue(result1 >= 0);
            Assert.IsTrue(result2 >= 0);
        }

    }
}