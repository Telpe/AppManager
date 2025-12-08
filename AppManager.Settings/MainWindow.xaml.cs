using AppManager.Core.Utils;
using AppManager.Settings.AppGroupEdit;
using AppManager.Settings.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AppManager.Settings.AppEdit;
using AppManager.Core.Models;
using AppManager.Settings.Utils;
using System.ComponentModel;
using System.IO;

namespace AppManager.Settings
{
    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppEdit.MainPage AppsPage = new();
        private readonly AppGroupEdit.MainPage AppGroupsPage = new();
        private readonly ShortcutsPage ShortcutsPage = new();
        private OverlayManager OverlayManagerStored;

        public MainWindow()
        {
            InitializeComponent();

            Debug.WriteLine("MainWindow initialized");

            this.Closing += Window_Closing;

            // Initialize overlay manager
            OverlayManagerStored = new OverlayManager(this);

            // Handle window size changes
            this.SizeChanged += (sender, e) => OverlayManagerStored.UpdateSize();

            // Set window icon using FileManager
            SetWindowIcon();

            // Apply settings to window
            ApplyWindowSettings();

            // Load navigation based on profile's last selected page
            LoadNav1List(ProfileManager.CurrentProfile.SelectedNav1Menu);
        }

        private void SetWindowIcon()
        {
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileManager.IconfileDefault);
                
                if (File.Exists(iconPath))
                {
                    this.Icon = FileManager.CreateImageSourceFromFile(iconPath);
                }
                else
                {
                    iconPath = FileManager.FindExecutables("AppManager.Settings.exe").FirstOrDefault() ?? "";

                    if (File.Exists(iconPath))
                    {
                        this.Icon = FileManager.ExtractBitmapSourceFromExecutable(iconPath);
                        Debug.WriteLine("Using AppManager.exe icon for Settings window");
                    }
                    else
                    {
                        this.Icon = FileManager.GetShellIcon();
                        Debug.WriteLine("Using shell icon as fallback for Settings");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting window icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an overlay with the specified content and active area size
        /// </summary>
        /// <param name="content">Content that inherits from OverlayContent</param>
        /// <param name="widthPercent">Width of active area as percentage (0-100)</param>
        /// <param name="heightPercent">Height of active area as percentage (0-100)</param>
        public void ShowOverlay(OverlayContent content, double widthPercent = 50, double heightPercent = 50, bool clickHide = true)
        {
            OverlayManagerStored.ShowOverlay(content, widthPercent, heightPercent, clickHide);
        }

        /// <summary>
        /// Hides the current overlay
        /// </summary>
        public void HideOverlay()
        {
            OverlayManagerStored.HideOverlay();
        }

        /// <summary>
        /// Gets whether an overlay is currently visible
        /// </summary>
        public bool IsOverlayVisible => OverlayManagerStored.IsOverlayVisible;


        private void DataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dataPath = FileManager.AppDataPath;
                
                // Ensure directory exists
                System.IO.Directory.CreateDirectory(dataPath);
                
                // Open the folder in Windows Explorer
                Process.Start("explorer.exe", dataPath);
                
                Debug.WriteLine($"Opened data folder: {dataPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening data folder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error opening data folder: {ex.Message}");
            }
        }

        private void ApplyWindowSettings()
        {
            var settings = SettingsManager.CurrentSettings;
            if (settings != null)
            {
                // Apply window settings from settings file
                this.Width = settings.WindowWidth;
                this.Height = settings.WindowHeight;
                this.Left = settings.WindowLeft;
                this.Top = settings.WindowTop;
                
                if (settings.WindowMaximized)
                {
                    this.WindowState = WindowState.Maximized;
                }

                // Apply theme if you have themes implemented
                // ApplyTheme(settings.Theme);
                
                Debug.WriteLine($"Applied window settings");
            }
        }

