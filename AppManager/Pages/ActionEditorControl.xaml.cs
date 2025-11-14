using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppManager.Actions;
using AppManager.Conditions;
using Microsoft.Win32;

namespace AppManager.Pages
{
    public partial class ActionEditorControl : UserControl
    {
        private ActionModel _currentAction;
        private ObservableCollection<ConditionDisplayItem> _conditions;
        private ActionManager _actionManager;

        public event EventHandler<ActionModel> ActionSaved;
        public event EventHandler ActionCancelled;

        public ActionModel CurrentAction
        {
            get => _currentAction;
            set
            {
                _currentAction = value;
                LoadActionData();
            }
        }

        public ActionEditorControl()
        {
            try
            {
                InitializeComponent();
                Initialize();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActionEditorControl initialization error: {ex.Message}");
                // Try to continue without XAML components for debugging
                InitializeManually();
            }
        }

        private void Initialize()
        {
            _actionManager = new ActionManager();
            _conditions = new ObservableCollection<ConditionDisplayItem>();
            
            // Only set ItemsSource if ConditionsListBox exists
            if (ConditionsListBox != null)
            {
                ConditionsListBox.ItemsSource = _conditions;
            }
            
            InitializeComboBoxes();
            UpdatePreview();
        }

        private void InitializeManually()
        {
            // Create a basic grid structure in code if XAML fails
            var grid = new Grid();
            var textBlock = new TextBlock
            {
                Text = "ActionEditorControl - XAML Loading Failed",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(textBlock);
            this.Content = grid;
            
            _actionManager = new ActionManager();
            _conditions = new ObservableCollection<ConditionDisplayItem>();
        }

        private void InitializeComboBoxes()
        {
            try
            {
                // Populate Action Type ComboBox
                if (ActionTypeComboBox != null)
                {
                    ActionTypeComboBox.ItemsSource = Enum.GetValues(typeof(AppActionEnum)).Cast<AppActionEnum>();
                    ActionTypeComboBox.SelectedIndex = 0;
                }

                // Populate Condition Type ComboBox
                if (ConditionTypeComboBox != null)
                {
                    var conditionTypes = Enum.GetValues(typeof(ConditionTypeEnum))
                        .Cast<ConditionTypeEnum>()
                        .Where(c => c != ConditionTypeEnum.None)
                        .ToList();
                    ConditionTypeComboBox.ItemsSource = conditionTypes;
                    if (conditionTypes.Any())
                        ConditionTypeComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeComboBoxes error: {ex.Message}");
            }
        }

        private void LoadActionData()
        {
            if (_currentAction == null) return;

            try
            {
                if (ActionTypeComboBox != null)
                    ActionTypeComboBox.SelectedItem = _currentAction.ActionName;
                
                if (AppNameTextBox != null)
                    AppNameTextBox.Text = _currentAction.AppName ?? string.Empty;
                
                if (WindowTitleTextBox != null)
                    WindowTitleTextBox.Text = _currentAction.WindowTitle ?? string.Empty;
                
                if (ExecutablePathTextBox != null)
                    ExecutablePathTextBox.Text = _currentAction.ExecutablePath ?? string.Empty;
                
                if (ArgumentsTextBox != null)
                    ArgumentsTextBox.Text = _currentAction.Arguments ?? string.Empty;
                
                if (ForceOperationCheckBox != null)
                    ForceOperationCheckBox.IsChecked = _currentAction.ForceOperation;
                
                if (IncludeChildProcessesCheckBox != null)
                    IncludeChildProcessesCheckBox.IsChecked = _currentAction.IncludeChildProcesses;
                
                if (IncludeSimilarNamesCheckBox != null)
                    IncludeSimilarNamesCheckBox.IsChecked = _currentAction.IncludeSimilarNames;
                
                if (TimeoutTextBox != null)
                    TimeoutTextBox.Text = _currentAction.TimeoutMs.ToString();

                // Load conditions
                _conditions.Clear();
                if (_currentAction.Conditions != null)
                {
                    foreach (var condition in _currentAction.Conditions)
                    {
                        _conditions.Add(new ConditionDisplayItem(condition));
                    }
                }

                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadActionData error: {ex.Message}");
            }
        }

        private ActionModel CreateActionModel()
        {
            var action = new ActionModel
            {
                ActionName = (AppActionEnum)(ActionTypeComboBox?.SelectedItem ?? AppActionEnum.Launch),
                AppName = AppNameTextBox?.Text?.Trim() ?? string.Empty,
                WindowTitle = WindowTitleTextBox?.Text?.Trim() ?? string.Empty,
                ExecutablePath = ExecutablePathTextBox?.Text?.Trim() ?? string.Empty,
                Arguments = ArgumentsTextBox?.Text?.Trim() ?? string.Empty,
                ForceOperation = ForceOperationCheckBox?.IsChecked ?? false,
                IncludeChildProcesses = IncludeChildProcessesCheckBox?.IsChecked ?? false,
                IncludeSimilarNames = IncludeSimilarNamesCheckBox?.IsChecked ?? false,
                TimeoutMs = int.TryParse(TimeoutTextBox?.Text, out int timeout) ? timeout : 5000,
                Conditions = _conditions.Select(c => c.Model).ToArray()
            };

            return action;
        }

        private void UpdatePreview()
        {
            try
            {
                if (PreviewTextBlock == null) return;

                var action = CreateActionModel();
                var preview = $"Action Type: {action.ActionName}\n" +
                             $"App Name: {action.AppName ?? "Not specified"}\n" +
                             $"Window Title: {action.WindowTitle ?? "Not specified"}\n";

                if (!string.IsNullOrEmpty(action.ExecutablePath))
                    preview += $"Executable: {action.ExecutablePath}\n";

                if (!string.IsNullOrEmpty(action.Arguments))
                    preview += $"Arguments: {action.Arguments}\n";

                preview += $"Force Operation: {action.ForceOperation}\n" +
                          $"Include Child Processes: {action.IncludeChildProcesses}\n" +
                          $"Include Similar Names: {action.IncludeSimilarNames}\n" +
                          $"Timeout: {action.TimeoutMs}ms\n\n";

                if (action.Conditions?.Length > 0)
                {
                    preview += "Conditions:\n";
                    foreach (var condition in action.Conditions)
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
                    PreviewTextBlock.Text = $"Error generating preview: {ex.Message}";
            }
        }

        private string GetConditionDescription(ConditionModel condition)
        {
            return condition.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process '{condition.ProcessName}' is running",
                ConditionTypeEnum.ProcessNotRunning => $"Process '{condition.ProcessName}' is not running",
                ConditionTypeEnum.FileExists => $"File '{condition.FilePath}' exists",
                ConditionTypeEnum.FileNotExists => $"File '{condition.FilePath}' does not exist",
                _ => "Unknown condition"
            };
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ActionTypeComboBox?.SelectedItem is AppActionEnum actionType && LaunchOptionsGroup != null)
                {
                    LaunchOptionsGroup.Visibility = actionType == AppActionEnum.Launch ? Visibility.Visible : Visibility.Collapsed;
                }
                UpdatePreview();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ActionTypeComboBox_SelectionChanged error: {ex.Message}");
            }
        }

