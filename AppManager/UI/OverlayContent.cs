using System;
using System.Windows.Controls;

namespace AppManager.UI
{
    /// <summary>
    /// Base class for overlay content that provides access to DisableOverlay functionality
    /// </summary>
    public abstract class OverlayContent : UserControl
    {
        /// <summary>
        /// Event raised when the overlay should be disabled
        /// </summary>
        public event EventHandler DisableOverlayRequested;

        /// <summary>
        /// Call this method from derived classes to request overlay dismissal
        /// </summary>
        protected void DisableOverlay()
        {
            DisableOverlayRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}