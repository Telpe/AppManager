using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using AppManager.Core.Actions;

namespace AppManager.Core.Triggers
{
    internal class ButtonTrigger : BaseTrigger
    {
        public override TriggerTypeEnum TriggerType => TriggerTypeEnum.Button;
        public override string Description => "Monitors UI button clicks";

        private TriggerModel _parameters;
        private System.Windows.Controls.Button _monitoredButton;

        public ButtonTrigger(string name = null) : base(name)
        {
        }

        public override bool CanStart(TriggerModel parameters = null)
        {
            return parameters?.CustomProperties?.ContainsKey("Button") == true;
        }

        public override async Task<bool> StartAsync(TriggerModel parameters = null)
        {
            if (IsActive || parameters == null)
                return false;

            try
            {
                _parameters = parameters;

                if (parameters.CustomProperties.TryGetValue("Button", out var buttonObj) && buttonObj is System.Windows.Controls.Button button)
                {
                    _monitoredButton = button;
                    _monitoredButton.Click += OnButtonClicked;
                    
                    IsActive = true;
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
        }

        public override async Task<bool> StopAsync()
        {
            if (!IsActive)
                return true;

            try
            {
                if (_monitoredButton != null)
                {
                    _monitoredButton.Click -= OnButtonClicked;
                }
                
                IsActive = false;
                System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' stopped");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping button trigger '{Name}': {ex.Message}");
                return false;
            }
        }

        private void OnButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Button trigger '{Name}' activated");
            
            // Trigger the configured action
            OnTriggerActivated("target_app", AppActionEnum.Launch, null, new { ButtonName = _monitoredButton?.Name });
        }

        public override void Dispose()
        {
            _ = StopAsync();
            base.Dispose();
        }
    }
}