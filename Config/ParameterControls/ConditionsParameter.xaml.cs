using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Config.EditorControls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Config.ParameterControls
{
    public partial class ConditionsParameter : BaseParameterControl
    {
        private ConditionModel[] ConditionsValue = [];

        public ConditionModel[] Value
        {
            get => ConditionsValue;
            set
            {
                if (ConditionsValue == value
                    || ConditionsValue.SequenceEqual(value))
                { return; }

                ConditionsValue = value;

                UpdateConditionsList();
                AnnouncePropertyChanged(ValueName);
            }
        }

        public ConditionsParameter()
        {
            HeaderText = "Conditions";
            LabelText = String.Empty;

            InitializeComponent();
            this.DataContext = this;
        }

        public ConditionsParameter(ConditionModel[]? conditions, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (headerText is not null)
            { HeaderText = headerText; }

            if (labelText is not null)
            { LabelText = labelText; }

            if (customValueName is not null)
            { ValueName = customValueName; }

            if (conditions is not null)
            { ConditionsValue = conditions; }

            UpdateConditionsList();

            if (eventHandler is not null)
            { PropertyChanged += eventHandler; }
        }

        private void UpdateConditionsList()
        {
            try
            {
                ConditionsListBox.ItemsSource = ConditionsValue;
                
                ConditionsListBox.Loaded += (s, e) => UpdateDisplayTexts();
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
                        textBlock.Text = ConditionalModel.GetDisplayText(condition);
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
                if (ConditionsValue is null)
                {
                    MessageBox.Show("No model is currently set.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var conditionModel = new ConditionModel
                {
                    ConditionType = ConditionTypeEnum.ProcessRunning
                };

                var conditionEditor = new ConditionEditorControl(conditionModel);

                conditionEditor.OnSave += (s, args) =>
                {
                    if (args.ConditionModel is ConditionModel updatedCondition)
                    {
                        Value = [..ConditionsValue, updatedCondition];
                        Log.WriteLine($"Condition {updatedCondition.ConditionType} added successfully");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    }
                };

                conditionEditor.OnEdited += (s, args) =>
                {
                    Log.WriteLine($"Condition edited for {conditionModel.ConditionType}");
                    
                };

                conditionEditor.OnCancel += (s, args) =>
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
                if (sender is Button button && button.Tag is ConditionModel condition && ConditionsValue != null)
                {
                    Value = ConditionsValue.Where(c => c != condition).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is ConditionModel condition && ConditionsValue != null)
                {
                    var conditionEditor = new ConditionEditorControl(condition.Clone());

                    conditionEditor.OnSave += (s, args) =>
                    {
                        if (args.ConditionModel is ConditionModel updatedCondition && null != ConditionsValue)
                        {
                            int index = ConditionsValue.IndexOf(condition);
                            if (-1 < index)
                            {
                                ConditionsValue[index] = updatedCondition;
                                UpdateConditionsList();
                                AnnouncePropertyChanged(ValueName);
                                Log.WriteLine($"Condition {updatedCondition.ConditionType} updated successfully");
                            }
                            ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                        }
                    };

                    conditionEditor.OnEdited += (s, args) =>
                    {
                        Log.WriteLine($"Condition edited for {condition.ConditionType}");
                        
                    };

                    conditionEditor.OnCancel += (s, args) =>
                    {
                        Log.WriteLine($"Condition edit cancelled for {condition.ConditionType}");
                        ((MainWindow)Application.Current.MainWindow)?.HideOverlay();
                    };

                    ((MainWindow)Application.Current.MainWindow)?.ShowOverlay(conditionEditor);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing condition: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RefreshConditions()
        {
            UpdateConditionsList();
        }
    }
}