        private void BrowseExecutableButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                    Title = "Select Executable"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    if (ExecutablePathTextBox != null)
                        ExecutablePathTextBox.Text = openFileDialog.FileName;
                    
                    // Auto-populate app name if empty
                    if (AppNameTextBox != null && string.IsNullOrEmpty(AppNameTextBox.Text))
                    {
                        var fileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                        AppNameTextBox.Text = fileName;
                    }
                    
                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error browsing for executable: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddConditionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ConditionTypeComboBox?.SelectedItem is ConditionTypeEnum conditionType)
                {
                    var conditionModel = new ConditionModel { ConditionType = conditionType };
                    
                    // Show condition configuration dialog
                    var conditionDialog = new ConditionConfigDialog(conditionModel);
                    if (conditionDialog.ShowDialog() == true)
                    {
                        _conditions.Add(new ConditionDisplayItem(conditionDialog.ConditionModel));
                        UpdatePreview();
                    }
                }
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
                if (sender is Button button && button.Tag is ConditionDisplayItem condition)
                {
                    _conditions.Remove(condition);
                    UpdatePreview();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RemoveConditionButton_Click error: {ex.Message}");
            }
        }

        private void TestActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var action = CreateActionModel();
                var canExecute = _actionManager.CanExecuteAction(action);
                
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
                
                if (string.IsNullOrWhiteSpace(AppNameTextBox?.Text))
                    errors.Add("App Name is required");
                
                if (ActionTypeComboBox?.SelectedItem is AppActionEnum actionType && actionType == AppActionEnum.Launch)
                {
                    if (string.IsNullOrWhiteSpace(ExecutablePathTextBox?.Text))
                        errors.Add("Executable Path is required for Launch action");
                }

                if (!int.TryParse(TimeoutTextBox?.Text, out int timeout) || timeout <= 0)
                    errors.Add("Timeout must be a positive number");

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
            try
            {
                var action = CreateActionModel();
                _currentAction = action;
                ActionSaved?.Invoke(this, action);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Save failed: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ActionCancelled?.Invoke(this, EventArgs.Empty);
        }

        // Event handlers for text changes to update preview
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdatePreview();
        private void CheckBox_Changed(object sender, RoutedEventArgs e) => UpdatePreview();
    }

    public class ConditionDisplayItem
    {
        public ConditionModel Model { get; }
        public string DisplayText { get; }

        public ConditionDisplayItem(ConditionModel model)
        {
            Model = model;
            DisplayText = GetDisplayText(model);
        }

        private string GetDisplayText(ConditionModel model)
        {
            return model.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning => $"Process Running: {model.ProcessName}",
                ConditionTypeEnum.ProcessNotRunning => $"Process Not Running: {model.ProcessName}",
                ConditionTypeEnum.FileExists => $"File Exists: {model.FilePath}",
                ConditionTypeEnum.FileNotExists => $"File Not Exists: {model.FilePath}",
                _ => $"Unknown Condition: {model.ConditionType}"
            };
        }
    }

    // Simple condition configuration dialog
    public class ConditionConfigDialog : Window
    {
        public ConditionModel ConditionModel { get; }
        private TextBox _valueTextBox;

        public ConditionConfigDialog(ConditionModel model)
        {
            ConditionModel = model;
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            Title = $"Configure {ConditionModel.ConditionType} Condition";
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new Label { Content = GetLabelText(), Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            _valueTextBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(_valueTextBox, 1);
            grid.Children.Add(_valueTextBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 0, 5, 0)
            };
            okButton.Click += (s, e) => { SaveAndClose(); };

            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 75,
                Height = 25
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        private string GetLabelText()
        {
            return ConditionModel.ConditionType switch
            {
                ConditionTypeEnum.ProcessRunning or ConditionTypeEnum.ProcessNotRunning => "Process Name:",
                ConditionTypeEnum.FileExists or ConditionTypeEnum.FileNotExists => "File Path:",
                _ => "Value:"
            };
        }

        private void SaveAndClose()
        {
            var value = _valueTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(value))
            {
                MessageBox.Show("Please enter a value.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (ConditionModel.ConditionType)
            {
                case ConditionTypeEnum.ProcessRunning:
                case ConditionTypeEnum.ProcessNotRunning:
                    ConditionModel.ProcessName = value;
                    break;
                case ConditionTypeEnum.FileExists:
                case ConditionTypeEnum.FileNotExists:
                    ConditionModel.FilePath = value;
                    break;
            }

            DialogResult = true;
            Close();
        }
    }
}