using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppManager.Config.ParameterControls
{
    public class TagItem : INotifyPropertyChanged
    {
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _originalKey = string.Empty;

        public string Key
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
                }
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        /*public string OriginalKey
        {
            get => _originalKey;
            set
            {
                if (_originalKey != value)
                {
                    _originalKey = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OriginalKey)));
                }
            }
        }*/

        public bool IsEmpty => string.IsNullOrEmpty(Key) && string.IsNullOrEmpty(Value);
        public bool CanDelete => !IsEmpty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class TagsParameter : BaseParameterControl
    {
        private Dictionary<string, string> _value = new Dictionary<string, string>();
        private ObservableCollection<TagItem> _displayItems = new ObservableCollection<TagItem>();
        private bool _isUpdatingFromValue = false;
        private bool _isUpdatingFromDisplayItems = false;

        public Dictionary<string, string> Value
        {
            get => _value;
            set
            {
                if (!_value.SequenceEqual(value))
                {
                    _value = value;

                    if (!_isUpdatingFromDisplayItems) { UpdateDisplayFromValue(); }

                    AnnouncePropertyChanged(ValueName);
                }
            }
        }


        public TagsParameter()
        {
            _headerText = "Tags:";
            _labelText = "Key-Value pairs:";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;

            UpdateDisplayFromValue();
        }

        public TagsParameter(Dictionary<string, string>? tags, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (tags is not null)
            {
                _value = new Dictionary<string, string>(tags);
            }

            if (headerText is not null)
            {
                _headerText = headerText;
            }

            if (labelText is not null)
            {
                _labelText = labelText;
            }

            if (customValueName is not null)
            {
                ValueName = customValueName;
            }

            UpdateDisplayFromValue();

            if (eventHandler is not null)
            {
                PropertyChanged += eventHandler;
            }
        }

        private void UpdateDisplayFromValue()
        {
            _displayItems.Clear();

            // Add existing items from the dictionary
            foreach (var kvp in _value)
            {
                var item = new TagItem
                {
                    Key = kvp.Key,
                    Value = kvp.Value
                };
                item.PropertyChanged += TagItem_PropertyChanged;
                _displayItems.Add(item);
            }

            EnsureEmptyItemAtEnd();
        }

        private void TagItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TagItem && (e.PropertyName == nameof(TagItem.Key) || e.PropertyName == nameof(TagItem.Value)))
            {
                UpdateValueFromDisplay();
                EnsureEmptyItemAtEnd();
            }
        }

        private void EnsureEmptyItemAtEnd()
        {
            if (_displayItems.Count == 0 || !_displayItems.Last().IsEmpty)
            {
                var emptyItem = new TagItem();
                emptyItem.PropertyChanged += TagItem_PropertyChanged;
                _displayItems.Add(emptyItem);
            }
        }

        private void UpdateValueFromDisplay()
        {
            _isUpdatingFromDisplayItems = true;

            Value = _displayItems.Where(a=> !a.IsEmpty).ToDictionary(item => item.Key, item => item.Value);

            _isUpdatingFromDisplayItems = false;
        }

        private bool DictionariesEqual(Dictionary<string, string> dict1, Dictionary<string, string> dict2)
        {
            if (dict1.Count != dict2.Count) return false;

            foreach (var kvp in dict1)
            {
                if (!dict2.ContainsKey(kvp.Key) || dict2[kvp.Key] != kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void CategoryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is TagItem item)
            {
                var newKey = textBox.Text.Trim();

                if (item.Key == newKey 
                    || RemovedItemIfEmpty(item))
                { return; }

                if (Value.ContainsKey(newKey))
                {
                    textBox.Text = item.Key;
                    // TODO: Show error message about duplicate keys
                    return;
                }

                item.Key = newKey;
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is TagItem item)
            {
                string newValue = textBox.Text.Trim();

                if (RemovedItemIfEmpty(item))
                { return; } 

                item.Value = newValue;
            }
        }

        private bool RemovedItemIfEmpty(TagItem item)
        {
            if (item.IsEmpty)
            {
                _displayItems.Remove(item);
                UpdateValueFromDisplay();
                EnsureEmptyItemAtEnd();
                return true;
            }

            return false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TagItem item && item.CanDelete)
            {
                item.PropertyChanged -= TagItem_PropertyChanged;
                _displayItems.Remove(item);
                UpdateValueFromDisplay();
            }
        }
    }
}