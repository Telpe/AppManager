using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using AppManager.Settings.Actions;
using AppManager.Settings.AppGroups;
using AppManager.Settings.Pages;
using AppManager.Settings.Triggers;
using AppManager.Settings.UI;
using AppManager.Settings.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace AppManager.Settings.Apps
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
        //private ObservableCollection<ModelListItem<ActionModel>> ActionListItemsValue = new();
        

        public AppsPage()
        {
            InitializeComponent();

            // Set ItemsSource for ListBoxes
            TriggersListBox.ItemsSource = TriggerListItemsValue;
            //ActionsListBox.ItemsSource = ActionListItemsValue;

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
            

            // Refresh the UI lists
            RefreshTriggersListBox();
            //RefreshActionsListBox();

            EnableButtons();
        }

        private void RefreshTriggersListBox()
        {
            ClearTriggers();
            foreach (var kvp in CurrentModelValue.AppTriggers)
            {
                TriggerListItemsValue.Add(new ModelListItem<TriggerModel>(kvp.Key, kvp.Value, this));
            }
            Dispatcher.BeginInvoke(() => SubscribeToTriggerButtonEvents(), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void SubscribeToTriggerButtonEvents()
        {
            var buttons = FindEditButtonsInListBox(TriggersListBox);
            foreach (var button in buttons)
            {
                //button.Click -= EditTriggerButton_Click; // Prevent double subscription
                button.Click += EditTriggerButton_Click;
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
                TriggerType = TriggerTypeEnum.Shortcut // Default type
            };

            var triggers = CurrentModelValue.AppTriggers ?? new Dictionary<int, TriggerModel>();
            int newId = triggers.Count > 0 ? triggers.Keys.Max() + 1 : 1;
            triggers.Add(newId, newTrigger);
            CurrentModelValue.AppTriggers = triggers;
            
            RefreshTriggersListBox();
            Edited();
        }

        private void EditTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ModelListItem<TriggerModel> viewModel)
            {
                Debug.WriteLine($"Edit trigger: {viewModel.DisplayName}");
                
                try
                {
                    // Create trigger editor control directly
                    var triggerEditor = new TriggerEditorControl(viewModel.Model);
                    
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
            var buttons = FindEditButtonsInListBox(TriggersListBox);
            foreach (var button in buttons)
            {
                button.Click -= EditTriggerButton_Click;
            }
            TriggerListItemsValue.Clear();
        }

        private IEnumerable<Button> FindEditButtonsInListBox(ListBox listBox)
        {
            var buttons = new List<Button>();

            for (int i = 0; i < listBox.Items.Count; i++)
            {
                var container = listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container, b => b.Content?.ToString() == "Edit");
                    if (button != null)
                    {
                        buttons.Add(button);
                    }
                }
            }

            return buttons;
        }

        private T? FindVisualChild<T>(DependencyObject parent, Func<T, bool>? predicate = null) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T childOfType && (predicate == null || predicate(childOfType)))
                {
                    return childOfType;
                }

                var childOfChild = FindVisualChild<T>(child, predicate);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }
    }

    

    
}
