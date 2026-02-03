using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Settings.Utilities;
using AppManager.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using AppManager.Settings.EditorControls;
using AppManager.Settings.Models;
using AppManager.Settings.Interfaces;

namespace AppManager.Settings.Pages
{
    /// <summary>
    /// Interaction logic for Apps.xaml
    /// </summary>
    public partial class AppsPage : Page, IPageWithParameter
    {
        private string PageNameValue = "";
        private static int MaxBackupModelsValue = 5;

        private Dictionary<string, AppPageModel> LoadedPagesValue = new();
        private AppPageModel CurrentPageValue { get; set; } = new AppPageModel(new AppManagedModel("None", false));
        private AppManagedModel CurrentModelValue
        { 
            get { return CurrentPageValue.CurrentModel; } 
            set { CurrentPageValue.CurrentModel = value; }
        }

        public AppManagedModel CurrentModel { get { return CurrentModelValue; } }

        public bool IsActive
        {
            get { return CurrentModelValue.Active; }
            set { CurrentModelValue.Active = value; }
        }

        private ObservableCollection<ModelListItem<TriggerModel>> TriggerListItemsValue = new();
        

        public AppsPage()
        {
            InitializeComponent();

            TriggersListBox.ItemsSource = TriggerListItemsValue;
            TriggersListBox.AddHandler(Button.ClickEvent, new RoutedEventHandler(EditTriggerButton_Click));

            DisableButtons();
        }

        public void LoadPageByName(string pageName)
        {
            DisableButtons();

            PageNameValue = pageName;

            try
            {
                if (!LoadedPagesValue.TryGetValue(pageName, out AppPageModel? pageModel))
                {
                    pageModel = new AppPageModel(ProfileManager.CurrentProfile.Apps.Where(app => app.AppName == pageName).FirstOrDefault() ?? throw new Exception("App not in profile."));
                    LoadedPagesValue[pageName] = pageModel;
                }

                CurrentPageValue = pageModel;

                LoadFromModel();
                
            }
            catch (Exception ex)
            {
                Log.WriteLine("AppPage initialization error");
                Log.WriteLine(ex.Message.ToString());
                Log.WriteLine(ex.StackTrace?.ToString()??"No stack");
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
            AppManagedModel? temp = ProfileManager.CurrentProfile.Apps.Where(app => app.AppName == PageNameValue).FirstOrDefault();
            if (temp != null)
            {
                CurrentPageValue.BackupModels = [temp.Clone(), ..CurrentPageValue.BackupModels.Take(MaxBackupModelsValue - 1)];
            }
        }

        private void LoadFromModel()
        {
            DisableButtons();

            AppNameBox.Text = CurrentModelValue.AppName;
            ActiveBox.IsChecked = CurrentModelValue.Active;
            
            RefreshTriggersListBox();

            EnableButtons();
        }

        private void RefreshTriggersListBox()
        {
            ClearTriggers();
            foreach (var kvp in CurrentModelValue?.Triggers ?? [])
            {
                TriggerListItemsValue.Add(new ModelListItem<TriggerModel>(kvp.Key, kvp.Value, this));
            }
        }

        private void DisableButtons()   
        {
            try
            {
                AppNameBox.TextChanged -= AppNameBox_TextChanged;
                ActiveBox.Checked -= ActiveBox_Changed;
                ActiveBox.Unchecked -= ActiveBox_Changed;
                
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
            AppNameBox.TextChanged += AppNameBox_TextChanged;
            ActiveBox.Checked += ActiveBox_Changed;
            ActiveBox.Unchecked += ActiveBox_Changed;
            
            CancelButton.IsEnabled = !CurrentPageValue.IsStored;
            SaveButton.IsEnabled = !CurrentPageValue.IsStored;
        }

        private void AppNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentModelValue.AppName = AppNameBox.Text;

            Edited();
        }

        private void ActiveBox_Changed(object sender, RoutedEventArgs e)
        {
            CurrentModelValue.Active = ActiveBox.IsChecked ?? false;

            Edited();
        }

        private void AddTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            var newTrigger = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.Keybind
            };

            CurrentModelValue.AddTrigger(newTrigger);


            RefreshTriggersListBox();
            Edited();
        }

        private void EditTriggerButton_Click(object? sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button button && button.Tag is ModelListItem<TriggerModel> viewModel)
            {
                Log.WriteLine($"Edit trigger: {viewModel.DisplayName}");
                
                try
                {
                    // Create trigger editor control directly
                    var triggerEditor = new TriggerEditorControl(viewModel.Model);
                    
                    // Subscribe to save event
                    triggerEditor.Save += (s, updatedTrigger) =>
                    {
                        if (null == updatedTrigger.TriggerModel) { return; }

                        // Update the model in the dictionary
                        CurrentModelValue.Triggers?[viewModel.Id] = updatedTrigger.TriggerModel;

                        // Refresh the UI to show changes
                        RefreshTriggersListBox();
                        
                        // Mark as edited
                        Edited();
                        
                        Log.WriteLine($"Trigger {viewModel.DisplayName} updated successfully");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    };

                    triggerEditor.Edited += (s, args) =>
                    {
                        Log.WriteLine($"Trigger edited for {viewModel.DisplayName}");
                        Edited();
                    };

                    triggerEditor.Cancel += (s, args) =>
                    {
                        Log.WriteLine($"Trigger editing cancelled for {viewModel.DisplayName}");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    };
                    
                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(triggerEditor);
                }
                catch (Exception ex)
                {
                    Log.WriteLine($"Error opening trigger editor: {ex.Message}");
                    MessageBox.Show($"Error opening trigger editor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int id = ProfileManager.CurrentProfile.Apps.Select((app, index) => new { app, index }).Where(x => x.app.AppName == PageNameValue).Select(x => x.index).FirstOrDefault();
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

        public void ClearTriggers()
        {
            //TriggersListBox.RemoveHandler(Button.ClickEvent, new RoutedEventHandler(EditTriggerButton_Click));
            TriggerListItemsValue.Clear();
        }

    }
    
}
