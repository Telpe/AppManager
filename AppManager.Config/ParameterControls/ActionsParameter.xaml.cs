using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Config.EditorControls;
using AppManager.Config.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Config.ParameterControls
{
    public partial class ActionsParameter : BaseParameterControl
    {
        private ActionModel[] ActionsValue = [];

        public ActionModel[] Value
        {
            get => ActionsValue;
            set
            {
                if (ActionsValue == value 
                    || ActionsValue.SequenceEqual(value)) 
                { return; }
                
                ActionsValue = value;

                UpdateActionsList();
                AnnouncePropertyChanged(ValueName);
            }
        }

        public ActionsParameter()
        {
            _headerText = "Actions";
            _labelText = String.Empty;

            InitializeComponent();
            this.DataContext = this;
        }

        public ActionsParameter(ActionModel[]? actions, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (headerText is not null)
            { _headerText = headerText; }

            if (labelText is not null)
            { _labelText = labelText; }

            if (customValueName is not null)
            { ValueName = customValueName; }

            if (actions is not null)
            { ActionsValue = actions; }

            UpdateActionsList();

            if (eventHandler is not null)
            { PropertyChanged += eventHandler; }
        
        }

        private void UpdateActionsList()
        {
            try
            {
                ActionsListBox.ItemsSource = ActionsValue;
                
                ActionsListBox.Loaded += (s, e) => UpdateDisplayTexts();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"UpdateActionsList error: {ex.Message}");
            }
        }

        private void UpdateDisplayTexts()
        {
            if (ActionsListBox.ItemsSource is null) { return; }

            for (int i = 0; i < ActionsListBox.Items.Count; i++)
            {
                if (ActionsListBox.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem container)
                {
                    if (FindVisualChild<TextBlock>(container, "DisplayTextBlock") is TextBlock textBlock
                        && ActionsListBox.Items[i] is ActionModel action)
                    {
                        textBlock.Text = "Missing text";
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) { return null; }

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild && child is FrameworkElement element && element.Name == childName) { return typedChild; }

                var foundChild = FindVisualChild<T>(child, childName);
                if (foundChild != null) { return foundChild; }
            }

            return null;
        }

        private void AddActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ActionsValue is null)
                {
                    MessageBox.Show("No model is currently set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var actionModel = new ActionModel
                {
                    ActionType = ActionTypeEnum.Launch
                };

                var actionEditor = new ActionEditorControl(actionModel);

                actionEditor.OnSave += (s, args) =>
                {
                    if (args.ActionModel is ActionModel updatedAction)
                    {
                        Value = [..ActionsValue, updatedAction];
                        Log.WriteLine($"Action {updatedAction.ActionType} added successfully");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    }
                };

                actionEditor.OnEdited += (s, args) =>
                {
                    Log.WriteLine($"Action edited for {actionModel.ActionType}");
                };

                actionEditor.OnCancel += (s, args) =>
                {
                    Log.WriteLine($"Action cancelled for {actionModel.ActionType}");
                    ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                };

                ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(actionEditor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding action: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ActionModel action && ActionsValue != null)
                {
                    Value = ActionsValue.Where(a => a != action).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing action: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ActionModel action && ActionsValue != null)
                {
                    var actionEditor = new ActionEditorControl(action.Clone());

                    actionEditor.OnSave += (s, args) =>
                    {
                        if (args.ActionModel is ActionModel updatedAction && null != ActionsValue)
                        {
                            int index = ActionsValue.IndexOf(action);
                            if (-1 < index)
                            {
                                ActionsValue[index] = updatedAction;
                                UpdateActionsList();
                                AnnouncePropertyChanged(ValueName);
                                Log.WriteLine($"Action {updatedAction.ActionType} updated successfully");
                            }
                            ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                        }
                    };

                    actionEditor.OnEdited += (s, args) =>
                    {
                        Log.WriteLine($"Action edited for {action.ActionType}");
                    };

                    actionEditor.OnCancel += (s, args) =>
                    {
                        Log.WriteLine($"Action edit cancelled for {action.ActionType}");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    };

                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(actionEditor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing action: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshActions()
        {
            UpdateActionsList();
        }
    }
}