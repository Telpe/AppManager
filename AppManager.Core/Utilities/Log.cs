using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AppManager.Core.Utilities
{
    public static class Log
    {
        private static StreamWriter? _streamWriter;
        private static string[] buffer = [];
        private static object LogLock = new();
        private static int MaxLogs = 5;

        public static bool IsStraeming { get => null != _streamWriter; }

        private static void CleanupOldLogs()
        {
            try
            {
                string logDirectory = FileManager.LogsPath;
                string logFilePrefix = $"{AppDomain.CurrentDomain.FriendlyName}_";
                string logFilePattern = $"{logFilePrefix}*.log";

                string[] logFiles = Directory.GetFiles(logDirectory, logFilePattern);

                if (logFiles.Length <= MaxLogs)
                {
                    return;
                }

                // Sort by creation time descending (newest first)
                Array.Sort(logFiles, (x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));

                // Delete files starting from index MaxLogs
                for (int i = MaxLogs; i < logFiles.Length; i++)
                {
                    try
                    {
                        File.Delete(logFiles[i]);
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine($"Failed to delete log file {logFiles[i]}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Failed to cleanup old logs: {ex.Message}");
            }
        }

        public static void OpenStream()
        {
            lock (LogLock)
            {
                if (IsStraeming) { return; }

                Directory.CreateDirectory(FileManager.LogsPath);

                _streamWriter = new StreamWriter(Path.Combine(FileManager.LogsPath, $"{AppDomain.CurrentDomain.FriendlyName}_{GetTimestamp()}.log"), append: true)
                {
                    AutoFlush = true
                };

                if (0 < buffer.Length)
                {
                    foreach (var line in buffer)
                    {
                        _streamWriter.WriteLine(line);
                    }
                    buffer = [];
                }
            }

            _ = Task.Run(() =>
            {
                CleanupOldLogs();
            });
        }

        public static void CloseStream()
        {
            lock (LogLock)
            {
                _streamWriter?.Dispose();
            }
        }

        public static void WriteLine(string text)
        {
            lock (LogLock)
            {
                Debug.WriteLine(text);
                if (IsStraeming)
                {
                    _streamWriter!.WriteLine(text);
                }
                else
                {
                    buffer = [..buffer, text];
                }
            }
        }

        public static void Write(string text)
        {
            lock (LogLock)
            {
                Debug.Write(text);
                if (IsStraeming)
                {
                    _streamWriter!.Write(text);
                }
                else
                {
                    if (0 < buffer.Length)
                    {
                        buffer = [text];
                    }
                    else
                    {
                        buffer[^1] = buffer.Last() + text;
                    }   
                }
            }
        }

        public static void Dispose()
        {
            if (IsStraeming)
            {
                CloseStream();
                return;
            }

            if (0 < buffer.Length)
            {
                OpenStream();
                CloseStream();
            }
        }

        private static string GetTimestamp()
        {
            return DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss");
        }
    }
}
