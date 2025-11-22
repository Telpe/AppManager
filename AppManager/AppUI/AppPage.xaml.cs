using AppManager;
using AppManager.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AppManager.Pages;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Core.Actions;
using AppManager.Utils;

namespace AppManager.AppUI
{
    /// <summary>
    /// Interaction logic for Apps.xaml
    /// </summary>
    public partial class AppPage : Page, IPageWithParameter
    {
        private string _PageName = "";
        private int _MaxBackupModels = 5;

        private Dictionary<string, AppPageModel> _LoadedPages = new();
        private AppPageModel _CurrentPage { get; set; } = new AppPageModel();
        private AppManagedModel _CurrentModel
        { 
            get { return _CurrentPage.CurrentModel ?? new AppManagedModel(); } 
            set { _CurrentPage.CurrentModel = value; }
        }

        private ObservableCollection<TriggerViewModel> _TriggerViewModels = new();
        private ObservableCollection<ActionViewModel> _ActionViewModels = new();

        public AppPage()
        {
            InitializeComponent();

            // Set ItemsSource for ListBoxes
            TriggersListBox.ItemsSource = _TriggerViewModels;
            ActionsListBox.ItemsSource = _ActionViewModels;

            DisableButtons();
        }

        public void LoadPageByName(string pageName)
        {
            DisableButtons();

            _PageName = pageName;

            try
            {
                if (!_LoadedPages.ContainsKey(_PageName))
                {
                    var model = ProfileManager.CurrentProfile.Apps.Where(app => app.AppName == _PageName).FirstOrDefault();
                    _LoadedPages[_PageName] = new AppPageModel()
                    {
                        CurrentModel = model.Clone(),
                        BackupModels = [model.Clone()]
                    };
                }

                _CurrentPage = _LoadedPages[_PageName];

                // Load data into UI
                LoadFromModel();

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AppPage initialization error");
                Debug.WriteLine(ex.Message.ToString());
                Debug.WriteLine(ex.StackTrace.ToString());
            }

        }

        private void Edited()
        {
            _CurrentPage.IsStored = false;
            
            CancelButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }

        private void Unedited()
        {

            _CurrentPage.IsStored = true;
            
            CancelButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
        }

        private void SetBackupModel()
        {
            AppManagedModel temp = ProfileManager.CurrentProfile.Apps.Where(app => app.AppName == _PageName).FirstOrDefault();
            if (temp != null)
            {
                _CurrentPage.BackupModels = [temp.Clone(), .._CurrentPage.BackupModels.Take(_MaxBackupModels - 1)];
            }
        }

        private void LoadFromModel()
        {
            

            // Load UI controls from model (without triggering change events)
            AppNameBox.Text = _CurrentModel.AppName;
            ActiveBox.IsChecked = _CurrentModel.Active;
            

            // Refresh the UI lists
            RefreshTriggersListBox();
            RefreshActionsListBox();

            EnableButtons();
        }

        private void RefreshTriggersListBox()
        {
            _TriggerViewModels.Clear();
            foreach (var kvp in _CurrentModel.AppTriggers)
            {
                _TriggerViewModels.Add(new TriggerViewModel(kvp.Key, kvp.Value, this));
            }
        }

        private void RefreshActionsListBox()
        {
            _ActionViewModels.Clear();
            foreach (var kvp in _CurrentModel.AppActions)
            {
                _ActionViewModels.Add(new ActionViewModel(kvp.Key, kvp.Value, this));
            }
        }
        private void DisableButtons()   
        {
            try
            {
                AppNameBox.TextChanged -= AppNameBox_TextChanged;
                ActiveBox.Checked -= ActiveBox_Changed;
                ActiveBox.Unchecked -= ActiveBox_Changed;
                
                // Set Cancel and Save buttons based on IsStored status
                CancelButton.IsEnabled = false;
                SaveButton.IsEnabled = false;
            }
            catch
            {                 
                // Ignore errors
            }
        }

        private void EnableButtons()
        {
            // Initialize button events if needed
            AppNameBox.TextChanged += AppNameBox_TextChanged;
            ActiveBox.Checked += ActiveBox_Changed;
            ActiveBox.Unchecked += ActiveBox_Changed;
            
            // Set Cancel and Save buttons based on IsStored status
            CancelButton.IsEnabled = !_CurrentPage.IsStored;
            SaveButton.IsEnabled = !_CurrentPage.IsStored;
        }

