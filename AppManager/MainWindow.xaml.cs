using AppManager.Pages;
using AppManager.Profile;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinRT;

namespace AppManager
{
    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AppPage AppsPage = new();
        private readonly AppGroupsPage AppGroupsPage = new();
        private readonly ShortcutsPage ShortcutsPage = new();

        public MainWindow()
        {
            InitializeComponent();

            Debug.WriteLine("MainWindow initialized");

            this.Closing += Window_Closing;

            // Apply profile settings to window
            ApplyProfileSettings();

            // Load navigation based on profile's last selected page
            LoadNav1List(App.CurrentProfile.SelectedNav1Menu);
        }

        private void ApplyProfileSettings()
        {
            var profile = App.CurrentProfile;
            if (profile != null)
            {
                // Apply window settings from profile
                this.Width = profile.WindowWidth;
                this.Height = profile.WindowHeight;
                this.Left = profile.WindowLeft;
                this.Top = profile.WindowTop;
                
                if (profile.WindowMaximized)
                {
                    this.WindowState = WindowState.Maximized;
                }

                // Apply theme if you have themes implemented
                // ApplyTheme(profile.Theme);
                
                Debug.WriteLine($"Applied profile settings for user: {profile.Username}");
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
                    LoadNavigation(Nav1List.Children, App.CurrentProfile.Apps.Select(a=> a.AppName).ToArray(), Nav1ListButton_Click);
                    
                    // Add text input for creating new apps using the general method
                    var newAppTextBox = NewNavigationAddItemTextBox("Enter app name...", (appName) =>
                    {
                        // Create new AppManagedModel
                        var newApp = new AppManagedModel
                        {
                            AppName = appName
                        };

                        // Add to current profile
                        App.CurrentProfile.Apps = App.CurrentProfile.Apps.Append(newApp).ToArray();

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
                    LoadNavigation(Nav1List.Children, App.CurrentProfile.AppGroups.Select(g => g.GroupName).ToArray(), Nav1ListButton_Click);
                    
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
                        App.CurrentProfile.AppGroups = App.CurrentProfile.AppGroups.Append(newGroup).ToArray();

                        // Reload the navigation list
                        LoadNav1List("groups");

                        Debug.WriteLine($"Added new group: {newGroup.GroupName}");
                    });

                    Nav1List.Children.Add(newGroupTextBox);
                    break;
                }
                case "shortcuts":
                {
                    //LoadNavigation(Nav1List.Children, App.CurrentProfile. .Select(a => a.AppName).ToArray(), Nav1ListButton_Click);
                    
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
            switch (App.CurrentProfile.SelectedNav1Menu.ToLower())
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
            Debug.WriteLine($"Nav1MenuButton_Click: {sender.As<Button>().Content.ToString()}\nGetting Nav1List");
            LoadNav1List(sender.As<Button>().Content.ToString());
            Debug.WriteLine($"Update profile with selected page");
            // Update profile with selected page
            App.CurrentProfile.SelectedNav1Menu = sender.As<Button>().Content.ToString();
        }

        private void Nav1ListButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(sender.As<Button>().Content.ToString());

            // Update profile with last selected page
            App.CurrentProfile.SelectedNav1List = sender.As<Button>().Content.ToString();
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
            Debug.WriteLine($"Showing favorite apps: {string.Join(", ", App.CurrentProfile.FavoriteApps)}");
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

            if (App.CurrentProfile != null)
            {
                App.CurrentProfile.WindowWidth = this.Width;
                App.CurrentProfile.WindowHeight = this.Height;
                App.CurrentProfile.WindowLeft = this.Left;
                App.CurrentProfile.WindowTop = this.Top;
                App.CurrentProfile.WindowMaximized = this.WindowState == WindowState.Maximized;
            }

            // Save profile with updated settings
            App.SaveProfile();

            e.Cancel = false;
        }
        public void ConsoleWriteline(string text)
        {
            ConsolePanel.Children.Add(new Label() { Content = text });
        }
    }
}
