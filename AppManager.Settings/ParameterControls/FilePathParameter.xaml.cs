using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Utilities;
using Microsoft.Win32;

namespace AppManager.Settings.ParameterControls
{
    public partial class FilePathParameter : UserControl, INotifyPropertyChanged
    {
        private string? _filePath;

        public event EventHandler? FilePathChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    UpdateFilePathDisplay();
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string? FileFilter { get; set; } = "All Files (*.*)|*.*";
        public string? Title { get; set; } = "Select File";

        public FilePathParameter()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public FilePathParameter(string filePath) : this()
        {
            FilePath = filePath;
        }

        private void UpdateFilePathDisplay()
        {
            try
            {
                FilePathTextBox.Text = _filePath ?? string.Empty;
                UpdateFileStatus();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"UpdateFilePathDisplay error: {ex.Message}");
            }
        }

        private void UpdateFileStatus()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                FileStatusTextBlock.Text = "No file selected";
                FileStatusTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
            }
            else if (File.Exists(_filePath))
            {
                FileStatusTextBlock.Text = "File exists";
                FileStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                FileStatusTextBlock.Text = "File not found";
                FileStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = FileFilter ?? "All Files (*.*)|*.*",
                    Title = Title ?? "Select File",
                    CheckFileExists = false,
                    CheckPathExists = true
                };

                if (!string.IsNullOrWhiteSpace(_filePath))
                {
                    openFileDialog.FileName = Path.GetFileName(_filePath);
                    if (Directory.Exists(Path.GetDirectoryName(_filePath)))
                    {
                        openFileDialog.InitialDirectory = Path.GetDirectoryName(_filePath);
                    }
                }

                if (openFileDialog.ShowDialog() == true)
                {
                    FilePath = openFileDialog.FileName;
                    OnFilePathChanged();
                    Log.WriteLine($"File path selected: {FilePath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FilePath = string.Empty;
                OnFilePathChanged();
                Log.WriteLine("File path cleared");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing file path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    FilePath = textBox.Text;
                    OnFilePathChanged();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"FilePathTextBox_TextChanged error: {ex.Message}");
            }
        }

        protected virtual void OnFilePathChanged()
        {
            FilePathChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshFilePath()
        {
            UpdateFilePathDisplay();
        }
    }
}