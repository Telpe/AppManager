using AppManager.Core.Models;
using AppManager.Config.Interfaces;
using AppManager.Config.Pages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using AppManager.Config.Dialogs;

namespace AppManager.Config
{
    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AppManager.Core.Version Version => App.Version;
        private readonly Pages.AppsPage AppsPage = new();
        private readonly Pages.AppGroupsPage AppGroupsPage = new();
        private readonly ShortcutsPage ShortcutsPage = new();
        private OverlayManager? OverlayManagerValue;
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

            Log.WriteLine("MainWindow initialized");

            this.Closing += Window_Closing;

            // Set window icon using FileManager
            SetWindowIcon();

            // Apply settings to window
            ApplyWindowSettings();

            // Initialize profile selector
            InitializeProfileSelector();

            LoadPage();
        }

        /// <summary>
        /// Initializes the profile selector ComboBox
        /// </summary>
        private void InitializeProfileSelector()
        {
            try
            {
                // Load all available profiles
                string[] allProfiles = FileManager.GetAllProfiles();
                
                // Populate the ComboBox
                ProfileSelector.ItemsSource = allProfiles;

                // Set the current profile as selected
                ProfileSelector.SelectedItem = SettingsManager.CurrentSettings.LastUsedProfileName;
                
                Log.WriteLine($"Profile selector initialized with {allProfiles.Length} profiles. Current: {ProfileSelector.SelectedItem}");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error initializing profile selector: {ex.Message}");
                
                // Fallback: just show the current profile
                ProfileSelector.ItemsSource = new[] { ProfileManager.DefaultProfileFilename };
                ProfileSelector.SelectedItem = ProfileManager.DefaultProfileFilename;
            }
        }

        /// <summary>
        /// Handles profile selection changes
        /// </summary>
        private void ProfileSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ProfileSelector.SelectedItem is string selectedProfile && ProfileModel.IsValidProfileName(selectedProfile))
                {
                    // Only update if the profile actually changed
                    if (SettingsManager.CurrentSettings.LastUsedProfileName != selectedProfile)
                    {
                        Log.WriteLine($"Profile changed from '{SettingsManager.CurrentSettings.LastUsedProfileName}' to '{selectedProfile}'");
                        
                        // Update settings
                        SettingsManager.CurrentSettings.LastUsedProfileName = selectedProfile;
                        SettingsManager.SaveSettings();
                        
                        // Clear profile cache and reload
                        ProfileManager.ClearCache();

                        // Reload the navigation and page content
                        LoadPage();
                        
                        Log.WriteLine($"Profile switched to: {selectedProfile}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error handling profile selection change: {ex.Message}");
                MessageBox.Show($"Error switching profile: {ex.Message}", "Profile Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Refreshes the profile selector with the latest profiles
        /// </summary>
        public void RefreshProfileSelector()
        {
            try
            {
                string[] allProfiles = FileManager.GetAllProfiles();
                string currentSelection = ProfileSelector.SelectedItem as string ?? "";
                
                ProfileSelector.ItemsSource = allProfiles;
                
                // Restore selection if it still exists
                if (allProfiles.Contains(currentSelection))
                {
                    ProfileSelector.SelectedItem = currentSelection;
                }
                else
                {
                    // Select the current profile from settings
                    string currentProfile = SettingsManager.CurrentSettings.LastUsedProfileName ?? ProfileManager.DefaultProfileFilename;
                    ProfileSelector.SelectedItem = currentProfile;
                }
                
                Log.WriteLine($"Profile selector refreshed with {allProfiles.Length} profiles");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error refreshing profile selector: {ex.Message}")
;            }
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
                    iconPath = FileManager.FindExecutables("AppManager.Config.exe").FirstOrDefault() ?? "";

                    if (File.Exists(iconPath))
                    {
                        this.Icon = FileManager.ExtractBitmapSourceFromExecutable(iconPath);
                        Log.WriteLine("Using AppManager.exe icon for Settings window");
                    }
                    else
                    {
                        this.Icon = FileManager.GetShellIcon();
                        Log.WriteLine("Using shell icon as fallback for Settings");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error setting window icon: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows an overlay with the specified content and active area size
        /// </summary>
        /// <param name="content">Content that inherits from InputEditControl</param>
        /// <param name="widthPercent">Width of active area as percentage (0-100)</param>
        /// <param name="heightPercent">Height of active area as percentage (0-100)</param>
        /// <param name="clickHide">Whether to hide the overlay when clicking outside of it</param>
        public void ShowOverlay(IInputEditControl content)
        {
            OverlayManagerValue ??= new OverlayManager(this, new Vector2() { X = (float)(0.01f * 85), Y = (float)(0.01 * 85) }, false);
            OverlayManagerValue.ShowOverlay(content);
            //this.SizeChanged += (sender, e) => OverlayManagerValue.UpdateSize();
        }

        /// <summary>
        /// Hides the current overlay
        /// </summary>
        public void HideOverlay()
        {
            if (OverlayManagerValue?.HideOverlay() is true)
            {
                OverlayManagerValue = null;
            }
            
        }

        /// <summary>
        /// Gets whether an overlay is currently visible
        /// </summary>
        public bool IsOverlayVisible => OverlayManagerValue?.IsOverlayVisible ?? false;


        private void DataFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dataPath = FileManager.AppDataPath;
                
                // Ensure directory exists
                System.IO.Directory.CreateDirectory(dataPath);
                
                // Open the folder in Windows Explorer
                Process.Start("explorer.exe", dataPath);
                
                Log.WriteLine($"Opened data folder: {dataPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening data folder: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Log.WriteLine($"Error opening data folder: {ex.Message}");
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
                
                Log.WriteLine($"Applied window settings");
            }
        }


        private void LoadPage(string pageName = "Triggers", string? itemId = null)
        {
            IPageWithParameter page = pageName.ToLower() switch
            {
                "apps" => AppsPage,
                "groups" => AppGroupsPage,
                "shortcuts" => ShortcutsPage,
                "triggers" => new TriggersPage(),
                _ => throw new Exception($"Page not found: {pageName}")
            };

            if (itemId is not null) { page.LoadItemById(itemId); }

            MainFrame.Navigate(page);
        }

        public void Close(bool ignoreClosing)
        {
            if (ignoreClosing)
            {
                this.Closing -= Window_Closing;
            }

            Close();
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

            SettingsManager.CurrentSettings.WindowWidth = this.Width;
            SettingsManager.CurrentSettings.WindowHeight = this.Height;
            SettingsManager.CurrentSettings.WindowLeft = this.Left;
            SettingsManager.CurrentSettings.WindowTop = this.Top;
            SettingsManager.CurrentSettings.WindowMaximized = this.WindowState == WindowState.Maximized;

            SettingsManager.SaveSettings();

            e.Cancel = false;
        }

        public void ConsoleWriteline(string text)
        {
            ConsolePanel.Children.Add(new Label() { Content = text });
        }

    }
}


