using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal class ButtonTrigger : BaseTrigger, IButtonTrigger
    {
        private bool _IsOn { get; set; } = false;
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Button;

        private System.Windows.Controls.Button MonitoredButtonValue;

        public Dictionary<string, object>? CustomProperties { get; set; }

        public ButtonTrigger(TriggerModel model) : base(model)
        {
            Description = "Monitors UI button clicks";

            CustomProperties = model.CustomProperties ?? [];
        }

        public override bool CanStart()
        {
            return CustomProperties.ContainsKey("Button") == true;
        }

        public override Task<bool> StartAsync()
        {
            return Task.Run<bool>(() =>
            {
                if (!IsActive) { return false; }

                try
                {
                    if (CustomProperties.TryGetValue("Button", out var buttonObj) && buttonObj is System.Windows.Controls.Button button)
                    {
                        MonitoredButtonValue = button;
                        MonitoredButtonValue.Click += OnButtonClicked;

                        System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' started monitoring button '{button.Name}'");
                        return true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error starting button trigger '{Name}': {ex.Message}");
                    return false;
                }
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
            OnTriggerActivated("target_app", AppActionTypeEnum.Launch, null, new { ButtonName = MonitoredButtonValue?.Name });
        }

        public override TriggerModel ToModel()
        {
            return new TriggerModel
            {
                TriggerType = TriggerType,
                IsActive = IsActive,
                CustomProperties = CustomProperties
            };
        }
    }
}