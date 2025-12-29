using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    public class ButtonTrigger : BaseTrigger, IButtonTrigger
    {
        private bool _IsOn { get; set; } = false;
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Button;

        private System.Windows.Controls.Button? MonitoredButtonValue;

        public Dictionary<string, object>? CustomProperties { get; set; }
        private Dictionary<string, object> CustomPropertiesDefined
        {
            get { return CustomProperties ?? []; }
            set { CustomProperties = value; }
        }

        public ButtonTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors UI button clicks";

            CustomProperties = model.CustomProperties;
        }

        protected override bool CanStartTrigger()
        {
            return CustomPropertiesDefined.ContainsKey("Button");
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                try
                {
                    if (CustomPropertiesDefined.TryGetValue("Button", out object? buttonObj) && buttonObj is System.Windows.Controls.Button button)
                    {
                        MonitoredButtonValue = button;
                        MonitoredButtonValue.Click += OnButtonClicked;

                        System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' started monitoring button '{button.Name}'");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error starting button trigger '{Name}': {ex.Message}");
                }

                return false;
            });
        }

        public override void Stop()
        {
            if (MonitoredButtonValue != null)
            {
                MonitoredButtonValue.Click -= OnButtonClicked;
            }
        }

        private void OnButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' activated");
            
            // Trigger the configured action
            TriggerActivated();
        }

        public override TriggerModel ToModel()
        {
            TriggerModel model = base.ToModel();
            model.CustomProperties = CustomProperties;
            return model;
        }
    }
}