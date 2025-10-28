using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Profile
{
    public class GroupManaged : DockPanel, IGroupManaged
    {
        public TextBox GroupNameBox;
        public TextBox DescriptionBox;
        public CheckBox SelectedBox, AutoCloseAllBox, IsExpandedBox;
        public ListBox MemberAppsListBox;
        public Button AddAppButton, RemoveAppButton;
        public Label MemberCountLabel;

        public string GroupName
        {
            get { return GroupNameBox.Text; }
            set { GroupNameBox.Text = value; }
        }

        public string Description
        {
            get { return DescriptionBox.Text; }
            set { DescriptionBox.Text = value; }
        }

        public bool Selected
        {
            get { return (bool)SelectedBox.IsChecked; }
            set { SelectedBox.IsChecked = value; }
        }

        public bool AutoCloseAll
        {
            get { return (bool)AutoCloseAllBox.IsChecked; }
            set { AutoCloseAllBox.IsChecked = value; }
        }

        public bool IsExpanded
        {
            get { return (bool)IsExpandedBox.IsChecked; }
            set { IsExpandedBox.IsChecked = value; }
        }

        public string[] MemberApps
        {
            get 
            { 
                return MemberAppsListBox.Items.Cast<string>().ToArray();
            }
            set 
            { 
                MemberAppsListBox.Items.Clear();
                foreach (string app in value)
                {
                    MemberAppsListBox.Items.Add(app);
                }
                UpdateMemberCount();
            }
        }

        public GroupManaged()
        {
            Initiate();
        }

        public GroupManaged(GroupManagedModel model)
        {
            Initiate();

            GroupName = model.GroupName;
            Description = model.Description;
            Selected = model.Selected;
            AutoCloseAll = model.AutoCloseAll;
            IsExpanded = model.IsExpanded;
            MemberApps = model.MemberApps;
        }

        private void Initiate()
        {
            Height = 120; // Taller than AppManaged due to more controls
            double checkboxNewWidth = 40;
            double checkboxWidth = 16;

            // Selected checkbox
            SelectedBox = new CheckBox();
            SelectedBox.Width = checkboxWidth;
            SelectedBox.Height = checkboxWidth;
            int leftright = (int)((checkboxNewWidth - checkboxWidth - SelectedBox.Padding.Left - SelectedBox.Padding.Right) * 0.5);
            SelectedBox.Margin = new Thickness(leftright, 1, leftright, 1);
            SelectedBox.VerticalAlignment = VerticalAlignment.Top;

            // Group name textbox
            GroupNameBox = new TextBox();
            GroupNameBox.Width = 160;
            GroupNameBox.Height = 22;
            GroupNameBox.Padding = new Thickness(2);
            GroupNameBox.Margin = new Thickness(0, 1, 0, 1);
            GroupNameBox.VerticalAlignment = VerticalAlignment.Top;

            // Description textbox
            DescriptionBox = new TextBox();
            DescriptionBox.Width = 160;
            DescriptionBox.Height = 22;
            DescriptionBox.Padding = new Thickness(2);
            DescriptionBox.Margin = new Thickness(0, 1, 0, 1);
            DescriptionBox.VerticalAlignment = VerticalAlignment.Top;

            // Auto close all checkbox
            AutoCloseAllBox = new CheckBox();
            AutoCloseAllBox.Width = checkboxWidth;
            AutoCloseAllBox.Height = checkboxWidth;
            AutoCloseAllBox.Margin = new Thickness(leftright, 1, leftright, 1);
            AutoCloseAllBox.VerticalAlignment = VerticalAlignment.Top;

            // Is expanded checkbox
            IsExpandedBox = new CheckBox();
            IsExpandedBox.Width = checkboxWidth;
            IsExpandedBox.Height = checkboxWidth;
            IsExpandedBox.Margin = new Thickness(leftright, 1, leftright, 1);
            IsExpandedBox.VerticalAlignment = VerticalAlignment.Top;

            // Member apps list
            MemberAppsListBox = new ListBox();
            MemberAppsListBox.Width = 120;
            MemberAppsListBox.Height = 60;
            MemberAppsListBox.Margin = new Thickness(2);
            MemberAppsListBox.VerticalAlignment = VerticalAlignment.Top;

            // Add app button
            AddAppButton = new Button();
            AddAppButton.Content = "+";
            AddAppButton.Width = 20;
            AddAppButton.Height = 20;
            AddAppButton.Margin = new Thickness(1);
            AddAppButton.Click += AddAppButton_Click;

            // Remove app button
            RemoveAppButton = new Button();
            RemoveAppButton.Content = "-";
            RemoveAppButton.Width = 20;
            RemoveAppButton.Height = 20;
            RemoveAppButton.Margin = new Thickness(1);
            RemoveAppButton.Click += RemoveAppButton_Click;

            // Member count label
            MemberCountLabel = new Label();
            MemberCountLabel.Padding = new Thickness(2);
            MemberCountLabel.Margin = new Thickness(1);
            MemberCountLabel.Content = "0 apps";

            // Create a vertical panel for the buttons and member count
            var buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Vertical;
            buttonPanel.Children.Add(AddAppButton);
            buttonPanel.Children.Add(RemoveAppButton);
            buttonPanel.Children.Add(MemberCountLabel);

            // Add all controls to the main panel
            this.Children.Add(SelectedBox);
            this.Children.Add(GroupNameBox);
            this.Children.Add(DescriptionBox);
            this.Children.Add(AutoCloseAllBox);
            this.Children.Add(IsExpandedBox);
            this.Children.Add(MemberAppsListBox);
            this.Children.Add(buttonPanel);

            UpdateMemberCount();
        }

        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {
            // This could open a dialog to select apps or add from a dropdown
            // For now, we'll add a placeholder
            var inputDialog = new Window
            {
                Title = "Add App to Group",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var textBox = new TextBox { Margin = new Thickness(5) };
            var addButton = new Button { Content = "Add", Margin = new Thickness(5) };

            addButton.Click += (s, args) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text) && !MemberAppsListBox.Items.Contains(textBox.Text))
                {
                    MemberAppsListBox.Items.Add(textBox.Text.Trim());
                    UpdateMemberCount();
                    inputDialog.Close();
                }
            };

            stackPanel.Children.Add(new Label { Content = "App Name:" });
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(addButton);
            inputDialog.Content = stackPanel;
            inputDialog.ShowDialog();
        }

        private void RemoveAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemberAppsListBox.SelectedItem != null)
            {
                MemberAppsListBox.Items.Remove(MemberAppsListBox.SelectedItem);
                UpdateMemberCount();
            }
        }

        private void UpdateMemberCount()
        {
            MemberCountLabel.Content = $"{MemberAppsListBox.Items.Count} apps";
        }
    }
}