        private void AppNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Edited();

            // Update the model directly when text changes
            _CurrentModel.AppName = AppNameBox.Text;
        }

        private void ActiveBox_Changed(object sender, RoutedEventArgs e)
        {
            Edited();

            // Update the model directly when checkbox changes
            _CurrentModel.Active = ActiveBox.IsChecked ?? false;
        }

        private void AddTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            Edited();

            var newTrigger = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.Shortcut // Default type
            };

            var triggers = _CurrentModel.AppTriggers ?? new Dictionary<int, TriggerModel>();
            int newId = triggers.Count > 0 ? triggers.Keys.Max() + 1 : 1;
            triggers.Add(newId, newTrigger);
            _CurrentModel.AppTriggers = triggers;
            
            RefreshTriggersListBox();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            Edited();

            var newAction = new ActionModel
            {
                AppName = _CurrentModel.AppName,
                ActionName = AppActionEnum.Launch // Default action
            };

            var actions = _CurrentModel.AppActions ?? new Dictionary<int, ActionModel>();
            int newId = actions.Count > 0 ? actions.Keys.Max() + 1 : 1;
            actions.Add(newId, newAction);
            _CurrentModel.AppActions = actions;
            
            RefreshActionsListBox();
        }

        private void EditTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TriggerViewModel viewModel)
            {
                Debug.WriteLine($"Edit trigger: {viewModel.DisplayName}");
                
                try
                {
                    // Create trigger editor control directly
                    var triggerEditor = new TriggerEditorControl();
                    triggerEditor.CurrentTrigger = viewModel.Model;
                    
                    // Subscribe to save event
                    triggerEditor.TriggerSaved += (s, updatedTrigger) =>
                    {
                        // Update the model in the dictionary
                        _CurrentModel.AppTriggers[viewModel.Id] = updatedTrigger;
                        
                        // Refresh the UI to show changes
                        RefreshTriggersListBox();
                        
                        // Mark as edited
                        Edited();
                        
                        Debug.WriteLine($"Trigger {viewModel.DisplayName} updated successfully");
                    };
                    
                    triggerEditor.TriggerCancelled += (s, args) =>
                    {
                        Debug.WriteLine($"Trigger editing cancelled for {viewModel.DisplayName}");
                    };
                    
                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(triggerEditor, 80, 70, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening trigger editor: {ex.Message}");
                    MessageBox.Show($"Error opening trigger editor: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ActionViewModel viewModel)
            {
                Debug.WriteLine($"Edit action: {viewModel.DisplayName}");
                
                try
                {
                    // Create action editor control directly
                    var actionEditor = new ActionEditorControl();
                    actionEditor.CurrentAction = viewModel.Model;
                    
                    // Subscribe to save event
                    actionEditor.ActionSaved += (s, updatedAction) =>
                    {
                        // Update the model in the dictionary
                        _CurrentModel.AppActions[viewModel.Id] = updatedAction;
                        
                        // Refresh the UI to show changes
                        RefreshActionsListBox();
                        
                        // Mark as edited
                        Edited();
                        
                        Debug.WriteLine($"Action {viewModel.DisplayName} updated successfully");
                    };
                    
                    actionEditor.ActionCancelled += (s, args) =>
                    {
                        Debug.WriteLine($"Action editing cancelled for {viewModel.DisplayName}");
                    };
                    
                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(actionEditor, 80, 70, false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error opening action editor: {ex.Message}");
                    MessageBox.Show($"Error opening action editor: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save profile to file
            int id = ProfileManager.CurrentProfile.Apps.Select((app, index) => new { app, index }).Where(x => x.app.AppName == _PageName).Select(x => x.index).FirstOrDefault();
            ProfileManager.CurrentProfile.Apps[id] = _CurrentModel.Clone();
            ProfileManager.SaveProfile();
            SetBackupModel();

            Unedited();

            Debug.WriteLine($"App {_CurrentModel.AppName} saved successfully.", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var model = _CurrentPage.BackupModels?.FirstOrDefault();
            if (model != null)
            {
                DisableButtons();
                _CurrentModel = model.Clone();
                LoadFromModel();
                Unedited();
                Debug.WriteLine($"App {_CurrentModel.AppName} reloaded successfully.", "Reload Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public bool HasUnsavedChanges()
        {
            return _LoadedPages?.Values.Any(page => !page.IsStored) ?? false;
        }
    }

    

    
}
