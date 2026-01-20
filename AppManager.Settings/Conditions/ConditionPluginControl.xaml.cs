using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Settings.Conditions;
using AppManager.Settings.UI;

namespace AppManager.Settings.Conditions
{
    public partial class ConditionPluginControl : UserControl, INotifyPropertyChanged
    {
        private ConditionalModel? _conditionalModel;

        public event EventHandler? ConditionsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ConditionalModel? ConditionalModel
        {
            get => _conditionalModel;
            set
            {
                if (_conditionalModel != value)
                {
                    _conditionalModel = value;
                    OnPropertyChanged(nameof(ConditionalModel));
                    UpdateConditionsList();
                }
            }
        }

        public ConditionPluginControl()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public ConditionPluginControl(ConditionalModel conditionalModel) : this()
        {
            ConditionalModel = conditionalModel;
        }

        private string GetDisplayText(ConditionModel model)
        {
            return model.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process Running: {model.ProcessName}",
                ConditionTypeEnum.FileExists => $"File Exists: {model.FilePath}",
                ConditionTypeEnum.PreviousActionSuccess => $"Previous action result. Only when trigger has multiple actions.",
                _ => $"Unknown Condition: {model.ConditionType}"
            };
        }

        private void UpdateConditionsList()
        {
            try
            {
                ConditionsListBox.ItemsSource = _conditionalModel?.Conditions;
                
                // Update the display text for each item after the template is applied
                ConditionsListBox.Loaded += (s, e) => UpdateDisplayTexts();
                //UpdateDisplayTexts();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"UpdateConditionsList error: {ex.Message}");
            }
        }

        private void UpdateDisplayTexts()
        {
            if (ConditionsListBox.ItemsSource is null) { return; }

            for (int i = 0; i < ConditionsListBox.Items.Count; i++)
            {
                if (ConditionsListBox.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem container)
                {
                    if (FindVisualChild<TextBlock>(container, "DisplayTextBlock") is TextBlock textBlock
                        && ConditionsListBox.Items[i] is ConditionModel condition)
                    {
                        textBlock.Text = GetDisplayText(condition);
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null){ return null; }

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild && child is FrameworkElement element && element.Name == childName)  { return typedChild; }

                var foundChild = FindVisualChild<T>(child, childName);
                if (foundChild != null) { return foundChild; }
            }

            return null;
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_conditionalModel is null)
                {
                    MessageBox.Show("No model is currently set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var conditionModel = new ConditionModel
                {
                    ConditionType = ConditionTypeEnum.None
                };

                var conditionEditor = new ConditionEditorControl(conditionModel);

                conditionEditor.Save += (s, args) =>
                {
                    if (args.ConditionModel is ConditionModel updatedCondition)
                    {
                        _conditionalModel.AddCondition(updatedCondition);
                        UpdateConditionsList();
                        OnConditionsChanged();
                        Log.WriteLine($"Condition {updatedCondition.ConditionType} added successfully");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    }
                };

                conditionEditor.Edited += (s, args) =>
                {
                    Log.WriteLine($"Condition edited for {conditionModel.ConditionType}");
                    OnConditionsChanged();
                };

                conditionEditor.Cancel += (s, args) =>
                {
                    Log.WriteLine($"Condition cancelled for {conditionModel.ConditionType}");
                    ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                };

                ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(conditionEditor);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ConditionModel condition && _conditionalModel != null)
                {
                    _conditionalModel.RemoveCondition(condition);
                    UpdateConditionsList();
                    OnConditionsChanged();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnConditionsChanged()
        {
            ConditionsChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshConditions()
        {
            UpdateConditionsList();
        }
    }
}