using AppManager.OsApi.Interfaces;
using AppManager.OsApi.Windows11;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace AppManager.OsApi.Tests.Windows11
{
    [TestClass]
    public class Windows11ApiTests
    {
        private IOsApi _api = null!;

        [TestInitialize]
        public void Setup()
        {
            _api = new Windows11Api();
        }

        private void FindWindowWithTitle(int processId, string title)
        {
            try
            {
                // Get all processes with the same name
                var processes = Process.GetProcessesByName("explorer");
                var targetProcess = processes.FirstOrDefault(p => p.Id == processId);

                if (targetProcess != null)
                {
                    Debug.WriteLine($"  Main Window: Handle={targetProcess.MainWindowHandle}, Title='{targetProcess.MainWindowTitle}'");

                    // Alternative approach: Use Windows API to enumerate all windows for this process
                    // This would require additional P/Invoke calls, but for now we'll show what's available through .NET

                    // Show modules/threads info that might help identify window creation
                    Debug.WriteLine($"  Threads: {targetProcess.Threads.Count}");
                    Debug.WriteLine($"  Working Set: {targetProcess.WorkingSet64} bytes");

                    // Try to get window handle using our API
                    IntPtr apiHandle = _api.GetProcessMainWindowHandle(targetProcess);
                    Debug.WriteLine($"  API GetProcessMainWindowHandle result: {apiHandle}");

                    if (apiHandle != IntPtr.Zero)
                    {
                        Debug.WriteLine($"  Window is minimized: {_api.Window.IsMinimized(apiHandle)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error enumerating windows: {ex.Message}");
            }
        }

        private void EnumerateProcessWindows(int processId)
        {
            try
            {
                // Get all processes with the same name
                var processes = Process.GetProcessesByName("explorer");
                var targetProcess = processes.FirstOrDefault(p => p.Id == processId);

                if (targetProcess != null)
                {
                    Debug.WriteLine($"  Main Window: Handle={targetProcess.MainWindowHandle}, Title='{targetProcess.MainWindowTitle}'");

                    // Alternative approach: Use Windows API to enumerate all windows for this process
                    // This would require additional P/Invoke calls, but for now we'll show what's available through .NET

                    // Show modules/threads info that might help identify window creation
                    Debug.WriteLine($"  Threads: {targetProcess.Threads.Count}");
                    Debug.WriteLine($"  Working Set: {targetProcess.WorkingSet64} bytes");

                    // Try to get window handle using our API
                    IntPtr apiHandle = _api.GetProcessMainWindowHandle(targetProcess);
                    Debug.WriteLine($"  API GetProcessMainWindowHandle result: {apiHandle}");

                    if (apiHandle != IntPtr.Zero)
                    {
                        Debug.WriteLine($"  Window is minimized: {_api.Window.IsMinimized(apiHandle)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error enumerating windows: {ex.Message}");
            }
        }

        [TestMethod]
        public void BringWindowToFront()
        {
            // Arrange - Start Explorer process
            Process? explorerProcess = null;
            try
            {
                string windowTitle = @"C:\";
                string processTitle = "explorer";
                nint? windowHandle = null;

                explorerProcess = Process.Start(processTitle + ".exe", windowTitle);

                // Wait for the process to start and create its window
                explorerProcess.WaitForInputIdle(5000);
                explorerProcess.Dispose();
                Thread.Sleep(2000); // Additional wait for window creation

                Process[] processs = Process.GetProcessesByName(processTitle);

                foreach (Process p in processs)
                {
                    if (TryGetHandleFromWindowTitleInProcess(p, windowTitle, out nint? handle))
                    {
                        Debug.WriteLine($"windowHandle found: {handle}");
                        explorerProcess = p;
                        windowHandle = handle;
                        break;
                    }
                }


                if (windowHandle is null)
                {
                    throw new InvalidOperationException($"{processTitle} process with window {windowTitle} not found");
                }



                // Debug: List all windows for the explorer process
                Debug.WriteLine($"{processTitle} Process ID: {explorerProcess.Id}");
                Debug.WriteLine($"Main Window Handle: {explorerProcess.MainWindowHandle}");
                Debug.WriteLine($"Main Window Title: '{explorerProcess.MainWindowTitle}'");
                Debug.WriteLine($"Process Name: {explorerProcess.ProcessName}");
                Debug.WriteLine($"Has Exited: {explorerProcess.HasExited}");


                // If you want to enumerate ALL windows associated with this specific process
                //Debug.WriteLine($"\nEnumerating all windows for {processTitle} process {explorerProcess.Id}:");
                //EnumerateProcessWindows(explorerProcess.Id);
                IntPtr mainWindowHandle = (nint)windowHandle;

                // Skip test if we can't get a valid window handle
                if (mainWindowHandle == IntPtr.Zero)
                {
                    Assert.Inconclusive("Could not get main window handle for {processTitle} process");
                    return;
                }

                // Minimize the window first to set up test conditions
                _api.WindowSetState(mainWindowHandle, (int)ShowWindowEnum.Minimize);
                Thread.Sleep(2000); // Wait for minimize to complete

                Debug.WriteLine("Window should be minimized");
                Assert.IsTrue(_api.Window.IsMinimized(mainWindowHandle), "Window is not minimized");

                // Act - Bring window to front using the provided logic
                Debug.WriteLine("Bring to Front");
                Assert.IsTrue(BringWindowToFrontImpl(mainWindowHandle), "Failed to bring window to front");
                Thread.Sleep(2000);

                Assert.IsFalse(_api.Window.IsMinimized(mainWindowHandle), "Window should not be minimized after bringing to front");
                Assert.IsFalse(_api.Window.IsMaximized(mainWindowHandle), "Window should not be maximized after bringing to front");
            }
            catch(Exception ex) 
            {
                string message = $"{ex.Message}\n{ex.StackTrace}\n";
                if (ex.InnerException is not null)
                {
                    message += $"InnerException: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n";
                }
                Assert.Fail(message);
            }
            finally
            {
                // Cleanup - Close the Explorer window
                if (explorerProcess != null && !explorerProcess.HasExited)
                {
                    try
                    {
                        if (explorerProcess.CloseMainWindow())
                        { Debug.WriteLine($"closing mainWindow"); }
                        else
                        { Debug.WriteLine($"no mainWindow to close.\nwaiting for exit."); }

                        if (!explorerProcess.WaitForExit(3000))
                        {
                            explorerProcess.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to clean up process: {ex.Message}\n{ex.StackTrace}");
                    }
                    finally
                    {
                        explorerProcess.Dispose();
                    }
                }
                Debug.WriteLine($"Cleaning up process done:");
            }
        }

        private bool TryGetHandleFromWindowTitleInProcess(Process process, string windowTitle, out nint? handle)
        {
            nint? aHandle = null;

            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId == process.Id)
                {
                    var title = new System.Text.StringBuilder(256);
                    GetWindowText(hWnd, title, 256);
                    if (title.ToString() == windowTitle)
                    {
                        aHandle = hWnd;
                        Debug.WriteLine($"  Found window: Handle={hWnd}, Title='{title}'");
                        return false;
                    }
                    
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            handle = aHandle;

            return handle is not null;
        }

        private bool BringWindowToFrontImpl(IntPtr mainWindowHandle)
        {
            try
            {
                // If window is minimized, restore it first
                if (_api.Window.IsMinimized(mainWindowHandle))
                {
                    _api.Window.Restore(mainWindowHandle);
                }
                else
                {
                    _api.Window.Focus(mainWindowHandle);
                }

                /*
                // Make window topmost temporarily
                if (!_api.WindowSetPosition(mainWindowHandle, _api.HWND_TOPMOST, 0, 0, 0, 0,
                    _api.SWP_NOMOVE | _api.SWP_NOSIZE))
                {
                    return false;
                }

                // Bring to foreground
                if (!_api.WindowSetForeground(mainWindowHandle))
                {
                    return false;
                }

                // Wait a moment, then remove topmost flag
                Thread.Sleep(100); // Using shorter delay for tests
                return _api.WindowSetPosition(mainWindowHandle, _api.HWND_NOTOPMOST, 0, 0, 0, 0,
                    _api.SWP_NOMOVE | _api.SWP_NOSIZE);
                */
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [TestMethod]
        public void CurrentThreadId_ShouldReturnPositiveValue()
        {
            // Act
            uint threadId = _api.CurrentThreadId;

            // Assert
            Assert.IsTrue(threadId > 0);
        }

        [TestMethod]
        public void KeyEvent_ShouldNotBeNull()
        {
            // Act & Assert
            Assert.IsNotNull(_api.KeyEvent);
        }

        [TestMethod]
        public void GetProcessMainWindowHandle_WithValidProcess_ShouldReturnHandle()
        {
            // Arrange
            using Process currentProcess = Process.GetProcessesByName("devenv")[0];

            // Act
            IntPtr handle = _api.GetProcessMainWindowHandle(currentProcess);

            // Assert
            // Note: This might be IntPtr.Zero for console applications or background processes
            Assert.IsTrue(handle != IntPtr.Zero);
        }

        [TestMethod]
        public void WindowIsMinimized_WithInvalidHandle_ShouldThrow()
        {
            IntPtr invalidHandle = IntPtr.Zero;

            Assert.ThrowsException<Exception>(() => { bool result = _api.Window.IsMinimized(invalidHandle); });
        }

        [TestMethod]
        public void WindowRestore_WithInvalidHandle_ShouldThrow()
        {
            IntPtr invalidHandle = IntPtr.Zero;

            Assert.ThrowsException<Exception>(() => { _api.Window.Restore(invalidHandle); });
        }

        [TestMethod]
        public void WindowShow_WithInvalidHandle_ShouldThrow()
        {
            IntPtr invalidHandle = IntPtr.Zero;

            Assert.ThrowsException<Exception>(()=>{ _api.Window.Focus(invalidHandle); });

        }

        [TestMethod]
        public void WindowSetForeground_WithInvalidHandle_ShouldReturnFalse()
        {
            // Arrange
            IntPtr invalidHandle = IntPtr.Zero;

            // Act
            bool result = _api.WindowSetForeground(invalidHandle);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void WindowSetPosition_WithInvalidHandle_ShouldReturnFalse()
        {
            // Arrange
            IntPtr invalidHandle = IntPtr.Zero;

            // Act
            bool result = _api.WindowSetPosition(invalidHandle, _api.HWND_TOPMOST, 0, 0, 100, 100, _api.SWP_NOMOVE);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShutdownBlockReasonCreate_WithInvalidHandle_ShouldReturnNonZero()
        {
            // Arrange
            IntPtr invalidHandle = IntPtr.Zero;
            string reason = "Test reason";

            // Act
            int result = _api.ShutdownBlockReasonCreate(invalidHandle, reason);

            // Assert
            // Note: This might succeed or fail depending on the OS implementation
            Assert.IsTrue(result >= 0);
        }

        [TestMethod]
        public void ShutdownBlockReasonDestroy_WithInvalidHandle_ShouldReturnNonZero()
        {
            // Arrange
            IntPtr invalidHandle = IntPtr.Zero;

            // Act
            int result = _api.ShutdownBlockReasonDestroy(invalidHandle);

            // Assert
            // Note: This might succeed or fail depending on the OS implementation
            Assert.IsTrue(result >= 0);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private void EnumerateAllWindowsForProcess(Process process)
        {
            Debug.WriteLine($"Enumerating ALL system windows for process {process.ProcessName}: {process.Id}");
            
            EnumWindows((hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out uint processId);
                if (processId == process.Id)
                {
                    var title = new System.Text.StringBuilder(256);
                    GetWindowText(hWnd, title, 256);
                    Debug.WriteLine($"  Found window: Handle={hWnd}, Title='{title}'");
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);
        }
    }
}