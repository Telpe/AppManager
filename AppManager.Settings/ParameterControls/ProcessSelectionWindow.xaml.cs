using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using AppManager.Core.Utilities;

namespace AppManager.Settings.ParameterControls
{
    public partial class ProcessSelectionWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<ProcessInfo> _processes = new();
        private ProcessInfo? _selectedProcess;
        private string _searchText = string.Empty;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<ProcessInfo> Processes
        {
            get => _processes;
            set
            {
                _processes = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Processes)));
            }
        }

        public ProcessInfo? SelectedProcess
        {
            get => _selectedProcess;
            set
            {
                _selectedProcess = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedProcess)));
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
                FilterProcesses();
            }
        }

        public string SelectedProcessName => SelectedProcess?.ProcessName ?? string.Empty;

        public ProcessSelectionWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadProcesses();
        }

        private void LoadProcesses()
        {
            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .Select(p => new ProcessInfo(p.ProcessName, p.Id, p.MainWindowTitle))
                    .OrderBy(p => p.ProcessName)
                    .ToList();

                Processes.Clear();
                foreach (var process in processes)
                {
                    Processes.Add(process);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"LoadProcesses error: {ex.Message}");
                MessageBox.Show($"Error loading processes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterProcesses()
        {
            if (ProcessesListView.ItemsSource is CollectionView view)
            {
                view.Refresh();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProcesses();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedProcess != null)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Please select a process.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private bool ProcessFilter(object item)
        {
            if (item is ProcessInfo process && !string.IsNullOrEmpty(SearchText))
            {
                return process.ProcessName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       (!string.IsNullOrEmpty(process.WindowTitle) && process.WindowTitle.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }
            return true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var view = CollectionViewSource.GetDefaultView(Processes);
            view.Filter = ProcessFilter;
        }
    }

    public class ProcessInfo
    {
        public string ProcessName { get; }
        public int ProcessId { get; }
        public string WindowTitle { get; }

        public ProcessInfo(string processName, int processId, string windowTitle)
        {
            ProcessName = processName;
            ProcessId = processId;
            WindowTitle = windowTitle ?? string.Empty;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(WindowTitle) 
                ? $"{ProcessName} (PID: {ProcessId})" 
                : $"{ProcessName} - {WindowTitle} (PID: {ProcessId})";
        }
    }
}