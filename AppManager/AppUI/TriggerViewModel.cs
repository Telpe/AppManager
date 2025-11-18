using AppManager.Triggers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager.AppUI
{
    // ViewModel for Trigger items in the ListBox
    internal class TriggerViewModel : INotifyPropertyChanged
    {
        private readonly AppPage _page;
        public int Id { get; }
        public TriggerModel Model { get; }

        private bool _isActive = true;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                    // TODO: Update model when Active property is added to TriggerModel
                }
            }
        }

        public string DisplayName => $"Trigger: {Model.TriggerType}";
        public int ConditionCount => 0; // TODO: Implement when conditions are added to TriggerModel
        public bool HasConditions => ConditionCount > 0;

        public TriggerViewModel(int id, TriggerModel model, AppPage page)
        {
            Id = id;
            Model = model;
            _page = page;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
