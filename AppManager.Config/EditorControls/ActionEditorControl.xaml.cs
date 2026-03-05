using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Config.Interfaces;
using AppManager.Config.ParameterControls;
using AppManager.Config.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Actions.BringToFront;
using AppManager.Core.Actions.Launch;
using AppManager.Core.Actions.Minimize;
using AppManager.Core.Actions.Close;
using AppManager.Core.Actions.Focus;
using AppManager.Core.Actions.Restart;
using System.Text;

namespace AppManager.Config.EditorControls
{
    public partial class ActionEditorControl : UserControl, IInputEditControl
    {
        private ActionModel CurrentActionModelValue;

        public event TrueEventHandler? OnEdited;

        public event TrueEventHandler? OnCancel;

        public event TrueEventHandler<InputEditEventArgs>? OnSave;

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
                TypeSelectionGroupBox!.Content = new TypeSelectParameter(typeof(ActionTypeEnum), null, null, "Choose:");
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

                ActionConditions.Content = new ConditionsParameter(CurrentActionModelValue.Conditions ?? [], ParameterChanged, nameof(ConditionalModel.Conditions));

                if (TypeSelectionGroupBox!.Content is TypeSelectParameter { Selected: ActionTypeEnum actionType })
                {
                    switch (actionType)
                    {
                        case ActionTypeEnum.Launch:
                            AddParametersFromInterface(typeof(ILaunchAction));
                            break;
                        case ActionTypeEnum.Close:
                            AddParametersFromInterface(typeof(ICloseAction));
                            break;
                        case ActionTypeEnum.Restart:
                            AddParametersFromInterface(typeof(IRestartAction));
                            break;
                        case ActionTypeEnum.Minimize:
                            AddParametersFromInterface(typeof(IMinimizeAction));
                            break;
                        case ActionTypeEnum.Focus:
                            AddParametersFromInterface(typeof(IFocusAction));
                            break;
                        case ActionTypeEnum.BringToFront:
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
                if (PreviewTextBlock is null)
                { return; }

                var preview = new StringBuilder();
                preview.AppendLine($"Action Type: {CurrentActionModelValue.ActionType}");
                preview.AppendLine($"App Name: {CurrentActionModelValue.AppName ?? "Not specified"}");
                preview.AppendLine($"Window Title: {CurrentActionModelValue.WindowTitle ?? "Not specified"}");

                if (!string.IsNullOrEmpty(CurrentActionModelValue.ExecutablePath))
                {
                    preview.AppendLine($"Executable: {CurrentActionModelValue.ExecutablePath}");
                }

                if (!string.IsNullOrEmpty(CurrentActionModelValue.Arguments))
                {
                    preview.AppendLine($"Arguments: {CurrentActionModelValue.Arguments}");
                }

                preview.AppendLine($"Force Operation: {CurrentActionModelValue.ForceOperation}");
                preview.AppendLine($"Include Child Processes: {CurrentActionModelValue.IncludeChildProcesses}");
                preview.AppendLine($"Include Similar Names: {CurrentActionModelValue.IncludeSimilarNames}");
                preview.AppendLine($"Timeout: {CurrentActionModelValue.TimeoutMs}ms");
                preview.AppendLine();

                preview.Append(CurrentActionModelValue.Conditions?.Length.ToString() ?? "0");
                preview.AppendLine(" Conditions:");
                foreach (ConditionModel condition in CurrentActionModelValue.Conditions ?? [])
                {
                    preview.AppendLine($"  - {condition.ConditionType}: {condition.ConditionType.ToDescription()}");
                }

                PreviewTextBlock.Text = preview.ToString();
            }
            catch (Exception ex)
            {
                PreviewTextBlock?.Text = $"Error generating preview: {ex.Message}";
            }
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
                            prop.Name)
                            { Description=DescriptionHelper.GetDescription(prop) };
                        break;

                    case nameof(ActionModel.ExecutablePath):

                        UseParameterGroupBox("Executable Path:").Content = new FilePathParameter(
                            CurrentActionModel.ExecutablePath, 
                            ParameterChanged,
                            prop.Name)
                            { Description = DescriptionHelper.GetDescription(prop) };
                        break;

                    case nameof(ActionModel.Arguments):
                        
                        UseParameterStackPanel("Arguments:").Children.Add(new ExeArgumentsParameter(
                            CurrentActionModel.WindowTitle,
                            ParameterChanged,
                            prop.Name)
                            { Description = DescriptionHelper.GetDescription(prop)} );
                        break;

                    case nameof(ActionModel.ForceOperation):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.ForceOperation,
                            ParameterChanged,
                            prop.Name,
                            "Advanced:",
                            "Force Operation:")
                            { Description = DescriptionHelper.GetDescription(prop)} );
                        break;

                    case nameof(ActionModel.IncludeChildProcesses):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.IncludeChildProcesses,
                            ParameterChanged,
                            prop.Name,
                            "Advanced:",
                            "Include Child Processes:")
                            { Description = DescriptionHelper.GetDescription(prop) });
                        break;

                    case nameof(ActionModel.IncludeSimilarNames):

                        UseParameterStackPanel("Advanced:").Children.Add(new BooleanParameter(
                            CurrentActionModel.IncludeSimilarNames,
                            ParameterChanged,
                            prop.Name,
                            "Advanced:",
                            "Include Similar Names:")
                            { Description = DescriptionHelper.GetDescription(prop) });
                        break;

                    case nameof(ActionModel.TimeoutMs):

                        UseParameterGroupBox("Timeout (ms):").Content = new TimerParameter(
                            CurrentActionModel.TimeoutMs,
                            ParameterChanged,
                            prop.Name)
                            { Description = DescriptionHelper.GetDescription(prop) };
                        break;

                    case nameof(ActionModel.WindowTitle):

                        UseParameterGroupBox("Window Title:").Content = new StringParameter(
                            CurrentActionModel.WindowTitle,
                            ParameterChanged,
                            prop.Name)
                            { Description = DescriptionHelper.GetDescription(prop) };
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
       

        protected void AnnounceEdited()
        {
            OnEdited?.Invoke(this, EventArgs.Empty);
        }

        protected void AnnounceCancel()
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
        }

        protected void AnnounceSave()
        {
            OnSave?.Invoke(this, new InputEditEventArgs(CurrentActionModelValue.Clone()));
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
            AnnounceSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AnnounceCancel();
        }


        private void ActionTypeComboBox_SelectionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TypeSelectParameter tsp && tsp.Selected is ActionTypeEnum typeEnum)
            {
                CurrentActionModelValue.ActionType = typeEnum;

                LoadActionData();

                UpdatePreview();
                AnnounceEdited();

                return;
            }

            throw new InvalidOperationException($"{nameof(ActionTypeComboBox_SelectionChanged)}: {nameof(sender)} is not {nameof(TypeSelectParameter)} or {nameof(TypeSelectParameter.Selected)} is not {nameof(ActionTypeEnum)}");
        }

        private void ParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is not BaseParameterControl) { throw new InvalidOperationException($"{nameof(ParameterChanged)}: {nameof(sender)} is not {nameof(BaseParameterControl)}"); }
            
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
            AnnounceEdited();
            
        }
    }
}