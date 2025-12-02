using AppManager.Core.Utils;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<OSShortcutItem> _shortcuts;

        public MainWindow()
        {
            InitializeComponent();
            LoadShortcuts();
        }

        private void LoadShortcuts()
        {
            try
            {
                StatusTextBlock.Text = "Loading shortcuts...";

                // FileManager now returns BrowserShortcutModel objects
                var shortcutModels = FileManager.GetOSShortcuts();

                // Convert models to view models
                _shortcuts = shortcutModels
                    .Select(model => new OSShortcutItem(model))
                    .ToList();

                ShortcutsItemsControl.ItemsSource = _shortcuts;

                ItemCountTextBlock.Text = $"{_shortcuts.Count} items";
                StatusTextBlock.Text = _shortcuts.Count > 0 ? "Ready" : "No shortcuts found";

                Debug.WriteLine($"Loaded {_shortcuts.Count} browser shortcuts");
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Error loading shortcuts";
                MessageBox.Show($"Error loading shortcuts: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error loading browser shortcuts: {ex.Message}");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadShortcuts();
        }

        private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderPath = FileManager.GetBrowserShortcutsPath();

                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                Process.Start("explorer.exe", folderPath);
                StatusTextBlock.Text = "Opened BrowserShortcuts folder";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is not System.Windows.Controls.Button button ||
                    button.Tag is not OSShortcutItem shortcutItem)
                {
                    return;
                }

                StatusTextBlock.Text = $"Executing {shortcutItem.Name}...";

                bool success = FileManager.ExecuteFile(shortcutItem.ExecutablePath);

                if (success)
                {
                    StatusTextBlock.Text = $"Executed {shortcutItem.Name} successfully";
                    Debug.WriteLine($"Successfully executed: {shortcutItem.Name} ({shortcutItem.ExecutablePath})");
                }
                else
                {
                    StatusTextBlock.Text = $"Failed to execute {shortcutItem.Name}";
                    MessageBox.Show($"Failed to execute {shortcutItem.Name}", "Execution Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Execution failed";
                MessageBox.Show($"Error executing file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error executing browser shortcut: {ex.Message}");
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }
    }
}