using AppManager.Core.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace AppManager.Core.Utils
{
    public static partial class FileManager
    {
        public static readonly string[] ExecuteableExtensions = { ".bat", ".cmd", ".exe", ".msi" };
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static readonly string IconfileDefault = "AppManagerIcon_temp.png";

        public static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppManager");

        public static readonly string OSShortcutsPath = Path.Combine(AppDataPath, "Shortcuts");

        // JSON Operations
        public static T LoadJsonFile<T>(string filePath) where T : new()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? new T();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading JSON file {filePath}: {ex.Message}");
            }
            return new T();
        }

        public static bool SaveJsonFile<T>(T data, string filePath)
        {
            try
            {
                Directory.CreateDirectory( Path.GetDirectoryName(filePath) ?? throw new ArgumentNullException("filePath can not be null."));
                var json = JsonSerializer.Serialize(data, JsonOptions);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving JSON file {filePath}: {ex.Message}");
                return false;
            }
        }

        // File Dialog Operations
        public static string ShowOpenFileDialog(string filter = "All files (*.*)|*.*", string title = "Select File")
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Title = title
            };
            return dialog.ShowDialog() == true ? dialog.FileName : String.Empty;
        }

        public static string ShowSaveFileDialog(string filter = "All files (*.*)|*.*", string title = "Save File")
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = filter,
                Title = title
            };
            return dialog.ShowDialog() == true ? dialog.FileName : String.Empty;
        }

        // File System Operations
        public static bool FileExists(string path) => !string.IsNullOrEmpty(path) && File.Exists(path);
        
        public static string[] FindExecutables(string appName, string[]? searchPaths = null, bool slobbySearch = false, bool? includeAllDirectories = null)
        {
            searchPaths ??= GetDefaultSearchPaths();
            var results = new List<string>();
            bool nameExtended = ExecuteableExtensions.Any(a=> appName.EndsWith(a, true, null));

            foreach (string basePath in searchPaths)
            {
                try
                {
                    if (nameExtended)
                    {
                        if (slobbySearch)
                        {
                            results.AddRange(Directory.GetFiles(
                                basePath,
                                $"*{appName}", 
                                false != includeAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                                ));
                        }
                        else
                        {
                            results.AddRange(Directory.GetFiles(
                                basePath,
                                $"{appName}",
                                true != includeAllDirectories ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories
                                ));
                        }
                    }

                    foreach (string ext in ExecuteableExtensions)
                    {
                        if (slobbySearch)
                        {
                            results.AddRange(Directory.GetFiles(
                                basePath,
                                $"*{appName}*{ext}",
                                false != includeAllDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                                ));
                        }
                        else
                        {
                            results.AddRange(Directory.GetFiles(
                                basePath,
                                $"{appName}{ext}",
                                true != includeAllDirectories ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories
                                ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error searching in {basePath}: {ex.Message}");
                }
            }

            return results.Distinct().ToArray();
        }

        // Browser Shortcuts Operations
        public static List<OSShortcutModel> GetOSShortcuts()
        {
            var shortcuts = new List<OSShortcutModel>();

            try
            {
                // Ensure the BrowserShortcuts directory exists
                Directory.CreateDirectory(OSShortcutsPath);

                string[] executableExtensions = { ".bat", ".lnk", ".cmd", ".exe" };
                string[] iconExtensions = { ".ico", ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

                var executableFiles = new List<string>();

                // Find all executable files
                foreach (string ext in executableExtensions)
                {
                    var files = Directory.GetFiles(OSShortcutsPath, $"*{ext}", SearchOption.TopDirectoryOnly);
                    executableFiles.AddRange(files);
                }

                // Process each executable file
                foreach (string executablePath in executableFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(executablePath);
                    var extension = Path.GetExtension(executablePath);
                    
                    // Try to extract icon source
                    ImageSource? iconSource = ExtractIconSource(executablePath, fileName, iconExtensions);

                    if (null == iconSource)
                    {
                        iconSource = GetShellIcon();
                    }

                    shortcuts.Add(new OSShortcutModel
                    {
                        Name = fileName,
                        ExecutablePath = executablePath,
                        IconSource = iconSource,
                        FileExtension = extension
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting OS shortcuts: {ex.Message}");
            }

            return shortcuts.OrderBy(s => s.Name).ToList();
        }

        public static Icon ConvertBitmapSourceToIcon(System.Windows.Media.Imaging.BitmapSource bitmapSource)
        {
            try
            {
                if (bitmapSource == null)
                    return SystemIcons.Application;

                using (var bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight))
                {
                    var data = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.WriteOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    bitmapSource.CopyPixels(
                        new System.Windows.Int32Rect(0, 0, bitmapSource.PixelWidth, bitmapSource.PixelHeight),
                        data.Scan0,
                        data.Stride * bitmap.Height,
                        data.Stride);

                    bitmap.UnlockBits(data);
                    return Icon.FromHandle(bitmap.GetHicon());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting BitmapSource to Icon: {ex.Message}");
                return SystemIcons.Application;
            }
        }

        private static ImageSource? ExtractIconSource(string executablePath, string baseName, string[] iconExtensions)
        {
            try
            {
                // First, try to find a standalone icon file with the same name
                string? iconPath = FindIconFileForExecutable(baseName, iconExtensions);
                if (!string.IsNullOrEmpty(iconPath)) { return CreateImageSourceFromFile(iconPath); }

                // Second, try to extract icon from executable (for .exe files)
                if (Path.GetExtension(executablePath).Equals(".exe", StringComparison.OrdinalIgnoreCase)) { return ExtractBitmapSourceFromExecutable(executablePath); }

                // Third, try to extract icon from .lnk files
                if (Path.GetExtension(executablePath).Equals(".lnk", StringComparison.OrdinalIgnoreCase)) { return ExtractIconFromShortcut(executablePath); }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting icon for {executablePath}: {ex.Message}");
            }

            return null;
        }

        private static string? FindIconFileForExecutable(string baseName, string[] iconExtensions)
        {
            foreach (string ext in iconExtensions)
            {
                string iconPath = Path.Combine(OSShortcutsPath, $"{baseName}{ext}");
                if (File.Exists(iconPath)) { return iconPath; }
            }
            return null;
        }

        /// <summary>
        /// Creates an ImageSource from image file bytes
        /// </summary>
        public static ImageSource CreateImageSourceFromBytes(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) { throw new ArgumentException("Image data cannot be null or empty", nameof(imageData)); }

            try
            {
                using var stream = new MemoryStream(imageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // Make it thread-safe
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating ImageSource from bytes: {ex.Message}");
                throw new InvalidOperationException("Failed to create ImageSource from bytes", ex);
            }
        }

        /// <summary>
        /// Creates an ImageSource from an image file path
        /// </summary>
        public static ImageSource CreateImageSourceFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) 
                { 
                    throw new FileNotFoundException($"Image file not found: {filePath}", filePath); 
                }

                byte[] imageData = File.ReadAllBytes(filePath);
                return CreateImageSourceFromBytes(imageData);
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating ImageSource from file {filePath}: {ex.Message}");
                throw new InvalidOperationException($"Failed to create ImageSource from file: {filePath}", ex);
            }
        }

        public static BitmapSource ExtractBitmapSourceFromExecutable(string executablePath)
        {
            try
            {
                Icon? icon = Icon.ExtractIcon(executablePath, 0, false);

                if (icon != null)
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze(); // Make it thread-safe
                    return bitmapSource;
                }
                else
                {
                    return CreateArmyBlueFallbackIcon();
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting icon from executable {executablePath}: {ex.Message}");
                throw new InvalidOperationException($"Failed to extract icon from executable: {executablePath}", ex);
            }
        }

        public static ImageSource ExtractIconFromShortcut(string shortcutPath)
        {
            try
            {
                // For .lnk files, we need to resolve the target and extract its icon
                string targetPath = ResolveShortcutTarget(shortcutPath);
                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath)) 
                { 
                    return ExtractBitmapSourceFromExecutable(targetPath); 
                }
                
                throw new InvalidOperationException($"Failed to resolve shortcut target or target does not exist: {shortcutPath}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting icon from shortcut {shortcutPath}: {ex.Message}");
                throw new InvalidOperationException($"Failed to extract icon from shortcut: {shortcutPath}", ex);
            }
        }

        public static string ResolveShortcutTarget(string shortcutPath)
        {
            try
            {
                // Simple shortcut resolution using Shell COM object
                Type shellType = Type.GetTypeFromProgID("WScript.Shell") ?? throw new InvalidOperationException("WScript.Shell COM object not available");
                dynamic shell = Activator.CreateInstance(shellType) ?? throw new Exception("Instance dynamic shell is null.");
                var shortcut = shell.CreateShortcut(shortcutPath);
                string targetPath = shortcut.TargetPath;
                Marshal.ReleaseComObject(shortcut);
                Marshal.ReleaseComObject(shell);
                
                return !String.IsNullOrEmpty(targetPath) ? targetPath : throw new InvalidOperationException($"Shortcut does not contain a valid target path: {shortcutPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resolving shortcut target for {shortcutPath}: {ex.Message}");
                throw new InvalidOperationException($"Failed to resolve shortcut target: {shortcutPath}", ex);
            }
        }

        public static bool ExecuteFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"File not found: {filePath}");
                    return false;
                }

                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(filePath)
                };

                System.Diagnostics.Process.Start(processStartInfo);
                System.Diagnostics.Debug.WriteLine($"Successfully executed: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing file {filePath}: {ex.Message}");
                return false;
            }
        }

        //private const uint SHGFI_ICON = 0x100;
        /// <summary>
        /// private const uint SHGFI_LARGEICON = 0x0;
        /// </summary>
        //private const uint SHGFI_SMALLICON = 0x1;

        private static string[] GetDefaultSearchPaths()
        {
#if DEBUG
            string[] otherPaths = AppDomain.CurrentDomain.BaseDirectory.Split(Path.DirectorySeparatorChar);
            int iop = Array.IndexOf(otherPaths, "AppManager");
            var basePath = Path.Combine( otherPaths.Where((a,i)=> i <= iop).ToArray() );
            var devCorePath = Path.Combine(basePath, "AppManager.Core", "bin", "Debug", "net8.0-windows");
            var devSettingsPath = Path.Combine(basePath, "AppManager.Settings", "bin", "Debug", "net8.0-windows");
            var devAppManagerPath = Path.Combine(basePath, "AppManager", "bin", "Debug", "net8.0-windows");
#endif
            //var pathEnv = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
            var commonPaths = new[]
            {
#if DEBUG
                devCorePath, devSettingsPath,devAppManagerPath,
#endif
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            };
            
            return commonPaths;
        }

        public static string GetProfilePath(string profileName = "default") =>
            Path.Combine(AppDataPath, $"{profileName}.json");

        public static string GetSettingsPath() =>
            Path.Combine(AppDataPath, "settings.json");

        public static string GetBrowserShortcutsPath() => OSShortcutsPath;

        /// <summary>
        /// Extracts a shell icon from shell32.dll
        /// </summary>
        /// <param name="iconIndex">The icon index (use negative values for resource IDs)</param>
        /// <returns>ImageSource of the shell icon or null if extraction fails</returns>
        public static ImageSource GetShellIcon(int iconIndex = -16769)
        {
            try
            {
                string shell32Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");

                Icon? icon = Icon.ExtractIcon(shell32Path, iconIndex, false);

                if (icon != null)
                {
                    var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    bitmapSource.Freeze(); // Make it thread-safe
                    return bitmapSource;
                }
                else
                {
                    return CreateArmyBlueFallbackIcon();
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting shell icon {iconIndex}: {ex.Message}");
                throw new InvalidOperationException($"Failed to extract shell icon with index {iconIndex}", ex);
            }
        }

        /// <summary>
        /// Gets the path to version.json in the assembly root directory
        /// </summary>
        public static string GetVersionFilePath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json");

        /// <summary>
        /// Loads version information from version.json file
        /// </summary>
        /// <returns>Version struct or default if file doesn't exist</returns>
        public static Version LoadVersion()
        {
            try
            {
                string versionFilePath = GetVersionFilePath();
                
                if (!File.Exists(versionFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"version.json not found at {versionFilePath}, using default version");
                    return new Version { Exspansion = 0, Patch = 0, Hotfix = 0, Work = 1 };
                }

                return LoadJsonFile<Version>(versionFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading version: {ex.Message}");
                return new Version { Exspansion = 0, Patch = 0, Hotfix = 0, Work = 1 };
            }
        }

        /// <summary>
        /// Converts an ImageSource to a BitmapSource
        /// </summary>
        /// <param name="imageSource">The ImageSource to convert</param>
        /// <returns>A BitmapSource representation of the ImageSource</returns>
        public static BitmapSource ImageSourceToBitmapSource(ImageSource imageSource)
        {
            if (imageSource == null)
            {
                throw new ArgumentNullException(nameof(imageSource), "ImageSource cannot be null");
            }

            try
            {
                // If it's already a BitmapSource, return it directly
                if (imageSource is BitmapSource bitmapSource)
                {
                    return bitmapSource;
                }

                // For DrawingImage types, render to RenderTargetBitmap
                if (imageSource is DrawingImage drawingImage)
                {
                    var drawing = drawingImage.Drawing;
                    var bounds = drawing.Bounds;

                    var targetBitmap = new RenderTargetBitmap(
                        (int)bounds.Width,
                        (int)bounds.Height,
                        96,
                        96,
                        PixelFormats.Pbgra32);

                    var drawingVisual = new DrawingVisual();
                    using (var context = drawingVisual.RenderOpen())
                    {
                        context.DrawImage(drawingImage, bounds);
                    }

                    targetBitmap.Render(drawingVisual);
                    targetBitmap.Freeze();
                    return targetBitmap;
                }

                throw new InvalidOperationException($"Unsupported ImageSource type: {imageSource.GetType().Name}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting ImageSource to BitmapSource: {ex.Message}");
                throw new InvalidOperationException("Failed to convert ImageSource to BitmapSource", ex);
            }
        }

        public static Icon GetDefaultIcon()
        {
            try
            {
                // First, try to load custom icon from PNG file
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IconfileDefault);
                if (File.Exists(iconPath))
                {
                    return FileManager.ConvertBitmapSourceToIcon(FileManager.ImageSourceToBitmapSource(FileManager.CreateImageSourceFromFile(iconPath)));
                }

                // Second, try to extract icon from AppManager.exe
                var appManagerPath = FileManager.FindExecutables("AppManager.exe").FirstOrDefault();
                if (!string.IsNullOrEmpty(appManagerPath))
                {
                    var bitmapSource = FileManager.ExtractBitmapSourceFromExecutable(appManagerPath);
                    return FileManager.ConvertBitmapSourceToIcon(bitmapSource);
                }

                // Fallback to system icon
                Debug.WriteLine("Failed to load icon, using system fallback");
                return SystemIcons.Application;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading tray icon: {ex.Message}");
                return SystemIcons.Application;
            }
        }

        /// <summary>
        /// Creates a fallback icon in semi-transparent army-blue color
        /// </summary>
        /// <returns>A 16x16 BitmapSource in semi-transparent army-blue</returns>
        private static BitmapSource CreateArmyBlueFallbackIcon()
        {
            const int size = 16;
            const int dpiX = 96;
            const int dpiY = 96;

            // Create a RenderTargetBitmap for the icon
            var bitmap = new RenderTargetBitmap(size, size, dpiX, dpiY, PixelFormats.Pbgra32);

            // Create a DrawingVisual to draw on
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                // Army blue color with semi-transparency: RGB(75, 100, 130) with 70% opacity
                var armyBlueBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(178, 75, 100, 130));
                drawingContext.DrawRectangle(armyBlueBrush, null, new Rect(0, 0, size, size));
            }

            // Render the visual onto the bitmap
            bitmap.Render(drawingVisual);
            bitmap.Freeze(); // Make it thread-safe

            return bitmap;
        }
    }
}