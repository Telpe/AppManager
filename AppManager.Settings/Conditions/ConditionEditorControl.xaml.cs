using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Settings.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AppManager.Settings.Conditions
{
    /// <summary>
    /// Interaction logic for ConditionEditorControl.xaml
    /// </summary>
    public partial class ConditionEditorControl : UserControl, IInputEditControl
    {
        public event EventHandler? Edited;

        public event EventHandler? Cancel;

        public event EventHandler<InputEditEventArgs>? Save;

        private ConditionModel ConditionModelValue;

        public ConditionEditorControl(ConditionModel conditionModel)
        {
            ConditionModelValue = conditionModel;

            InitializeComponent();

            if (ConditionTypeComboBox != null)
            {
                List<ConditionTypeEnum> conditionTypes = Enum.GetValues(typeof(ConditionTypeEnum))
                    .Cast<ConditionTypeEnum>()
                    .ToList();
                ConditionTypeComboBox.ItemsSource = conditionTypes;
                if (conditionTypes.Any())
                {
                    ConditionTypeComboBox.SelectedItem = ConditionModelValue.ConditionType;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ConditionTypeComboBox.SelectedItem is ConditionTypeEnum.None)
            {
                MessageBox.Show("Please select another condition type.", "Invalid Condition Type", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ConditionModelValue.ConditionType = (ConditionTypeEnum)ConditionTypeComboBox.SelectedItem;

            DoSave();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DoCancel();
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
            Save?.Invoke(this, new InputEditEventArgs(ConditionModelValue.Clone()));
        }
    }
}
