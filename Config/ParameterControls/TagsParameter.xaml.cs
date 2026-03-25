using AppManager.Config.ListItems;
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
    public partial class TagsParameter : BaseParameterControl
    {
        private Dictionary<string, string> TagsValue = new Dictionary<string, string>();
        private readonly ObservableCollection<TagItem> _displayItems = new();
        private bool _isUpdatingFromDisplayItems = false;

        public Dictionary<string, string> Value
        {
            get => TagsValue;
            set
            {
                if (!TagsValue.SequenceEqual(value))
                {
                    TagsValue = value;

                    if (!_isUpdatingFromDisplayItems) { UpdateDisplayFromValue(); }

                    AnnouncePropertyChanged(ValueName);
                }
            }
        }

        public ObservableCollection<TagItem> DisplayItems => _displayItems; 

        public TagsParameter()
        {
            HeaderText = "Tags:";
            LabelText = "Key-Value pairs:";
            ValueName = nameof(Value);

            InitializeComponent();
            this.DataContext = this;

            UpdateDisplayFromValue();
        }

        public TagsParameter(Dictionary<string, string>? tags, PropertyChangedEventHandler? eventHandler = null, string? customValueName = null, string? headerText = null, string? labelText = null) : this()
        {
            if (tags is not null)
            {
                TagsValue = new Dictionary<string, string>(tags);
            }

            if (headerText is not null)
            {
                HeaderText = headerText;
            }

            if (labelText is not null)
            {
                LabelText = labelText;
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
            foreach (var kvp in TagsValue)
            {
                var item = new TagItem
                {
                    TagKey = kvp.Key,
                    TagValue = kvp.Value
                };
                item.PropertyChanged += TagItem_PropertyChanged;
                _displayItems.Add(item);
            }

            EnsureEmptyItemAtEnd();
        }

        private void TagItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TagItem && (e.PropertyName == nameof(TagItem.TagKey) || e.PropertyName == nameof(TagItem.TagValue)))
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

            Value = _displayItems.Where(a=> !a.IsEmpty).ToDictionary(item => item.TagKey, item => item.TagValue);

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

                if (item.TagKey == newKey 
                    || RemovedItemIfEmpty(item))
                { return; }

                if (Value.ContainsKey(newKey))
                {
                    textBox.Text = item.TagKey;
                    // TODO: Show error message about duplicate keys
                    return;
                }

                item.TagKey = newKey;
            }
        }

        private void ValueTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is TagItem item)
            {
                string newValue = textBox.Text.Trim();

                if (RemovedItemIfEmpty(item))
                { return; } 

                item.TagValue = newValue;
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