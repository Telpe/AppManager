using AppManager;
using AppManager.Actions;
using AppManager.Profile;
using AppManager.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;

namespace AppManager.Pages
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

        public AppPage()
        {
            InitializeComponent();

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
            TriggersListBox.Items.Clear();
            foreach (var trigger in _CurrentModel.AppTriggers.Values)
            {
                TriggersListBox.Items.Add($"Trigger: {trigger.TriggerType}");
            }
        }

        private void RefreshActionsListBox()
        {
            ActionsListBox.Items.Clear();
            foreach (var action in _CurrentModel.AppActions.Values)
            {
                ActionsListBox.Items.Add($"Action: {action.ActionName}");
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
