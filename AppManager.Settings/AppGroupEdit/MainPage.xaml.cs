using AppManager.Core.Models;
using AppManager.Settings.Utils;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppManager.Settings.AppGroupEdit
{
    /// <summary>
    /// Interaction logic for AppGroupsPage.xaml
    /// </summary>
    public partial class MainPage : Page, IPageWithParameter
    {
        private string _PageName = "";
        private GroupManagedModel _currentGroup;

        public MainPage()
        {
            InitializeComponent();
        }

        public void LoadPageByName(string pageName)
        {
            _PageName = pageName;
            
            // Find the group by name
            _currentGroup = ProfileManager.CurrentProfile.AppGroups.FirstOrDefault(g => g.GroupName == pageName);
            
            if (_currentGroup != null)
            {
                LoadGroupDetails(_currentGroup);
                Debug.WriteLine($"Loaded group details for: {pageName}");
            }
            else
            {
                Debug.WriteLine($"Group not found: {pageName}");
                ShowNoGroupSelected();
            }
        }

        private void LoadGroupDetails(GroupManagedModel group)
        {
            // Clear the grid and add group details
            MainGrid.Children.Clear();

            // Create UI elements for the group
            var titleLabel = new Label
            {
                Content = $"Group: {group.GroupName}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var descriptionLabel = new Label
            {
                Content = "Description:",
                FontWeight = FontWeights.Bold
            };

            var descriptionTextBox = new TextBox
            {
                Text = group.Description,
                Margin = new Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap,
                Height = 60
            };
            descriptionTextBox.TextChanged += (s, e) => group.Description = descriptionTextBox.Text;

            var settingsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var selectedCheckBox = new CheckBox
            {
                Content = "Selected",
                IsChecked = group.Selected,
                Margin = new Thickness(0, 0, 20, 0)
            };
            selectedCheckBox.Checked += (s, e) => group.Selected = true;
            selectedCheckBox.Unchecked += (s, e) => group.Selected = false;

            var autoCloseCheckBox = new CheckBox
            {
                Content = "Auto Close All",
                IsChecked = group.AutoCloseAll,
                Margin = new Thickness(0, 0, 20, 0)
            };
            autoCloseCheckBox.Checked += (s, e) => group.AutoCloseAll = true;
            autoCloseCheckBox.Unchecked += (s, e) => group.AutoCloseAll = false;

            var expandedCheckBox = new CheckBox
            {
                Content = "Expanded",
                IsChecked = group.IsExpanded
            };
            expandedCheckBox.Checked += (s, e) => group.IsExpanded = true;
            expandedCheckBox.Unchecked += (s, e) => group.IsExpanded = false;

            settingsPanel.Children.Add(selectedCheckBox);
            settingsPanel.Children.Add(autoCloseCheckBox);
            settingsPanel.Children.Add(expandedCheckBox);

            var memberAppsLabel = new Label
            {
                Content = "Member Apps:",
                FontWeight = FontWeights.Bold
            };

            var memberAppsListBox = new ListBox
            {
                Height = 150,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Populate member apps
            foreach (var app in group.MemberApps)
            {
                memberAppsListBox.Items.Add(app);
            }

            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var addAppButton = new Button
            {
                Content = "Add App",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            addAppButton.Click += (s, e) => AddAppToGroup(group, memberAppsListBox);

            var removeAppButton = new Button
            {
                Content = "Remove App",
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            removeAppButton.Click += (s, e) => RemoveAppFromGroup(group, memberAppsListBox);

            var saveButton = new Button
            {
                Content = "Save Group",
                Padding = new Thickness(10, 5, 10, 5),
                Background = Brushes.LightGreen
            };
            saveButton.Click += (s, e) => SaveGroup();

            buttonsPanel.Children.Add(addAppButton);
            buttonsPanel.Children.Add(removeAppButton);
            buttonsPanel.Children.Add(saveButton);

            // Create a main stack panel for all elements
            var mainStackPanel = new StackPanel();
            mainStackPanel.Children.Add(titleLabel);
            mainStackPanel.Children.Add(descriptionLabel);
            mainStackPanel.Children.Add(descriptionTextBox);
            mainStackPanel.Children.Add(settingsPanel);
            mainStackPanel.Children.Add(memberAppsLabel);
            mainStackPanel.Children.Add(memberAppsListBox);
            mainStackPanel.Children.Add(buttonsPanel);

            MainGrid.Children.Add(mainStackPanel);
        }

        private void ShowNoGroupSelected()
        {
            MainGrid.Children.Clear();
            var noGroupLabel = new Label
            {
                Content = "No group selected or group not found.",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
            MainGrid.Children.Add(noGroupLabel);
        }

        private void AddAppToGroup(GroupManagedModel group, ListBox memberAppsListBox)
        {
            var availableApps = ProfileManager.CurrentProfile.Apps
                .Where(app => !group.MemberApps.Contains(app.AppName))
                .Select(app => app.AppName)
                .ToList();

            if (availableApps.Count == 0)
            {
                MessageBox.Show("No available apps to add to this group.", "No Apps Available", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var parentWindow = Window.GetWindow(this);
            var selectionWindow = new Window
            {
                Title = "Select App to Add",
                Width = 300,
                Height = 400,
                Owner = parentWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var listBox = new ListBox { Height = 250, Margin = new Thickness(0, 0, 0, 10) };

            foreach (var app in availableApps)
            {
                listBox.Items.Add(app);
            }

            var addButton = new Button
            {
                Content = "Add Selected",
                Padding = new Thickness(10, 5, 10, 5)
            };

            addButton.Click += (s, e) =>
            {
                if (listBox.SelectedItem != null)
                {
                    var selectedApp = listBox.SelectedItem.ToString();
                    group.MemberApps = group.MemberApps.Append(selectedApp).ToArray();
                    memberAppsListBox.Items.Add(selectedApp);
                    selectionWindow.Close();
                }
            };

            stackPanel.Children.Add(new Label { Content = "Available Apps:" });
            stackPanel.Children.Add(listBox);
            stackPanel.Children.Add(addButton);
            selectionWindow.Content = stackPanel;
            selectionWindow.ShowDialog();
        }

        private void RemoveAppFromGroup(GroupManagedModel group, ListBox memberAppsListBox)
        {
            if (memberAppsListBox.SelectedItem != null)
            {
                var selectedApp = memberAppsListBox.SelectedItem.ToString();
                group.MemberApps = group.MemberApps.Where(app => app != selectedApp).ToArray();
                memberAppsListBox.Items.Remove(memberAppsListBox.SelectedItem);
            }
        }

        private void SaveGroup()
        {
            ProfileManager.SaveProfile();
            MessageBox.Show("Group saved successfully!", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool HasUnsavedChanges()
        {
            throw new NotImplementedException();
        }
    }
}