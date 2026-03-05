using AppManager.Core.Models;
using AppManager.Config.EditorControls;
using AppManager.Config.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Config.Utilities;
using AppManager.Core.Triggers;
using System.Threading;

namespace AppManager.Config.Pages
{
    /// <summary>
    /// Interaction logic for TriggersPage.xaml
    /// </summary>
    public partial class TriggersPage : Page, IPageWithParameter
    {
        private string _selectedTagFilter = "All";
        private Dictionary<string, TriggerModel> NotSaved = [];
        private readonly Lock EditLock = new();

        public TriggersPage()
        {
            InitializeComponent();
            InitializeTagFilter();
            RefreshTriggerMenu();
        }

        public void LoadItemById(string triggerId)
        {
            if (!string.IsNullOrEmpty(triggerId))
            {
                var trigger = ProfileManager.CurrentProfile.Triggers.FirstOrDefault(t => t.Id == triggerId);
                if (trigger != null)
                {
                    ShowTriggerEditor(trigger);
                }
            }
        }

        private void InitializeTagFilter()
        {
            var allTagKeys = new HashSet<string>();
            foreach (var trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (trigger.Tags != null)
                {
                    foreach (var tagKey in trigger.Tags.Keys)
                    {
                        allTagKeys.Add(tagKey);
                    }
                }
            }

            TagFilterComboBox.Items.Clear();
            TagFilterComboBox.Items.Add("All");
            
            foreach (var tagKey in allTagKeys.OrderBy(k => k))
            {
                TagFilterComboBox.Items.Add(tagKey);
            }
            
            TagFilterComboBox.SelectedIndex = 0;
        }

        private void TagFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TagFilterComboBox.SelectedItem is string selectedTag)
            {
                _selectedTagFilter = selectedTag;
                RefreshTriggerMenu();
            }
        }

        private void RefreshTriggerMenu()
        {
            GroupsStackPanel.Children.Clear();
            AnonymousStackPanel.Children.Clear();
            AnonymousGroup.Visibility = Visibility.Collapsed;

            TriggerModel[] triggersWithoutTag;

            if (_selectedTagFilter == "All")
            {
                triggersWithoutTag = ProfileManager.CurrentProfile.Triggers;
            }
            else
            {
                // Group by tag values
                TriggerModel[] triggersWithTag = ProfileManager.CurrentProfile.Triggers.Where(t => t.Tags?.ContainsKey(_selectedTagFilter) is true).ToArray();
                triggersWithoutTag = ProfileManager.CurrentProfile.Triggers.Where(t => t.Tags?.ContainsKey(_selectedTagFilter) is not true).ToArray();

                // Group triggers by tag value
                var groups = triggersWithTag
                    .GroupBy(t => t.Tags![_selectedTagFilter])
                    .OrderBy(g => g.Key);

                foreach (var group in groups)
                {
                    var groupBox = new GroupBox
                    {
                        Header = string.IsNullOrEmpty(group.Key) ? "(Empty)" : group.Key,
                        Margin = new Thickness(0, 0, 0, 5)
                    };

                    var groupPanel = new StackPanel();
                    foreach (var trigger in group.OrderBy(t => t.Id))
                    {
                        var button = CreateTriggerButton(trigger);
                        groupPanel.Children.Add(button);
                    }

                    groupBox.Content = groupPanel;
                    GroupsStackPanel.Children.Add(groupBox);
                }
            }

            // Add triggers without the selected tag to anonymous group
            if (0 < triggersWithoutTag.Length)
            {
                foreach (var trigger in triggersWithoutTag.OrderBy(t => t.Id))
                {
                    var button = CreateTriggerButton(trigger);
                    AnonymousStackPanel.Children.Add(button);
                }
                AnonymousGroup.Header = $"No '{_selectedTagFilter}' tag";
                AnonymousGroup.Visibility = Visibility.Visible;
            }
        }

        private Button CreateTriggerButton(TriggerModel trigger)
        {
            var button = new Button
            {
                Content = GetTriggerDisplayName(trigger),
                Tag = trigger,
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(5)
            };

            button.Click += TriggerButton_Click;
            return button;
        }

        private string GetTriggerDisplayName(TriggerModel trigger)
        {
            if (!string.IsNullOrEmpty(trigger.Id))
            {
                return trigger.Id;
            }

            // Fallback to trigger type and key info
            var displayName = trigger.TriggerType.ToString();
            
            if (!string.IsNullOrEmpty(trigger.KeybindCombination))
            {
                displayName += $" ({trigger.KeybindCombination})";
            }
            else if (!string.IsNullOrEmpty(trigger.ProcessName))
            {
                displayName += $" ({trigger.ProcessName})";
            }
            else if (!string.IsNullOrEmpty(trigger.EventName))
            {
                displayName += $" ({trigger.EventName})";
            }

            return displayName;
        }

        private void TriggerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TriggerModel trigger)
            {
                ShowTriggerEditor(trigger);
            }
        }

        private void AddTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            var newTrigger = new TriggerModel
            {
                Id = Shared.ToShortString(Guid.NewGuid()),
                TriggerType = TriggerTypeEnum.Keybind
            };
            ShowTriggerEditor(newTrigger);
        }

        private void ShowTriggerEditor(TriggerModel trigger)
        {
            try
            {
                TriggerEditorControl triggerEditor = new( (NotSaved.GetValueOrDefault(trigger.Id) ?? trigger).Clone() );

                triggerEditor.OnCancel += OnTriggerCancel;
                triggerEditor.OnEdited += OnTriggerEdited;
                triggerEditor.OnSave += OnTriggerSave;

                PageFrame.Content = triggerEditor;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trigger editor: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool HasUnsavedChanges()
        {
            lock (EditLock)
            {
                return 0 < NotSaved.Count;
            }
        }

        private void OnTriggerCancel(object sender, EventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    NotSaved.Remove(editor.CurrentTriggerModel.Id);
                    ShowTriggerEditor(ProfileManager.CurrentProfile.Triggers.FirstOrDefault(a=> a.Id == editor.CurrentTriggerModel.Id) ?? new());
                }
            }
        }

        private void OnTriggerSave(object sender, InputEditEventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    int index = Array.FindIndex(
                        ProfileManager.CurrentProfile.Triggers,
                        t => t.Id == editor.CurrentTriggerModel.Id
                    );

                    if (-1 < index)
                    {
                        ProfileManager.CurrentProfile.Triggers[index] = editor.CurrentTriggerModel.Clone();
                    }
                    else
                    {
                        ProfileManager.CurrentProfile.Triggers =  [..ProfileManager.CurrentProfile.Triggers, editor.CurrentTriggerModel.Clone()];
                    }

                    ProfileManager.SaveProfile();

                    NotSaved.Remove(editor.CurrentTriggerModel.Id);
                }

                RefreshTriggerMenu();
            }
        }

        private void OnTriggerEdited(object sender, EventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    NotSaved[editor.CurrentTriggerModel.Id] = editor.CurrentTriggerModel;
                }
            }
        }
    }
}