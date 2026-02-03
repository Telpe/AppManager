using AppManager.Settings.Interfaces;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppManager.Settings.Utilities
{
    /// <summary>
    /// Manages application-wide overlay functionality
    /// </summary>
    public class OverlayManager
    {
        private Stack<IInputEditControl> ContentQueueValue = [];

        private ScrollViewer _scrollViewer = new()
            {
                IsHitTestVisible = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        private Grid _overlayContainer = new()
            {
                Background = Brushes.Transparent,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = true
            };
        private Border _overlayBackground = new()
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                IsHitTestVisible = true
            };
        private Border _activeArea = new()
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(BorderRadiusIndent),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = true,
                MaxWidth = 1600,
                MaxHeight = 1400,
                Padding = new Thickness(BorderRadiusIndent),
                MinWidth = 250,
                MinHeight = 300
            };
        private object? CurrentContentValue
        {
            get { return _scrollViewer.Content; }
            set { _scrollViewer.Content = value; }
        }
        private bool ClickHideValue = true;
        private readonly Window _parentWindow;
        public static double BorderRadiusIndent { get; } = 5.0;
        public static Vector2 ContentAreaSizeOfParentDefault { get; } = new Vector2(0.8f, 0.8f);
        private static Vector2? ContentAreaSizeOfParentValue { get; set; }
        public static Vector2 ContentAreaSizeOfParent { get { return ContentAreaSizeOfParentValue ?? ContentAreaSizeOfParentDefault; } }
        public object? CurrentContent { get =>CurrentContentValue; }
        //public Stack<IInputEditControl<object>> ContentQueue { get => ContentQueueValue; }

        public OverlayManager(Window parentWindow, Vector2? sizeFactor = null, bool clickHide = true)
        {
            ContentAreaSizeOfParentValue = sizeFactor;
            _parentWindow = parentWindow;
            ClickHideValue = clickHide;
            InitializeOverlay();
        }

        /// <summary>
        /// Shows the overlay with specified content and active area size
        /// </summary>
        /// <param name="content">Content that inherits from OverlayContent</param>
        /// <param name="sizeFactor">Size factor of parent window (0-1)</param>
        /// <param name="clickHide">Whether clicking outside the active area should hide the overlay</param>
        public void ShowOverlay(IInputEditControl content)
        {
            ContentQueueValue.Push(content);
            CurrentContentValue = content;

            // Add background click handler if clickHide is enabled
            if (ClickHideValue)
            {
                _overlayBackground.MouseDown += OnBackgroundClick;
            }

            UpdateSize();

            // Enable window size changed event handler
            _parentWindow.SizeChanged += OnWindowSizeChanged;

            // Show overlay
            _overlayContainer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the overlay
        /// </summary>
        public bool HideOverlay()
        {
            ContentQueueValue.Pop();

            if (ContentQueueValue.TryPeek(out IInputEditControl? lastQueued))
            {
                CurrentContentValue = lastQueued;

                return false;
            }

            _parentWindow.SizeChanged -= OnWindowSizeChanged;
            _overlayBackground.MouseDown -= OnBackgroundClick;

            _overlayContainer.Visibility = Visibility.Collapsed;

            return true;
        }

        private void OnBackgroundClick(object sender, MouseButtonEventArgs eve)
        {
            HideOverlay();
        }

        /// <summary>
        /// Gets whether the overlay is currently visible
        /// </summary>
        public bool IsOverlayVisible => _overlayContainer?.Visibility == Visibility.Visible;

        private void InitializeOverlay()
        {
            if (_parentWindow.Content is not Grid mainGrid)
            {
                throw new InvalidOperationException("Parent window must have a Grid as its main content");
            }

            _activeArea.Child = _scrollViewer;
            _overlayContainer.Children.Add(_overlayBackground);
            _overlayContainer.Children.Add(_activeArea);

            Grid.SetRowSpan(_overlayContainer, mainGrid.RowDefinitions.Count > 0 ? mainGrid.RowDefinitions.Count : 1);
            Grid.SetColumnSpan(_overlayContainer, mainGrid.ColumnDefinitions.Count > 0 ? mainGrid.ColumnDefinitions.Count : 1);
            Panel.SetZIndex(_overlayContainer, 1000);

            mainGrid.Children.Add(_overlayContainer);
        }

        private void OnWindowSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        public void UpdateSize()
        {
            _activeArea.Width = _parentWindow.ActualWidth * (ContentAreaSizeOfParent.X < 0 ? ContentAreaSizeOfParentDefault.X : ContentAreaSizeOfParent.X);
            _activeArea.Height = _parentWindow.ActualHeight * (ContentAreaSizeOfParent.Y < 0 ? ContentAreaSizeOfParentDefault.Y : ContentAreaSizeOfParent.Y);
        }
    }
}