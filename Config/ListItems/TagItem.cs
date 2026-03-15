using System;
using System.ComponentModel;

namespace AppManager.Config.ListItems
{
    public class TagItem : INotifyPropertyChanged
    {
        private string _key = string.Empty;
        private string _value = string.Empty;

        public string TagKey
        {
            get => _key;
            set
            {
                if (_key != value)
                {
                    _key = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagKey)));
                }
            }
        }

        public string TagValue
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagValue)));
                }
            }
        }

        public bool IsEmpty => string.IsNullOrEmpty(TagKey) && string.IsNullOrEmpty(TagValue);
        public bool CanDelete => !IsEmpty;

        public event PropertyChangedEventHandler? PropertyChanged;
    }


}
