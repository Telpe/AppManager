using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppManager.Settings.UI
{
    /// <summary>
    /// Manages application-wide overlay functionality
    /// </summary>
    public class OverlayManager
    {
        private Grid? _overlayContainer;
        private Border? _overlayBackground;
        private Border? _activeArea;
        private OverlayContent? _currentContent;
        private readonly Window _parentWindow;
        private MouseButtonEventHandler? _backgroundClickHandler;

        public OverlayManager(Window parentWindow)
        {
            _parentWindow = parentWindow ?? throw new ArgumentNullException(nameof(parentWindow));
            InitializeOverlay();
        }

        /// <summary>
        /// Shows the overlay with specified content and active area size
        /// </summary>
        /// <param name="content">Content that inherits from OverlayContent</param>
        /// <param name="activeAreaWidthPercent">Width of active area as percentage (0-100)</param>
        /// <param name="activeAreaHeightPercent">Height of active area as percentage (0-100)</param>
        /// <param name="clickHide">Whether clicking outside the active area should hide the overlay</param>
        public void ShowOverlay(OverlayContent content, double activeAreaWidthPercent = 50, double activeAreaHeightPercent = 50, bool clickHide = true)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (activeAreaWidthPercent <= 0 || activeAreaWidthPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(activeAreaWidthPercent), "Must be between 0 and 100");

            if (activeAreaHeightPercent <= 0 || activeAreaHeightPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(activeAreaHeightPercent), "Must be between 0 and 100");

            // Remove any existing content
            if (_currentContent != null)
            {
                _currentContent.DisableOverlayRequested -= OnDisableOverlayRequested;
                _activeArea.Child = null;
            }

            // Remove existing background click handler if any
            if (_backgroundClickHandler != null)
            {
                _overlayBackground.MouseDown -= _backgroundClickHandler;
                _backgroundClickHandler = null;
            }

            // Add background click handler if clickHide is enabled
            if (clickHide)
            {
                _backgroundClickHandler = (sender, e) => HideOverlay();
                _overlayBackground.MouseDown += _backgroundClickHandler;
            }

            // Set up new content
            _currentContent = content;
            _currentContent.DisableOverlayRequested += OnDisableOverlayRequested;
            
            // Configure active area size
            _activeArea.Width = _parentWindow.ActualWidth * (activeAreaWidthPercent / 100.0);
            _activeArea.Height = _parentWindow.ActualHeight * (activeAreaHeightPercent / 100.0);
            
            // Add content to active area
            _activeArea.Child = _currentContent;
            
            // Show overlay
            _overlayContainer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the overlay
        /// </summary>
        public void HideOverlay()
        {
            if (_currentContent != null)
            {
                _currentContent.DisableOverlayRequested -= OnDisableOverlayRequested;
                _activeArea.Child = null;
                _currentContent = null;
            }

            // Remove background click handler
            if (_backgroundClickHandler != null)
            {
                _overlayBackground.MouseDown -= _backgroundClickHandler;
                _backgroundClickHandler = null;
            }

            _overlayContainer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Gets whether the overlay is currently visible
        /// </summary>
        public bool IsOverlayVisible => _overlayContainer?.Visibility == Visibility.Visible;

        private void InitializeOverlay()
        {
            // Get the main window's content (should be a Grid based on MainWindow.xaml)
            if (_parentWindow.Content is not Grid mainGrid)
                throw new InvalidOperationException("Parent window must have a Grid as its main content");

            // Create overlay container that spans the entire window
            _overlayContainer = new Grid
            {
                Background = Brushes.Transparent, // Transparent container
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = true
            };

            // Create dark semi-transparent background that blocks clicks
            _overlayBackground = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), // 50% transparent black
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = true // This blocks clicks to content behind
            };

            // Create the active area (frame) that will contain the content
            _activeArea = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = true,
                // Default size - will be overridden when showing overlay
                Width = 400,
                Height = 300
            };

            // Add components to overlay container
            _overlayContainer.Children.Add(_overlayBackground);
            _overlayContainer.Children.Add(_activeArea);

            // Add overlay to main grid with highest Z-order
            Grid.SetRowSpan(_overlayContainer, mainGrid.RowDefinitions.Count > 0 ? mainGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(_overlayContainer, mainGrid.ColumnDefinitions.Count > 0 ? mainGrid.ColumnDefinitions.Count : 1);
            Panel.SetZIndex(_overlayContainer, 1000); // High Z-index to appear on top

            mainGrid.Children.Add(_overlayContainer);
        }

        private void OnDisableOverlayRequested(object? sender, EventArgs? e)
        {
            HideOverlay();
        }

        /// <summary>
        /// Updates overlay size when window is resized
        /// </summary>
        public void UpdateSize()
        {
            if (_currentContent != null && IsOverlayVisible)
            {
                // Recalculate active area size based on current window size
                // You can store the percentages and recalculate here if needed
                // For now, keep existing size
            }
        }
    }
}