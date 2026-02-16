using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Config.EditorControls;
using AppManager.Config.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Config.Utilities;

namespace AppManager.Config.Pages
{
    /// <summary>
    /// Interaction logic for TriggersPage.xaml
    /// </summary>
    public partial class TriggersPage : Page, IPageWithParameter
    {
        private string _selectedTagFilter = "All";
        private readonly List<TriggerModel> _triggers;
        private TriggerModel[] NotSaved = [];
        private object EditLock = new();

        public TriggersPage()
        {
            InitializeComponent();
            _triggers = ProfileManager.CurrentProfile.Triggers?.ToList() ?? new List<TriggerModel>();
            InitializeTagFilter();
            RefreshTriggerMenu();
        }

        public void LoadItemById(string triggerId)
        {
            if (!string.IsNullOrEmpty(triggerId))
            {
                var trigger = _triggers.FirstOrDefault(t => t.Id == triggerId);
                if (trigger != null)
                {
                    ShowTriggerEditor(trigger);
                }
            }
        }

        private void InitializeTagFilter()
        {
            var allTagKeys = new HashSet<string>();
            foreach (var trigger in _triggers)
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

            if (_selectedTagFilter == "All")
            {
                // Show all triggers in a simple list
                foreach (var trigger in _triggers.OrderBy(t => t.Id))
                {
                    var button = CreateTriggerButton(trigger);
                    AnonymousStackPanel.Children.Add(button);
                }
                AnonymousGroup.Header = "All Triggers";
                AnonymousGroup.Visibility = Visibility.Visible;
            }
            else
            {
                // Group by tag values
                var triggersWithTag = _triggers.Where(t => t.Tags?.ContainsKey(_selectedTagFilter) == true).ToList();
                var triggersWithoutTag = _triggers.Where(t => t.Tags?.ContainsKey(_selectedTagFilter) != true).ToList();

                // Group triggers by tag value
                var groups = triggersWithTag
                    .GroupBy(t => t.Tags[_selectedTagFilter])
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

                // Add triggers without the selected tag to anonymous group
                if (triggersWithoutTag.Any())
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

        private void ShowTriggerEditor(TriggerModel trigger)
        {
            try
            {
                TriggerModel? editedTrigger = NotSaved.FirstOrDefault(t => t.Id == trigger.Id);

                if (editedTrigger is not null)
                {
                    trigger = editedTrigger;
                }

                TriggerEditorControl triggerEditor = new(trigger.Clone());

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
                return 0 < NotSaved.Length;
            }
        }

        private void OnTriggerCancel(object sender, EventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    NotSaved = NotSaved.Where(a => editor.CurrentTriggerModel.Id != a.Id).ToArray();
                }
            }
        }

        private void OnTriggerSave(object sender, InputEditEventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    bool notThere = true;

                    for (int i = 0; i < ProfileManager.CurrentProfile.Triggers.Length; i++)
                    {
                        if (ProfileManager.CurrentProfile.Triggers[i].Id == editor.CurrentTriggerModel.Id)
                        {
                            ProfileManager.CurrentProfile.Triggers[i] = editor.CurrentTriggerModel.Clone();
                            notThere = false;
                        }
                    }

                    if (notThere)
                    {
                        ProfileManager.CurrentProfile.Triggers = ProfileManager.CurrentProfile.Triggers.Append(editor.CurrentTriggerModel.Clone()).ToArray();
                    }

                    ProfileManager.SaveProfile();

                    NotSaved = NotSaved.Where(a => editor.CurrentTriggerModel.Id != a.Id).ToArray();
                }
            }
        }

        private void OnTriggerEdited(object sender, EventArgs eve)
        {
            lock (EditLock)
            {
                if (sender is TriggerEditorControl editor)
                {
                    bool notThere = true;

                    for (int i = 0; i < NotSaved.Length; i++)
                    {
                        if (NotSaved[i].Id == editor.CurrentTriggerModel.Id)
                        {
                            NotSaved[i] = editor.CurrentTriggerModel;
                            notThere = false;
                        }
                    }

                    if (notThere)
                    {
                        NotSaved = NotSaved.Append(editor.CurrentTriggerModel).ToArray();
                    }
                }
            }
        }
    }
}