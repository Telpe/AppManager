using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class TypeSelectParameter : UserControl, INotifyPropertyChanged
    {
        private Type? _enumType;
        private object? _selectedValue;
        private string _labelText = "Change:";
        private string _headerText = "Selection";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string LabelText
        {
            get => _labelText;
        }

        public string HeaderText
        {
            get => _headerText;
        }

        public object? Selected
        {
            get => _selectedValue;
            set
            {
                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    TypeComboBox.SelectedItem = _selectedValue; //UpdateComboBoxSelection();
                    BroadcastPropertyChanged(nameof(Selected));
                }
            }
        }

        public TypeSelectParameter()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public TypeSelectParameter(Type enumType, Enum? selectedValue = null, string? headerText = null, string? labelText = null) : this()
        {
            if (!enumType.IsEnum) { throw new ArgumentException("Type must be an enum", nameof(enumType)); }

            _enumType = enumType;
            
            if (headerText != null)
            {
                _headerText = headerText;
            }
            
            if (labelText != null)
            {
                _labelText = labelText;
            }

            if (selectedValue != null)
            {
                _selectedValue = selectedValue;
            }

            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            if (_enumType is null) { return; }

            TypeComboBox.Items.Clear();

            TypeComboBox.ItemsSource = Enum.GetValues(_enumType);

            TypeComboBox.SelectedItem = Selected;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selected = TypeComboBox.SelectedItem;
        }

        protected virtual void BroadcastPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}