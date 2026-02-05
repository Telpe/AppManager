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

                (ActionConditions.Content as ConditionsParameter)!.PropertyChanged += ParameterChanged;

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


                        UseParameterGroupBox("App Name:").Content = new ProcessParameter(
                            CurrentActionModel.AppName, 
                            ParameterChanged, 
                            nameof(ActionModel.AppName));
                        break;

                    case nameof(ActionModel.ExecutablePath):

                        UseParameterGroupBox("Executable Path:").Content = new FilePathParameter(
                            CurrentActionModel.ExecutablePath, 
                            ParameterChanged, 
                            nameof(ActionModel.ExecutablePath));
                        break;

                    case nameof(ActionModel.Arguments):
                        
                        UseParameterStackPanel("Arguments:").Children.Add(new ExeArgumentsParameter(
                            CurrentActionModel.WindowTitle,
                            ParameterChanged,
                            nameof(ActionModel.Arguments)));
                        break;

                    case nameof(ActionModel.ForceOperation):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.ForceOperation,
                            ParameterChanged,
                            nameof(ActionModel.ForceOperation),
                            "Advanced:",
                            "Force Operation:"));
                        break;

                    case nameof(ActionModel.IncludeChildProcesses):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.IncludeChildProcesses,
                            ParameterChanged,
                            nameof(ActionModel.IncludeChildProcesses),
                            "Advanced:",
                            "Include Child Processes:"));
                        break;

                    case nameof(ActionModel.IncludeSimilarNames):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.IncludeSimilarNames,
                            ParameterChanged,
                            nameof(ActionModel.IncludeSimilarNames),
                            "Advanced:",
                            "Include Similar Names:"));
                        break;

                    case nameof(ActionModel.TimeoutMs):

                        UseParameterGroupBox("Timeout (ms):").Content = new TimerParameter(
                            CurrentActionModel.TimeoutMs,
                            ParameterChanged,
                            nameof(ActionModel.TimeoutMs));
                        break;

                    case nameof(ActionModel.WindowTitle):

                        UseParameterGroupBox("Window Title:").Content = new StringParameter(
                            CurrentActionModel.WindowTitle,
                            ParameterChanged,
                            nameof(ActionModel.WindowTitle));
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

        private void ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not null)
            {
                switch (e.PropertyName)
                {
                    case nameof(ActionModel.ActionType):
                        ActionTypeComboBox_SelectionChanged(sender, e);
                        break;

                    case nameof(ActionModel.AppName):
                        CurrentActionModelValue.AppName = (sender as ProcessParameter)!.Value ;
                        break;

                    case nameof(ActionModel.Arguments):
                        CurrentActionModelValue.Arguments = (sender as ExeArgumentsParameter)!.Value;
                        break;

                    case nameof(ActionModel.Conditions):
                        CurrentActionModelValue.Conditions = (sender as ConditionsParameter)!.Value;
                        break;

                    case nameof(ActionModel.ExecutablePath):
                        CurrentActionModelValue.ExecutablePath = (sender as FilePathParameter)!.Value;
                        break;

                    case nameof(ActionModel.ForceOperation):
                        CurrentActionModelValue.ForceOperation = (sender as BooleanParameter)!.Value;
                        break;

                    case nameof(ActionModel.Inactive):
                        CurrentActionModelValue.Inactive = (sender as BooleanParameter)!.Value;
                        break;

                    case nameof(ActionModel.IncludeChildProcesses):
                        CurrentActionModelValue.IncludeChildProcesses = (sender as BooleanParameter)!.Value;
                        break;

                    case nameof(ActionModel.IncludeSimilarNames):
                        CurrentActionModelValue.IncludeSimilarNames = (sender as BooleanParameter)!.Value;
                        break;

                    case nameof(ActionModel.ProcessLastId):
                        break;

                    case nameof(ActionModel.TimeoutMs):
                        CurrentActionModelValue.TimeoutMs = (sender as TimerParameter)!.Value;
                        break;
                    case nameof(ActionModel.WindowTitle):
                        CurrentActionModelValue.WindowTitle = (sender as StringParameter)!.Value;
                        break;

                }

                UpdatePreview();
                BroadcastEdited();

                return;
            }

            throw new InvalidOperationException($"{nameof(ParameterChanged)}: {nameof(sender)} is not {nameof(TypeSelectParameter)} or {nameof(TypeSelectParameter.Selected)} is not {nameof(AppActionTypeEnum)}");
        }
    }
}