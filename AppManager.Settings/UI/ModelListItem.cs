using AppManager.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppManager.Settings.UI
{
    internal class ModelListItem<T> : INotifyPropertyChanged where T : ConditionalModel
    {
        //private readonly Apps.MainPage _page;
        public int Id { get; }
        public T Model { get; }

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

        public string DisplayName 
        { 
            get 
            {
                if (Model is ActionModel model)
                {
                    return $"Action: {model.ActionType}"; 
                }

                if (Model is TriggerModel triggerModel)
                {
                    return $"Trigger: {triggerModel.TriggerType}";
                }

                return "Unknown Type";
            }
        }
        
        public int ConditionCount => Model.Conditions?.Length ?? 0;
        public bool HasConditions => ConditionCount > 0;

        public ModelListItem(int id, T model, Apps.AppsPage? page = null)
        {
            Id = id;
            Model = model;
            //_page = page;
        }
        
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
