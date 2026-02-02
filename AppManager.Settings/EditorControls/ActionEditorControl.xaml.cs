using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Settings.Interfaces;
using AppManager.Settings.ParameterControls;
using AppManager.Settings.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace AppManager.Settings.EditorControls
{
    public partial class ActionEditorControl : UserControl, IInputEditControl
    {
        private ActionModel CurrentActionModelValue;

        public event EventHandler? Edited;

        public event EventHandler? Cancel;

        public event EventHandler<InputEditEventArgs>? Save;

        public ActionModel CurrentActionModel
        {
            get => CurrentActionModelValue;
            set
            {
                CurrentActionModelValue = value;
                LoadActionData();
            }
        }

        public ActionEditorControl(ActionModel actionModel)
        {
            CurrentActionModelValue = actionModel;

            InitializeComponent();

            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"ActionEditorControl initialization error: {ex.Message}");
                InitializeManually();
            }

            
        }

        private void Initialize()
        {
            InitializeComboBoxes();
            LoadActionData();
        }

        private void InitializeManually()
        {
            var grid = new Grid();
            var textBlock = new TextBlock
            {
                Text = "ActionEditorControl - XAML Loading Failed",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(textBlock);
            Content = grid;

        }

        private void InitializeComboBoxes()
        {
            try
            {
                ActionTypeComboBox?.ItemsSource = ActionFactory.GetSupportedActionTypes();
                
            }
            catch (Exception ex)
            {
                Log.WriteLine($"InitializeComboBoxes error: {ex.Message}");
            }
        }


        private void LoadActionData()
        {
            try
            {
                ActionTypeComboBox!.SelectedItem = CurrentActionModelValue.ActionType;


                UpdatePreview();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"LoadActionData error: {ex.Message}");
            }
        }

        private void UpdatePreview()
        {
            try
            {
                if (PreviewTextBlock == null)
                {
                    return;
                }

                var preview = $"Action Type: {CurrentActionModelValue.ActionType}\n" +
                             $"App Name: {CurrentActionModelValue.AppName ?? "Not specified"}\n" +
                             $"Window Title: {CurrentActionModelValue.WindowTitle ?? "Not specified"}\n";

                if (!string.IsNullOrEmpty(CurrentActionModelValue.ExecutablePath))
                {
                    preview += $"Executable: {CurrentActionModelValue.ExecutablePath}\n";
                }

                if (!string.IsNullOrEmpty(CurrentActionModelValue.Arguments))
                {
                    preview += $"Arguments: {CurrentActionModelValue.Arguments}\n";
                }

                preview += $"Force Operation: {CurrentActionModelValue.ForceOperation}\n" +
                          $"Include Child Processes: {CurrentActionModelValue.IncludeChildProcesses}\n" +
                          $"Include Similar Names: {CurrentActionModelValue.IncludeSimilarNames}\n" +
                          $"Timeout: {CurrentActionModelValue.TimeoutMs}ms\n\n";

                if (CurrentActionModelValue.Conditions?.Length > 0)
                {
                    preview += "Conditions:\n";
                    foreach (var condition in CurrentActionModelValue.Conditions)
                    {
                        preview += $"  - {condition.ConditionType}: {GetConditionDescription(condition)}\n";
                    }
                }
                else
                {
                    preview += "No conditions specified";
                }

                PreviewTextBlock.Text = preview;
            }
            catch (Exception ex)
            {
                if (PreviewTextBlock != null)
                {
                    PreviewTextBlock.Text = $"Error generating preview: {ex.Message}";
                }
            }
        }

        private string GetConditionDescription(ConditionModel condition)
        {
            return condition.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process '{condition.ProcessName}' is running",
                ConditionTypeEnum.FileExists => $"File '{condition.FilePath}' exists",
                _ => "Unknown condition"
            };
        }


        private void AddParametersFromInterface(Type modelType)
        {
            foreach (PropertyInfo prop in modelType.GetProperties())
            {
                switch (prop.Name)
                {
                    case nameof(ActionModel.AppName):

                        ((ProcessParameter)ActionParameters.Children[
                                ActionParameters.Children.Add(new ProcessParameter(CurrentActionModel.AppName, "App Name:"))
                            ]).PropertyChanged += (s, e) => { TextBox_TextChanged(s ?? this, e); };
                        break;

                    case nameof(ActionModel.Conditions):

                        ((ConditionsParameter)ActionParameters.Children[
                                ActionParameters.Children.Add(new ConditionsParameter(CurrentActionModel.Conditions ?? []))
                            ]).PropertyChanged += ConditionsChanged;
                        break;

                    case nameof(ActionModel.ExecutablePath):

                        ((FilePathParameter)ActionParameters.Children[
                                ActionParameters.Children.Add(new FilePathParameter(CurrentActionModel.ExecutablePath, "Executable Path:"))
                            ]).PropertyChanged += (s, e) => { TextBox_TextChanged(s ?? this, e); };
                        break;

                    case nameof(ActionModel.Arguments):
                        
                        AddTextBoxRow(CurrentActionModel.WindowTitle ?? String.Empty, "Arguments:");
                        break;

                    case nameof(ActionModel.ForceOperation):

                        AddCheckBoxRow(CurrentActionModel.ForceOperation, "Force Operation:");
                        break;

                    case nameof(ActionModel.IncludeChildProcesses):

                        AddCheckBoxRow(CurrentActionModel.IncludeChildProcesses, "Include Child Processes:");
                        break;

                    case nameof(ActionModel.IncludeSimilarNames):

                        AddCheckBoxRow(CurrentActionModel.IncludeSimilarNames, "Include Similar Names:");
                        break;

                    case nameof(ActionModel.TimeoutMs):

                        ((TimerParameter)ActionParameters.Children[
                                ActionParameters.Children.Add(new TimerParameter(CurrentActionModel.TimeoutMs ?? -1, "Timeout (ms):"))
                            ]).PropertyChanged += TimeoutParameter_ValueChanged;
                        break;

                    case nameof(ActionModel.WindowTitle):

                        AddTextBoxRow(CurrentActionModel.WindowTitle ?? String.Empty, "Window Title:");
                        break;

                    default:

                        if (prop.PropertyType == typeof(string))
                        {
                            AddTextBoxRow((string?)prop.GetValue(CurrentActionModel) ?? String.Empty, prop.Name);
                        }
                        else if (prop.PropertyType == typeof(bool))
                        {
                            AddCheckBoxRow((bool?)prop.GetValue(CurrentActionModel), prop.Name);
                        }
                        break;
                }
            }
        }

        private void AddTextBoxRow(string text, string header)
        {
            ((TextBox)((GroupBox)ActionParameters.Children[
                                ActionParameters.Children.Add(new GroupBox
                                {
                                    Header = header,
                                    Content = new TextBox { Text = text }
                                })
                            ]).Content).TextChanged += TextBox_TextChanged;

        }

        private void AddCheckBoxRow(bool? isChecked, string header)
        {
            var chkbox = new CheckBox 
            { 
                IsChecked = isChecked,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };

            ActionParameters.Children.Add(new GroupBox
            {
                Header = header,
                Content = chkbox
            });

            chkbox.Checked += CheckBox_Changed;
            chkbox.Unchecked += CheckBox_Changed;
        }
                

        protected void BroadcastEdited()
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }

        protected void BroadcastCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        protected void BroadcastSave()
        {
            Save?.Invoke(this, new InputEditEventArgs(CurrentActionModelValue.Clone()));
        }


        private void TestActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var canExecute = ActionFactory.CreateAction(CurrentActionModelValue).Execute();

                var result = canExecute ? "✓ Action can be executed" : "✗ Action cannot be executed";
                MessageBox.Show(result, "Test Result", MessageBoxButton.OK,
                    canExecute ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test failed: {ex.Message}", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var errors = new List<string>();

                var message = errors.Any() ?
                    $"Validation failed:\n{string.Join("\n", errors)}" :
                    "✓ Validation passed";

                var icon = errors.Any() ? MessageBoxImage.Warning : MessageBoxImage.Information;
                MessageBox.Show(message, "Validation Result", MessageBoxButton.OK, icon);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation failed: {ex.Message}", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            BroadcastSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BroadcastCancel();
        }


        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ActionTypeComboBox?.SelectedItem is AppActionTypeEnum actionType)
                {
                    CurrentActionModelValue.ActionType = actionType;
                    ActionParameters.Children.Clear();

                    switch (actionType)
                    {
                        case AppActionTypeEnum.Launch:
                            AddParametersFromInterface(typeof(ILaunchAction));
                            break;
                        case AppActionTypeEnum.Close:
                            AddParametersFromInterface(typeof(ICloseAction));
                            break;
                        case AppActionTypeEnum.Restart:
                            AddParametersFromInterface(typeof(IRestartAction));
                            break;
                        case AppActionTypeEnum.Minimize:
                            AddParametersFromInterface(typeof(IMinimizeAction));
                            break;
                        case AppActionTypeEnum.Focus:
                            AddParametersFromInterface(typeof(IFocusAction));
                            break;
                        case AppActionTypeEnum.BringToFront:
                            AddParametersFromInterface(typeof(IBringToFrontAction));
                            break;
                    }

                }

                UpdatePreview();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"ActionTypeComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Name))
            {
                var propertyName = textBox.Name.Replace("TextBox", "");
                var property = CurrentActionModelValue.GetType().GetProperty(propertyName);
                
                if (property != null)
                {
                    if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(textBox.Text, out int intValue))
                        {
                            property.SetValue(CurrentActionModelValue, intValue);
                        }
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(CurrentActionModelValue, textBox.Text);
                    }
                }

                ParameterChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                var propertyName = checkBox.Name.Replace("CheckBox", "");
                var property = CurrentActionModelValue.GetType().GetProperty(propertyName);
                
                if (property != null && property.PropertyType == typeof(bool))
                {
                    property.SetValue(CurrentActionModelValue, checkBox.IsChecked == true);
                }

                ParameterChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void ConditionsChanged(object? sender, EventArgs e)
        {
            if (sender is ConditionsParameter cp)
            {
                CurrentActionModelValue.Conditions = cp.Value;
                ParameterChanged(sender, new PropertyChangedEventArgs(nameof(ActionModel.Conditions)));
            }
        }

        private void TimeoutParameter_ValueChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TimerParameter tp)
            {
                CurrentActionModelValue.TimeoutMs = tp.Value;
                ParameterChanged(sender, e);
            }
            
        }

        private void ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdatePreview();
            BroadcastEdited();
        }
    }
}