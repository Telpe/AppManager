using AppManager.Core.Models;
using AppManager.Settings.Apps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager.Settings.Actions
{
    // ViewModel for Action items in the ListBox
    internal class ActionListItem : INotifyPropertyChanged
    {
        private readonly MainPage _page;
        public int Id { get; }
        public ActionModel Model { get; }

        public bool IsActive
        {
            get => !Model.Inactive ?? true;
            set
            {
                if (IsActive != value)
                {
                    Model.Inactive = !value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DisplayName => $"Action: {Model.ActionType}";
        public int ConditionCount => Model.Conditions?.Length ?? 0;
        public bool HasConditions => ConditionCount > 0;

        public ActionListItem(int id, ActionModel model, MainPage page)
        {
            Id = id;
            Model = model;
            _page = page;
        }
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
