using System;
using System.Windows;

namespace AppManager.Settings.Utils
{
    public static class ScreenHelper
    {
        /// <summary>
        /// Gets the primary screen resolution using WPF's DPI-aware SystemParameters
        /// </summary>
        /// <returns>A Size object containing the width and height of the primary screen in WPF units</returns>
        public static Size GetScreenResolution()
        {
            return new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }

        /// <summary>
        /// Gets the working area of the primary screen (excludes taskbar) in WPF units
        /// </summary>
        /// <returns>A Size object containing the working area dimensions</returns>
        public static Size GetWorkingAreaSize()
        {
            return new Size(SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
        }

        /// <summary>
        /// Gets the working area bounds of the primary screen (excludes taskbar)
        /// </summary>
        /// <returns>A Rect object containing the working area bounds</returns>
        public static Rect GetWorkingArea()
        {
            return SystemParameters.WorkArea;
        }

        /// <summary>
        /// Checks if there is room to expand a window in the specified direction
        /// </summary>
        /// <param name="window">The window to check</param>
        /// <param name="direction">Direction vector (e.g., new Vector(1, 0) for right, new Vector(0, -1) for up)</param>
        /// <param name="wpfUnits">Amount to expand in WPF units</param>
        /// <returns>True if there is room to expand, false otherwise</returns>
        public static bool CanExpandWindow(Window window, Vector direction, double wpfUnits)
        {
            if (window == null)
                return false;

            var workingArea = GetWorkingArea();
            
            // Normalize the direction vector
            direction.Normalize();
            
            // Calculate the new window bounds after expansion
            double newLeft = window.Left;
            double newTop = window.Top;
            double newWidth = window.Width;
            double newHeight = window.Height;

            // Apply expansion based on direction
            if (direction.X > 0) // Expanding right
            {
                newWidth += wpfUnits;
            }
            else if (direction.X < 0) // Expanding left
            {
                newLeft -= wpfUnits;
                newWidth += wpfUnits;
            }

            if (direction.Y > 0) // Expanding down
            {
                newHeight += wpfUnits;
            }
            else if (direction.Y < 0) // Expanding up
            {
                newTop -= wpfUnits;
                newHeight += wpfUnits;
            }

            // Check if the new bounds fit within the working area
            return newLeft >= workingArea.Left &&
                   newTop >= workingArea.Top &&
                   (newLeft + newWidth) <= (workingArea.Left + workingArea.Width) &&
                   (newTop + newHeight) <= (workingArea.Top + workingArea.Height);
        }

        /// <summary>
        /// Gets the maximum amount a window can expand in the specified direction
        /// </summary>
        /// <param name="window">The window to check</param>
        /// <param name="direction">Direction vector</param>
        /// <returns>Maximum expansion in WPF units, or 0 if no expansion possible</returns>
        public static double GetMaxExpansion(Window window, Vector direction)
        {
            if (window == null)
                return 0;

            var workingArea = GetWorkingArea();
            direction.Normalize();

            if (direction.X > 0) // Right
            {
                return Math.Max(0, workingArea.Right - (window.Left + window.Width));
            }
            else if (direction.X < 0) // Left  
            {
                return Math.Max(0, window.Left - workingArea.Left);
            }
            else if (direction.Y > 0) // Down
            {
                return Math.Max(0, workingArea.Bottom - (window.Top + window.Height));
            }
            else if (direction.Y < 0) // Up
            {
                return Math.Max(0, window.Top - workingArea.Top);
            }

            return 0;
        }
    }
}