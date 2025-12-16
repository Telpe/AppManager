using System.Windows;
using System.Windows.Controls;
using AppManager.Core.Conditions;
using AppManager.Core.Models;

namespace AppManager.Settings.Conditions
{
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
                ConditionTypeEnum.ProcessRunning or ConditionTypeEnum.ProcessRunning => "Process Name:",
                ConditionTypeEnum.FileExists or ConditionTypeEnum.FileExists => "File Path:",
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
                    ConditionModel.ProcessName = value;
                    break;
                case ConditionTypeEnum.FileExists:
                    ConditionModel.FilePath = value;
                    break;
            }

            DialogResult = true;
            Close();
        }
    }
}