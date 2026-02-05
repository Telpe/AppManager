using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Utilities;
using Microsoft.Win32;

namespace AppManager.Settings.ParameterControls
{
    public partial class FilePathParameter : BaseParameterControl
    {
        private string _filePath = String.Empty;

        public string Value
        {
            get => _filePath;
            set
            {
                if (_filePath == value) { return; }

                _filePath = value;

                UpdateFilePathDisplay();
                BroadcastPropertyChanged(ValueName);
            }
        }

        public string FileFilter { get; } = "All Files (*.*)|*.*";
        public string Title { get; } = "Select File";

        public FilePathParameter()
        {
            _headerText = "File Path";
            _labelText = "Select File";

            InitializeComponent();
            this.DataContext = this;

            UpdateFilePathDisplay();
        }

        public FilePathParameter(string? filePath, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (filePath is not null) { _filePath = filePath; }
            if (headerText is not null) { _headerText = headerText; }
            if (labelText is not null) { _labelText = labelText; }

            if (customValueName is not null) { ValueName = customValueName; }

            UpdateFilePathDisplay();

            if (eventHandler is not null) { PropertyChanged += eventHandler; }
        }

        private void UpdateFilePathDisplay()
        {
            try
            {
                FilePathTextBox.Text = _filePath;
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
                    Filter = FileFilter,
                    Title = Title,
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
                    Value = openFileDialog.FileName;
                    Log.WriteLine($"File path selected: {Value}");
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
                Value = string.Empty;
                Log.WriteLine("File path cleared");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing file path: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
/*
        private void FilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox)
                {
                    Value = textBox.Text;
                    BroadcastFilePathChanged();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"FilePathTextBox_TextChanged error: {ex.Message}");
            }
        }
*/


        public void RefreshFilePath()
        {
            UpdateFilePathDisplay();
        }
    }
}