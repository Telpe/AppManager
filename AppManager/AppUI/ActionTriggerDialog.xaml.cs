using System;
using System.Windows;
using AppManager.Actions;
using AppManager.Triggers;

namespace AppManager.AppUI
{
    public partial class ActionTriggerDialog : Window
    {
        public ActionModel ResultAction { get; private set; }
        public TriggerModel ResultTrigger { get; private set; }
        public bool IsActionMode { get; }

        public ActionTriggerDialog(ActionModel action = null, TriggerModel trigger = null)
        {
            InitializeComponent();
            
            IsActionMode = action != null;
            
            if (IsActionMode)
            {
                ActionEditor.CurrentAction = action ?? new ActionModel();
                ActionEditor.ActionSaved += OnActionSaved;
                ActionEditor.ActionCancelled += OnCancelled;
                Title = "Edit Action";
            }
            else
            {
                TriggerEditor.CurrentTrigger = trigger ?? new TriggerModel();
                TriggerEditor.TriggerSaved += OnTriggerSaved;
                TriggerEditor.TriggerCancelled += OnCancelled;
                Title = "Edit Trigger";
            }
        }

        private void OnActionSaved(object sender, ActionModel action)
        {
            ResultAction = action;
            DialogResult = true;
            Close();
        }

        private void OnTriggerSaved(object sender, TriggerModel trigger)
        {
            ResultTrigger = trigger;
            DialogResult = true;
            Close();
        }

        private void OnCancelled(object sender, EventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}