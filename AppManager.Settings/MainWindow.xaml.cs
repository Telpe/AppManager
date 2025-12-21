using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager.Settings.AppGroups;
using AppManager.Settings.Apps;
using AppManager.Settings.Pages;
using AppManager.Settings.UI;
using AppManager.Settings.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppManager.Settings
{
    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Apps.AppsPage AppsPage = new();
        private readonly AppGroups.AppGroupsPage AppGroupsPage = new();
        private readonly ShortcutsPage ShortcutsPage = new();
        private OverlayManager OverlayManagerValue;
        private readonly ObservableCollection<string> Nav1ListValue = new();
        private string Nav1NewItemPlaceholderValue = "Enter new item...";
        private readonly ObservableCollection<string> Nav1MenuValue = new()
        {
            "Apps",
            "Groups",
            "Shortcuts"
        };

        public MainWindow()
        {
            InitializeComponent();

            

            Debug.WriteLine("MainWindow initialized");

            this.Closing += Window_Closing;

            // Initialize overlay manager
            OverlayManagerValue = new OverlayManager(this);

            // Handle window size changes
            this.SizeChanged += (sender, e) => OverlayManagerValue.UpdateSize();

            // Set window icon using FileManager
            SetWindowIcon();

            // Apply settings to window
            ApplyWindowSettings();

            InitiateNav1();
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
        public void ShowOverlay(OverlayContent content, double widthPercent = -100, double heightPercent = -100, bool clickHide = true)
        {
            OverlayManagerValue.ShowOverlay(content, new Vector2() { X = (float)(0.01f * widthPercent), Y = (float)(0.01 * heightPercent)}, clickHide);
        }

        /// <summary>
        /// Hides the current overlay
        /// </summary>
        public void HideOverlay()
        {
            OverlayManagerValue.HideOverlay();
        }

        /// <summary>
        /// Gets whether an overlay is currently visible
        /// </summary>
        public bool IsOverlayVisible => OverlayManagerValue.IsOverlayVisible;


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

        private void InitiateNav1()
        {
            Nav1List.ItemsSource = Nav1ListValue;
            Nav1List.AddHandler(Button.ClickEvent, new RoutedEventHandler(Nav1ListButton_Click));

            LoadNav1List(ProfileManager.CurrentProfile.SelectedNav1Menu);

            Nav1NewItemTextBox.GotFocus += (s, e) =>
            {
                if (Nav1NewItemTextBox.Text == Nav1NewItemPlaceholderValue)
                {
                    Nav1NewItemTextBox.Text = "";
                    Nav1NewItemTextBox.Foreground = Brushes.Black;
                }
            };

            Nav1NewItemTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(Nav1NewItemTextBox.Text))
                {
                    Nav1NewItemTextBox.Text = Nav1NewItemPlaceholderValue;
                    Nav1NewItemTextBox.Foreground = Brushes.Gray;
                }
            };

            Nav1NewItemTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter && 
                    !string.IsNullOrWhiteSpace(Nav1NewItemTextBox.Text) &&
                    Nav1NewItemTextBox.Text != Nav1NewItemPlaceholderValue)
                {
                    // Call the provided action with the entered text
                    AddNew(ProfileManager.CurrentProfile.SelectedNav1Menu, Nav1NewItemTextBox.Text.Trim());

                    // Reset the text box
                    Nav1NewItemTextBox.Text = "";
                    //Nav1NewItemTextBox.Text = Nav1NewItemPlaceholderValue;
                    //Nav1NewItemTextBox.Foreground = Brushes.Gray;
                    Nav1List.Focus();
                }
            };
        }

        private void AddNew(string category, string name)
        {
            switch (category.ToLower())
            {
                case "apps":
                    {
                        var newApp = new AppManagedModel(name, false);

                        ProfileManager.CurrentProfile.Apps = ProfileManager.CurrentProfile.Apps.Append(newApp).ToArray();

                        // Reload the navigation list
                        LoadNav1List("apps");

                        Debug.WriteLine($"Added new app: {newApp.AppName}");

                        break;
                    }
                case "groups":
                    {
                        var newGroup = new GroupManagedModel
                        {
                            GroupName = name,
                            Description = "New group"
                        };

                        // Add to current profile's app groups
                        ProfileManager.CurrentProfile.AppGroups = ProfileManager.CurrentProfile.AppGroups.Append(newGroup).ToArray();

                        // Reload the navigation list
                        LoadNav1List("groups");

                        Debug.WriteLine($"Added new group: {newGroup.GroupName}");

                        break;
                    }
            }
        }

        private void LoadNav1List(string pageName)
        {
            Nav1ListValue.Clear();

            switch (pageName.ToLower())
            {
                case "apps":
                {
                    foreach (AppManagedModel app in ProfileManager.CurrentProfile.Apps)
                    {
                        Nav1ListValue.Add(app.AppName);
                    }

                    Nav1NewItemPlaceholderValue = "Enter app name...";

                    break;
                }
                case "groups":
                {
                    foreach (GroupManagedModel grp in ProfileManager.CurrentProfile.AppGroups)
                    {
                        Nav1ListValue.Add(grp.GroupName);
                    }

                    Nav1NewItemPlaceholderValue = "Enter group name...";

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

            Nav1NewItemTextBox.Text = Nav1NewItemPlaceholderValue;
        }

        private void LoadPage()
        {
            string pageName = ProfileManager.CurrentProfile.SelectedNav1List;
            IPageWithParameter page = ProfileManager.CurrentProfile.SelectedNav1Menu.ToLower() switch
            {
                "apps" => AppsPage,
                "groups" => AppGroupsPage,
                "shortcuts" => ShortcutsPage,
                _ => throw new Exception($"Page not found: {pageName}")
            };
            
            page.LoadPageByName(pageName);

            MainFrame.Navigate(page);
        }

        private void Nav1MenuButton_Click(object? sender, RoutedEventArgs e)
        {
            if(sender is Button fromSender)
            {
                Debug.WriteLine($"Nav1MenuButton_Click: {fromSender.Content}\nGetting Nav1List");
                ProfileManager.CurrentProfile.SelectedNav1Menu = fromSender.Content.ToString() ?? "";
                LoadNav1List(ProfileManager.CurrentProfile.SelectedNav1Menu);
                Debug.WriteLine($"Update profile with selected page");
                // Update profile with selected page
                
            }
            
        }

        private void Nav1ListButton_Click(object? sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button fromSender && fromSender.Content is string selectedNav1ListItem)
            {
                ProfileManager.CurrentProfile.SelectedNav1List = selectedNav1ListItem;
                LoadPage();
            }
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

        private void Window_Closing(object? sender, CancelEventArgs e)
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


