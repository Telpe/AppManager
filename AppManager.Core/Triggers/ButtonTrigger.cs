using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers
{
    internal class ButtonTrigger : BaseTrigger
    {
        private bool _IsOn { get; set; } = false;
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Button;

        private System.Windows.Controls.Button MonitoredButtonStored;

        public Dictionary<string, object> CustomProperties { get; set; }

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
                        MonitoredButtonStored = button;
                        MonitoredButtonStored.Click += OnButtonClicked;

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
            if (MonitoredButtonStored != null)
            {
                MonitoredButtonStored.Click -= OnButtonClicked;
            }
        }

        private void OnButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' activated");
            
            // Trigger the configured action
            OnTriggerActivated("target_app", AppActionTypeEnum.Launch, null, new { ButtonName = MonitoredButtonStored?.Name });
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