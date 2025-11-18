using AppManager.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager.AppUI
{
    // ViewModel for Action items in the ListBox
    internal class ActionViewModel : INotifyPropertyChanged
    {
        private readonly AppPage _page;
        public int Id { get; }
        public ActionModel Model { get; }

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
                    // TODO: Update model when Active property is added to ActionModel
                }
            }
        }

        public string DisplayName => $"Action: {Model.ActionName}";
        public int ConditionCount => Model.Conditions?.Length ?? 0;
        public bool HasConditions => ConditionCount > 0;

        public ActionViewModel(int id, ActionModel model, AppPage page)
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
