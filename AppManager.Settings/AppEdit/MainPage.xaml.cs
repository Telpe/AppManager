using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Core.Actions;
using AppManager.Settings.Utils;
using AppManager.Settings.AppGroupEdit;
using AppManager.Settings.ActionEdit;

namespace AppManager.Settings.AppEdit
{
    /// <summary>
    /// Interaction logic for Apps.xaml
    /// </summary>
    public partial class MainPage : Page, IPageWithParameter
    {
        private string PageNameStored = "";
        private int MaxBackupModelsStored = 5;

        private Dictionary<string, AppPageModel> LoadedPagesValue = new();
        private AppPageModel CurrentPageValue { get; set; } = new AppPageModel();
        private AppManagedModel CurrentModelValue
        { 
            get { return CurrentPageValue.CurrentModel ?? new AppManagedModel(); } 
            set { CurrentPageValue.CurrentModel = value; }
        }

        private ObservableCollection<TriggerViewModel> TriggerViewModelsValue = new();
        private ObservableCollection<ActionViewModel> ActionViewModelsValue = new();

        public MainPage()
        {
            InitializeComponent();

            // Set ItemsSource for ListBoxes
            TriggersListBox.ItemsSource = TriggerViewModelsValue;
            ActionsListBox.ItemsSource = ActionViewModelsValue;

            DisableButtons();
        }

        public void LoadPageByName(string pageName)
        {
            DisableButtons();

            PageNameStored = pageName;

            try
            {
                if (!LoadedPagesValue.ContainsKey(PageNameStored))
                {
                    var model = ProfileManager.CurrentProfile.Apps.Where(app => app.AppName == PageNameStored).FirstOrDefault();
                    LoadedPagesValue[PageNameStored] = new AppPageModel()
                    {
                        CurrentModel = model.Clone(),
                        BackupModels = [model.Clone()]
                    };
                }

                CurrentPageValue = LoadedPagesValue[PageNameStored];

                // Load data into UI
                LoadFromModel();

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AppPage initialization error");
                Debug.WriteLine(ex.Message.ToString());
                Debug.WriteLine(ex.StackTrace?.ToString()??"No stack");
            }

        }

        private void Edited()
        {
            CurrentPageValue.IsStored = false;
            
            CancelButton.IsEnabled = true;
            SaveButton.IsEnabled = true;
        }

        private void Unedited()
        {

            CurrentPageValue.IsStored = true;
            
            CancelButton.IsEnabled = false;
            SaveButton.IsEnabled = false;
        }

        private void SetBackupModel()
        {
            AppManagedModel? temp = ProfileManager.CurrentProfile?.Apps.Where(app => app.AppName == PageNameStored).FirstOrDefault();
            if (temp != null)
            {
                CurrentPageValue.BackupModels = [temp.Clone(), ..CurrentPageValue.BackupModels.Take(MaxBackupModelsStored - 1)];
            }
        }

        private void LoadFromModel()
        {
            

            // Load UI controls from model (without triggering change events)
            AppNameBox.Text = CurrentModelValue.AppName;
            ActiveBox.IsChecked = CurrentModelValue.Active;
            

            // Refresh the UI lists
            RefreshTriggersListBox();
            RefreshActionsListBox();

            EnableButtons();
        }

        private void RefreshTriggersListBox()
        {
            TriggerViewModelsValue.Clear();
            foreach (var kvp in CurrentModelValue.AppTriggers)
            {
                TriggerViewModelsValue.Add(new TriggerViewModel(kvp.Key, kvp.Value, this));
            }
        }

        private void RefreshActionsListBox()
        {
            ActionViewModelsValue.Clear();
            foreach (var kvp in CurrentModelValue.AppActions)
            {
                ActionViewModelsValue.Add(new ActionViewModel(kvp.Key, kvp.Value, this));
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
            CancelButton.IsEnabled = !CurrentPageValue.IsStored;
            SaveButton.IsEnabled = !CurrentPageValue.IsStored;
        }

        private void AppNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Edited();

            // Update the model directly when text changes
            CurrentModelValue.AppName = AppNameBox.Text;
        }

        private void ActiveBox_Changed(object sender, RoutedEventArgs e)
        {
            Edited();

            // Update the model directly when checkbox changes
            CurrentModelValue.Active = ActiveBox.IsChecked ?? false;
        }

        private void AddTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            Edited();

            var newTrigger = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.Shortcut // Default type
            };

            var triggers = CurrentModelValue.AppTriggers ?? new Dictionary<int, TriggerModel>();
            int newId = triggers.Count > 0 ? triggers.Keys.Max() + 1 : 1;
            triggers.Add(newId, newTrigger);
            CurrentModelValue.AppTriggers = triggers;
            
            RefreshTriggersListBox();
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            Edited();

            var newAction = new ActionModel
            {
                AppName = CurrentModelValue.AppName,
                ActionType = AppActionTypeEnum.Launch // Default action
            };

            var actions = CurrentModelValue.AppActions ?? new Dictionary<int, ActionModel>();
            int newId = actions.Count > 0 ? actions.Keys.Max() + 1 : 1;
            actions.Add(newId, newAction);
            CurrentModelValue.AppActions = actions;
            
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
                        CurrentModelValue.AppTriggers[viewModel.Id] = updatedTrigger;
                        
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
                    var actionEditor = new ActionEditorControl(viewModel.Model.Clone());
                    
                    // Subscribe to save event
                    actionEditor.ActionSaved += (s, updatedAction) =>
                    {
                        // Update the model in the dictionary
                        CurrentModelValue.AppActions[viewModel.Id] = updatedAction;
                        
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
                    MessageBox.Show($"Error opening action editor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (null == ProfileManager.CurrentProfile) { throw new Exception("Unable to save apps, no profile loaded."); }
            // Save profile to file
            int id = ProfileManager.CurrentProfile.Apps.Select((app, index) => new { app, index }).Where(x => x.app.AppName == PageNameStored).Select(x => x.index).FirstOrDefault();
            ProfileManager.CurrentProfile.Apps[id] = CurrentModelValue.Clone();
            ProfileManager.SaveProfile();
            SetBackupModel();

            Unedited();

            Debug.WriteLine($"App {CurrentModelValue.AppName} saved successfully.", "Save Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var model = CurrentPageValue.BackupModels?.FirstOrDefault();
            if (model != null)
            {
                DisableButtons();
                CurrentModelValue = model.Clone();
                LoadFromModel();
                Unedited();
                Debug.WriteLine($"App {CurrentModelValue.AppName} reloaded successfully.", "Reload Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public bool HasUnsavedChanges()
        {
            return LoadedPagesValue?.Values.Any(page => !page.IsStored) ?? false;
        }
    }

    

    
}
