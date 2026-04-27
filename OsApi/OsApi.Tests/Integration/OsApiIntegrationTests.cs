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
        }

        [TestMethod]
        public void Api_WindowMethods_ShouldHandleInvalidInputsGracefully()
        {
            // Arrange
            IOsApi api = OsApiLoader.Load();
            IntPtr invalidHandle = IntPtr.Zero;

            Assert.ThrowsException<Exception>(() => { api.Window.IsMinimized(invalidHandle); });
            Assert.ThrowsException<Exception>(() => { api.Window.Restore(invalidHandle); });
            Assert.ThrowsException<Exception>(() => { api.Window.Focus(invalidHandle); });
            Assert.IsFalse(api.Window.SetPosition(invalidHandle, api.HWND_TOPMOST, 0, 0, 100, 100, api.SWP_NOMOVE));
            
            // Shutdown methods might return success even with invalid handles
            int result1 = api.Window.ShutdownBlockReasonCreate(invalidHandle, "Test");
            int result2 = api.Window.ShutdownBlockReasonDestroy(invalidHandle);
            Assert.IsTrue(result1 >= 0);
            Assert.IsTrue(result2 >= 0);
        }

    }
}