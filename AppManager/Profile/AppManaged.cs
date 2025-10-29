using AppManager.Actions;
using AppManager.Triggers;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AppManager.Profile
{
    public class AppManaged : DockPanel, IAppManaged
    {
        public TextBox AppNameBox;
        public CheckBox ActiveBox;

        // Add missing interface properties
        public Dictionary<int, TriggerModel> AppTriggers { get; set; } = new Dictionary<int, TriggerModel>();
        public Dictionary<int, ActionModel> AppActions { get; set; } = new Dictionary<int, ActionModel>();

        public string AppName
        {
            get { return AppNameBox.Text; }
            set { AppNameBox.Text = value; }
        }
        
        public bool Active
        {
            get { return (bool)ActiveBox.IsChecked; }
            set { ActiveBox.IsChecked = value; }
        }
        

        public AppManaged()
        {
            Initiate();
        }

        public AppManaged(AppManagedModel model)
        {
            Initiate();

            AppName = model.AppName;
            //Active = model.Active;
            
            // Copy the interface properties
            AppTriggers = model.AppTriggers ?? new Dictionary<int, TriggerModel>();
            AppActions = model.AppActions ?? new Dictionary<int, ActionModel>();
        }

        // Rest of the existing code remains the same...
        private void Initiate()
        {
            Height = 22;
            double checkboxNewWidth = 40;
            double checkboxWidth = 16;

            ActiveBox = new CheckBox();
            ActiveBox.Width = checkboxWidth;
            ActiveBox.Margin = new Thickness(0, 1, 0, 1);
            ActiveBox.VerticalAlignment = VerticalAlignment.Center;

            AppNameBox = new TextBox();
            AppNameBox.Width = 160;
            AppNameBox.Padding = new Thickness(0, 0, 0, 0);
            AppNameBox.Margin = new Thickness(0, 1, 0, 1);
            AppNameBox.VerticalAlignment = VerticalAlignment.Center;

            Children.Add(ActiveBox);
            Children.Add(AppNameBox);
        }
    }
}
