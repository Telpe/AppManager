using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Utilities;

namespace AppManager.Settings.ParameterControls
{
    public partial class ProcessParameter : UserControl, INotifyPropertyChanged
    {
        private string _processName = String.Empty;
        public string HeaderText { get; } = "Process";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Value
        {
            get => _processName;
            set
            {
                if (_processName != value)
                {
                    _processName = value;
                    UpdateProcessDisplay();
                    BroadcastProcessChanged();
                }
            }
        }

        public string Title { get; } = "Select Process";

        public ProcessParameter()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ProcessParameter(string? processName, string? header = null) : this()
        {
            if (processName is not null) { _processName = processName; }
            if (header is not null) { HeaderText = header; }
        }

        private void UpdateProcessDisplay()
        {
            try
            {
                ProcessNameTextBox.Text = _processName;
                UpdateProcessStatus();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"UpdateProcessDisplay error: {ex.Message}");
            }
        }

        private void UpdateProcessStatus()
        {
            if (string.IsNullOrWhiteSpace(_processName))
            {
                ProcessStatusTextBlock.Text = "No process specified";
                ProcessStatusTextBlock.Foreground = System.Windows.Media.Brushes.Gray;
            }
            else
            {
                try
                {
                    var processes = Process.GetProcessesByName(_processName.Replace(".exe", ""));
                    if (processes.Length > 0)
                    {
                        ProcessStatusTextBlock.Text = $"Running ({processes.Length} instance{(processes.Length > 1 ? "s" : "")})";
                        ProcessStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        ProcessStatusTextBlock.Text = "Not running";
                        ProcessStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
                catch (Exception ex)
                {
                    ProcessStatusTextBlock.Text = "Error checking status";
                    ProcessStatusTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                    Log.WriteLine($"UpdateProcessStatus error: {ex.Message}");
                }
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var processSelectionWindow = new ProcessSelectionWindow();
                if (processSelectionWindow.ShowDialog() == true)
                {
                    Value = processSelectionWindow.SelectedProcessName;
                    Log.WriteLine($"Process selected: {Value}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Value = string.Empty;
                Log.WriteLine("Process cleared");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateProcessStatus();
                Log.WriteLine("Process status refreshed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing process status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void BroadcastProcessChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }

        public void RefreshProcess()
        {
            UpdateProcessDisplay();
        }
    }
}