        private TextBox NewNavigationAddItemTextBox(string placeholderText, Action<string> onItemAdded)
        {
            var textBox = new TextBox
            {
                Margin = new Thickness(5),
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                Text = placeholderText,
                Foreground = Brushes.Gray
            };

            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    textBox.Text = "";
                    textBox.Foreground = Brushes.Black;
                }
            };

            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholderText;
                    textBox.Foreground = Brushes.Gray;
                }
            };

            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter && 
                    !string.IsNullOrWhiteSpace(textBox.Text) && 
                    textBox.Text != placeholderText)
                {
                    // Call the provided action with the entered text
                    onItemAdded?.Invoke(textBox.Text.Trim());

                    // Reset the text box
                    textBox.Text = placeholderText;
                    textBox.Foreground = Brushes.Gray;
                }
            };

            return textBox;
        }

        private void LoadNav1List(string pageName)
        {
            switch (pageName.ToLower())
            {
                case "apps":
                {
                    LoadNavigation(Nav1List.Children, ProfileManager.CurrentProfile.Apps.Select(a=> a.AppName).ToArray(), Nav1ListButton_Click);
                    
                    // Add text input for creating new apps using the general method
                    var newAppTextBox = NewNavigationAddItemTextBox("Enter app name...", (appName) =>
                    {
                        // Create new AppManagedModel
                        var newApp = new AppManagedModel
                        {
                            AppName = appName
                        };

                        // Add to current profile
                        ProfileManager.CurrentProfile.Apps = ProfileManager.CurrentProfile.Apps.Append(newApp).ToArray();

                        // Reload the navigation list
                        LoadNav1List("apps");

                        Debug.WriteLine($"Added new app: {newApp.AppName}");
                    });

                    Nav1List.Children.Add(newAppTextBox);
                    break;
                }
                case "groups":
                {
                    // Extract group names from GroupManagedModel array
                    LoadNavigation(Nav1List.Children, ProfileManager.CurrentProfile.AppGroups.Select(g => g.GroupName).ToArray(), Nav1ListButton_Click);
                    
                    // Add text input for creating new groups using the general method
                    var newGroupTextBox = NewNavigationAddItemTextBox("Enter group name...", (groupName) =>
                    {
                        // Create new GroupManagedModel
                        var newGroup = new GroupManagedModel
                        {
                            GroupName = groupName,
                            Description = "New group"
                        };

                        // Add to current profile's app groups
                        ProfileManager.CurrentProfile.AppGroups = ProfileManager.CurrentProfile.AppGroups.Append(newGroup).ToArray();

                        // Reload the navigation list
                        LoadNav1List("groups");

                        Debug.WriteLine($"Added new group: {newGroup.GroupName}");
                    });

                    Nav1List.Children.Add(newGroupTextBox);
                    break;
                }
                case "shortcuts":
                {
                    //LoadNavigation(Nav1List.Children, ProfileManager.CurrentProfile. .Select(a => a.AppName).ToArray(), Nav1ListButton_Click);
                    
                    // You can also add shortcuts input here when ready
                    // var newShortcutTextBox = NewNavigationAddItemTextBox("Enter shortcut name...", (shortcutName) =>
                    // {
                    //     // Add shortcut logic here
                    //     LoadNav1List("shortcuts");
                    //     Debug.WriteLine($"Added new shortcut: {shortcutName}");
                    // });
                    // Nav1List.Children.Add(newShortcutTextBox);
                    break;
                }
                default:
                {
                    LoadNav1List("apps");
                    break;
                }
            }
        }

        private void LoadPage(string pageName)
        {
            switch (ProfileManager.CurrentProfile.SelectedNav1Menu.ToLower())
            {
                case "apps":
                {
                    if (AppsPage is IPageWithParameter appsPageWithParam)
                    {
                        appsPageWithParam.LoadPageByName(pageName);
                    }
                    MainFrame.Navigate(AppsPage);
                    break;
                }
                case "groups":
                {
                    if (AppGroupsPage is IPageWithParameter groupsPageWithParam)
                    {
                        groupsPageWithParam.LoadPageByName(pageName);
                    }
                    MainFrame.Navigate(AppGroupsPage);
                    break;
                }
                case "shortcuts":
                {
                    if (ShortcutsPage is IPageWithParameter shortcutsPageWithParam)
                    {
                        shortcutsPageWithParam.LoadPageByName(pageName);
                    }
                    MainFrame.Navigate(ShortcutsPage);
                    break;
                }
                default:
                {
                    if (AppsPage is IPageWithParameter defaultPageWithParam)
                    {
                        defaultPageWithParam.LoadPageByName(pageName);
                    }
                    MainFrame.Navigate(AppsPage);
                    break;
                }
            }
        }

        private void Nav1MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button fromSender)
            {
                Debug.WriteLine($"Nav1MenuButton_Click: {fromSender.Content.ToString()}\nGetting Nav1List");
                LoadNav1List(fromSender.Content.ToString()??"");
                Debug.WriteLine($"Update profile with selected page");
                // Update profile with selected page
                ProfileManager.CurrentProfile.SelectedNav1Menu = fromSender.Content.ToString();
            }
            
        }

        private void Nav1ListButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button fromSender && null != fromSender.Content)
            {
                LoadPage(fromSender.Content.ToString()??"");

                // Update profile with last selected page
                ProfileManager.CurrentProfile.SelectedNav1List = fromSender.Content.ToString()??"";
            }
        }

        private Button NewNavigationButton(string content, RoutedEventHandler clickHandler)
        {
            var b = new Button
            {
                Content = content,
                Margin = new Thickness(5),
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray
            };

            b.Click += clickHandler;

            return b;
        }

        private void LoadNavigation(UIElementCollection listParent, string[] namesList, RoutedEventHandler clickHandler)
        {
            listParent.Clear();

            foreach (string name in namesList)
            {
                Button b = NewNavigationButton(name, clickHandler);
                listParent.Add(b);
            }
        }

        private StackPanel FindNavigationStackPanel()
        {
            // Find the navigation StackPanel in your XAML structure
            var mainGrid = (Grid)this.Content;
            var leftColumn = (StackPanel)mainGrid.Children[1]; // The left column StackPanel
            var scrollViewer = (ScrollViewer)leftColumn.Children[1]; // The ScrollViewer containing navigation
            return (StackPanel)scrollViewer.Content; // The actual navigation StackPanel
        }

        private Button CreateNavigationItem(string text, Action clickAction)
        {
            var navigationButton = new Button
            {
                Content = text,
                Margin = new Thickness(5),
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray
            };

            navigationButton.Click += (s, e) => clickAction();
            return navigationButton;
        }

        // Navigation action methods for Apps page
        private void ShowAllApps()
        {
            // Implementation to show all apps in the Apps page
            Debug.WriteLine("Showing all apps");
        }

        private void ShowRunningApps()
        {
            // Implementation to show only running apps
            Debug.WriteLine("Showing running apps");
        }

        private void ShowFavorites()
        {
            // Implementation to show favorite apps from profile
            Debug.WriteLine($"Showing favorite apps: {string.Join(", ", ProfileManager.CurrentProfile.FavoriteApps)}");
        }

        private void ShowRecentlyUsed()
        {
            // Implementation to show recently used apps
            Debug.WriteLine("Showing recently used apps");
        }

        // Navigation action methods for AppGroups page
        private void ShowGamingGroup()
        {
            Debug.WriteLine("Showing gaming group");
        }

        private void ShowDevelopmentGroup()
        {
            Debug.WriteLine("Showing development group");
        }

        private void ShowProductivityGroup()
        {
            Debug.WriteLine("Showing productivity group");
        }

        private void ShowMediaGroup()
        {
            Debug.WriteLine("Showing media group");
        }

        // Navigation action methods for Shortcuts page
        private void ShowSystemShortcuts()
        {
            Debug.WriteLine("Showing system shortcuts");
        }

        private void ShowCustomShortcuts()
        {
            Debug.WriteLine("Showing custom shortcuts");
        }

        private void ShowQuickActions()
        {
            Debug.WriteLine("Showing quick actions");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            // Warn about unsaved changes in Apps page
            if (AppsPage.HasUnsavedChanges() == true)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes in one or more app pages. Do you want to return and save them?", 
                    "Unsaved Changes", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Save window settings to settings file
            var settings = SettingsManager.CurrentSettings;
            if (settings != null)
            {
                settings.WindowWidth = this.Width;
                settings.WindowHeight = this.Height;
                settings.WindowLeft = this.Left;
                settings.WindowTop = this.Top;
                settings.WindowMaximized = this.WindowState == WindowState.Maximized;
            }

            // Save only settings
            SettingsManager.SaveSettings();

            e.Cancel = false;
        }

        public void ConsoleWriteline(string text)
        {
            ConsolePanel.Children.Add(new Label() { Content = text });
        }

        private void TestOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create example overlay content
                var overlayContent = new ExampleOverlayContent("This is a test overlay!\nClick 'Close Overlay' or click outside to dismiss.");
                
                // Show overlay with 60% width and 40% height
                ShowOverlay(overlayContent, 60, 40);
                
                Debug.WriteLine("Test overlay displayed");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing overlay: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Error showing overlay: {ex.Message}");
            }
        }
    }
}


