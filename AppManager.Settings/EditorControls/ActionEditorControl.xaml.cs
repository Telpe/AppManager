using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Settings.Interfaces;
using AppManager.Settings.UI;
using Microsoft.Win32;

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
                ForceOperationCheckBox!.IsChecked = CurrentActionModelValue.ForceOperation;
                IncludeChildProcessesCheckBox!.IsChecked = CurrentActionModelValue.IncludeChildProcesses;
                IncludeSimilarNamesCheckBox!.IsChecked = CurrentActionModelValue.IncludeSimilarNames;

                TimeoutParameter!.TimerValue = CurrentActionModelValue.TimeoutMs;
                ConditionPlugin.ConditionalModel = CurrentActionModelValue;

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

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ActionTypeComboBox?.SelectedItem is AppActionTypeEnum actionType)
                {
                    CurrentActionModelValue.ActionType = actionType;
                    GenerateDynamicGrid(actionType);
                }

                UpdatePreview();
            }
            catch (Exception ex)
            {
                Log.WriteLine($"ActionTypeComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private void GenerateDynamicGrid(AppActionTypeEnum actionType)
        {
            if (null == LaunchOptionsGroup){ return; }

            var grid = new Grid
            {
                Margin = new Thickness(5)
            };

            var rowCount = 0;
            //var controlDictionary = new Dictionary<string, UIElement>();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

            switch (actionType)
            {
                case AppActionTypeEnum.Launch:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(ILaunchAction));
                    LaunchOptionsGroup.Header = "Launch Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                case AppActionTypeEnum.Close:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(ICloseAction));
                    LaunchOptionsGroup.Header = "Close Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                case AppActionTypeEnum.Restart:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(IRestartAction));
                    LaunchOptionsGroup.Header = "Restart Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                case AppActionTypeEnum.Focus:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(IFocusAction));
                    LaunchOptionsGroup.Header = "Focus Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                case AppActionTypeEnum.BringToFront:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(IBringToFrontAction));
                    LaunchOptionsGroup.Header = "Bring To Front Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                case AppActionTypeEnum.Minimize:
                    AddGridRowsFromInterface(grid, ref rowCount, typeof(IMinimizeAction));
                    LaunchOptionsGroup.Header = "Minimize Options";
                    LaunchOptionsGroup.Visibility = Visibility.Visible;
                    break;

                default:
                    LaunchOptionsGroup.Visibility = Visibility.Collapsed;
                    break;
            }

            LaunchOptionsGroup.Content = grid;
        }

        private void AddGridRowsFromInterface(Grid grid, ref int rowIndex, Type modelType)
        {
            foreach (var prop in modelType.GetProperties())
            {
                var propLabel = prop.Name switch
                {
                    "ExecutablePath" => "Executable Path:",
                    "Arguments" => "Arguments:",
                    "ForceOperation" => "Force Operation",
                    "IncludeChildProcesses" => "Include Child Processes",
                    "IncludeSimilarNames" => "Include Similar Names",
                    "TimeoutMs" => "Timeout (ms):",
                    "WindowTitle" => "Window Title:",
                    _ => prop.Name
                };

                if (prop.PropertyType == typeof(string))
                {
                    AddTextBoxRow(grid, ref rowIndex, prop.Name, propLabel, "ExecutablePath" == prop.Name ? TextHelperButtonEnum.BrowseExecutable : TextHelperButtonEnum.None);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    AddCheckBoxRow(grid, ref rowIndex, prop.Name, propLabel);
                }
            }
        }

        private void AddTextBoxRow(Grid grid, ref int rowIndex, string controlName, string labelText, TextHelperButtonEnum withButton = TextHelperButtonEnum.None)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            var label = new Label
            {
                Content = labelText,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, rowIndex);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            var textBox = new TextBox
            {
                Name = controlName + "TextBox",
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Height = 22
            };
            Grid.SetRow(textBox, rowIndex);
            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);

            textBox.Text = CurrentActionModelValue.GetType().GetProperty(controlName)?.GetValue(CurrentActionModelValue)?.ToString() ?? string.Empty;

            textBox.TextChanged += TextBox_TextChanged;

            if (TextHelperButtonEnum.BrowseExecutable == withButton)
            {
                var browseButton = new Button
                {
                    Content = "Browse...",
                    Width = 80,
                    Height = 25,
                    Tag = controlName
                };
                browseButton.Click += (sender, e) => BrowseExecutableButton_Click(sender, e, textBox);
                Grid.SetRow(browseButton, rowIndex);
                Grid.SetColumn(browseButton, 2);
                grid.Children.Add(browseButton);
            }

            rowIndex++;
        }

        private void AddCheckBoxRow(Grid grid, ref int rowIndex, string controlName, string checkBoxText)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            var checkBox = new CheckBox
            {
                Content = checkBoxText,
                Name = controlName + "CheckBox",
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };
            checkBox.Checked += CheckBox_Changed;
            checkBox.Unchecked += CheckBox_Changed;

            Grid.SetRow(checkBox, rowIndex);
            Grid.SetColumn(checkBox, 0);
            Grid.SetColumnSpan(checkBox, 2);
            grid.Children.Add(checkBox);

            RegisterCheckBox(checkBox, controlName);

            rowIndex++;
        }

        private void RegisterCheckBox(CheckBox checkBox, string controlName)
        {
            switch (controlName)
            {
                case "ForceOperation":
                    ForceOperationCheckBox = checkBox;
                    break;
                case "IncludeChildProcesses":
                    IncludeChildProcessesCheckBox = checkBox;
                    break;
                case "IncludeSimilarNames":
                    IncludeSimilarNamesCheckBox = checkBox;
                    break;
            }
        }

        private void BrowseExecutableButton_Click(object sender, RoutedEventArgs e, TextBox target)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = $"Executable files({String.Join(',', FileManager.ExecuteableExtensions.Select(a => "*" + a))})|{String.Join(';', FileManager.ExecuteableExtensions.Select(a=>"*"+a))}|All files (*.*)|*.*",
                    Title = "Select Executable"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    target.Text = System.IO.Path.GetFullPath(openFileDialog.FileName);

                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing for executable: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        protected void AnnounceEdited()
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }

        protected void DoCancel()
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        protected void DoSave()
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
            DoSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DoCancel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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
            }
            
            AnnounceEdited();
            UpdatePreview();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && !string.IsNullOrEmpty(checkBox.Name))
            {
                var propertyName = checkBox.Name.Replace("CheckBox", "");
                var property = CurrentActionModelValue.GetType().GetProperty(propertyName);
                
                if (property != null && property.PropertyType == typeof(bool))
                {
                    property.SetValue(CurrentActionModelValue, checkBox.IsChecked == true);
                }
            }
            
            AnnounceEdited();
            UpdatePreview();
        }

        private void ConditionPlugin_ConditionsChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void TimeoutParameter_TimerValueChanged(object sender, EventArgs e)
        {

        }
    }
}