using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Settings.Interfaces;
using AppManager.Settings.ParameterControls;
using AppManager.Settings.Utilities;
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
            Initialize();

            
        }

        private void Initialize()
        {
            try
            {
                TypeSelectionGroupBox!.Content = new TypeSelectParameter(typeof(AppActionTypeEnum), null, null, "Choose:");
                (TypeSelectionGroupBox!.Content as TypeSelectParameter)!.PropertyChanged += ActionTypeComboBox_SelectionChanged;

                (TypeSelectionGroupBox!.Content as TypeSelectParameter)!.Selected = CurrentActionModelValue.ActionType;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"ActionEditorControl initialization error: {ex.Message}");
                InitializeManually();
            }
            
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


        private void LoadActionData()
        {
            try
            {
                ActionParameters.Children.Clear();

                ActionConditions.Content = new ConditionsParameter(CurrentActionModelValue.Conditions ?? []);

                (ActionConditions.Content as ConditionsParameter)!.PropertyChanged += ConditionsChanged;

                if (TypeSelectionGroupBox!.Content is TypeSelectParameter { Selected: AppActionTypeEnum actionType })
                {
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

                        ProcessParameter pp = new(CurrentActionModel.AppName);
                        UseParameterGroupBox("App Name:").Content = pp;
                        pp.PropertyChanged += (s, e) => { TextBox_TextChanged(s ?? this, e); };
                        break;

                    case nameof(ActionModel.ExecutablePath):

                        FilePathParameter fpp = new(CurrentActionModel.ExecutablePath);
                        UseParameterGroupBox("Executable Path:").Content = fpp;
                        fpp.PropertyChanged += (s, e) => { TextBox_TextChanged(s ?? this, e); };
                        break;

                    case nameof(ActionModel.Arguments):
                        
                        AddTextBoxRow(CurrentActionModel.WindowTitle ?? String.Empty, "Arguments:");
                        break;

                    case nameof(ActionModel.ForceOperation):

                        AddCheckBoxRow(CurrentActionModel.ForceOperation, "Advanced:", "Force Operation:");
                        break;

                    case nameof(ActionModel.IncludeChildProcesses):

                        AddCheckBoxRow(CurrentActionModel.IncludeChildProcesses, "Advanced:", "Include Child Processes:");
                        break;

                    case nameof(ActionModel.IncludeSimilarNames):

                        AddCheckBoxRow(CurrentActionModel.IncludeSimilarNames, "Advanced:", "Include Similar Names:");
                        break;

                    case nameof(ActionModel.TimeoutMs):

                        TimerParameter tp = new(CurrentActionModel.TimeoutMs ?? -1);
                        UseParameterGroupBox("Timeout (ms):").Content = tp;
                        tp.PropertyChanged += TimeoutParameter_ValueChanged;
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

        private GroupBox UseParameterGroupBox(string header)
        {
            GroupBox? box = null;

            foreach (object aBox in ActionParameters.Children)
            {
                if (aBox is GroupBox gb 
                    && gb.Header is string sh 
                    && sh == header)
                {
                    box = gb; 
                    break;
                }
            }

            if (box is null)
            {
                box = new GroupBox
                {
                    Header = header
                };

                ActionParameters.Children.Add(box);
            }

            return box;
        }

        private StackPanel UseParameterStackPanel(string header, Orientation orientation = Orientation.Vertical)
        {
            GroupBox box = UseParameterGroupBox(header);

            if (box.Content is StackPanel sp)
            {
                return sp;
            }

            if (box.Content is null)
            {
                box.Content = new StackPanel
                {
                    Orientation = orientation
                };

                return (StackPanel)box.Content;
            }

            throw new InvalidOperationException($"GroupBox with header '{header}', has Content that is not of type {typeof(StackPanel).Name}");
        }

        private void AddTextBoxRow(string text, string header, string? labelText = null)
        {
            TextBox textBox = new()
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center
            };

            UseParameterStackPanel(header).Children.Add(labelText is null ? textBox : ToLabeled(textBox, labelText));

            textBox.TextChanged += TextBox_TextChanged;
        }

        private void AddCheckBoxRow(bool? isChecked, string header, string? labelText = null)
        {
            CheckBox chkbox = new() 
            { 
                IsChecked = isChecked,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };

            UseParameterStackPanel(header, Orientation.Horizontal).Children.Add( labelText is null ? chkbox : ToLabeled(chkbox, labelText) );

            chkbox.Checked += CheckBox_Changed;
            chkbox.Unchecked += CheckBox_Changed;
        }

        private StackPanel ToLabeled(UIElement element, string labelText)
        {
            Label label = new() { Content = labelText };

            StackPanel sp = new() { Orientation = Orientation.Horizontal };

            sp.Children.Add(label);
            sp.Children.Add(element);

            return sp;
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


        private void ActionTypeComboBox_SelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeSelectParameter tsp && tsp.Selected is AppActionTypeEnum typeEnum)
            {
                CurrentActionModelValue.ActionType = typeEnum;

                LoadActionData();

                BroadcastEdited();

                return;
            }

            throw new InvalidOperationException($"{nameof(ActionTypeComboBox_SelectionChanged)}: {nameof(sender)} is not {nameof(TypeSelectParameter)} or {nameof(TypeSelectParameter.Selected)} is not {nameof(AppActionTypeEnum)}");
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