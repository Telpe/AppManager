using System.Windows;
using System.Windows.Controls;

namespace AppManager.UI
{
    /// <summary>
    /// Example implementation of OverlayContent for demonstration
    /// </summary>
    public class ExampleOverlayContent : OverlayContent
    {
        public ExampleOverlayContent(string message = "This is an overlay!")
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var messageLabel = new Label
            {
                Content = message,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var closeButton = new Button
            {
                Content = "Close Overlay",
                Width = 120,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            closeButton.Click += (sender, e) => DisableOverlay();

            stackPanel.Children.Add(messageLabel);
            stackPanel.Children.Add(closeButton);

            Content = stackPanel;
        }
    }
}