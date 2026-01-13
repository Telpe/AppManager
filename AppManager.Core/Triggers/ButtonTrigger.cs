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

        public override void Start()
        {
            if (!CanStart()) { return; }

            if (CustomPropertiesDefined.TryGetValue("Button", out object? buttonObj) && buttonObj is System.Windows.Controls.Button button)
            {
                MonitoredButtonValue = button;
                MonitoredButtonValue.Click += OnButtonClicked;

                Log.WriteLine($"Button trigger '{Name}' started monitoring button '{button.Name}'");
            }
            else
            {
                throw new InvalidOperationException($"ButtonTrigger '{Name}' cannot start: 'Button' property is missing or invalid.");
            }
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
            Log.WriteLine($"Button trigger '{Name}' activated");
            
            // Trigger the configured action
            ActivateTrigger();
        }

        public override TriggerModel ToModel()
        {
            TriggerModel model = base.ToModel();
            model.CustomProperties = CustomProperties;
            return model;
        }
    }
}