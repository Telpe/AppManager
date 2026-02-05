using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Settings.ParameterControls
{
    public partial class TypeSelectParameter : BaseParameterControl
    {
        private Type? _enumType;
        private object? _selectedValue;

        public object? Selected
        {
            get => _selectedValue;
            set
            {
                if (value != TypeComboBox.SelectedItem)
                {
                    TypeComboBox.SelectedItem = value;
                }

                if (_selectedValue != value)
                {
                    _selectedValue = value;
                    BroadcastPropertyChanged(ValueName);
                }
            }
        }

        public TypeSelectParameter()
        {
            _labelText = "Change:";
            _headerText = "Select Type";
            ValueName = nameof(Selected);

            InitializeComponent();
            this.DataContext = this;
            
        }

        public TypeSelectParameter(Type enumType, Enum? selectedValue = null, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
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

            if (customValueName != null)
            {
                ValueName = customValueName;
            }

            if (selectedValue != null)
            {
                _selectedValue = selectedValue;
            }

            PopulateComboBox();

            if (eventHandler != null)
            {
                PropertyChanged += eventHandler;
            }
        }

        private void PopulateComboBox()
        {
            if (_enumType is null) { return; }

            TypeComboBox.Items.Clear();

            TypeComboBox.ItemsSource = Enum.GetValues(_enumType);

            TypeComboBox.SelectedItem = _selectedValue;
        }

        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selected = TypeComboBox.SelectedItem;
        }
    }
}