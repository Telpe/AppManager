using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.OsApi.Windows11
{
    public enum ShowWindowEnum : int
    {
        /// <summary>
        /// Hides the window and activates another window.
        /// </summary>
        Hide = 0,
        
        /// <summary>
        /// Activates and displays the window. If minimized/maximized, restores it to original size.
        /// </summary>
        ShowNormal = 1,
        
        /// <summary>
        /// Activates the window and displays it as minimized (icon on taskbar).
        /// </summary>
        ShowMinimized = 2,
        
        /// <summary>
        /// Activates the window and displays it maximized (fills entire screen).
        /// </summary>
        ShowMaximized = 3,
        
        /// <summary>
        /// Shows the window in its most recent size/position, but without making it active.
        /// </summary>
        ShowLatestNoActivate = 4,
        
        /// <summary>
        /// Activates the window and displays it in its current size and position.
        /// </summary>
        Show = 5,
        
        /// <summary>
        /// Minimizes the specified window and activates the next window in Z-order.
        /// </summary>
        Minimize = 6,
        
        /// <summary>
        /// Shows the window as minimized, but without activating it.
        /// </summary>
        ShowMinimizedNoActivate = 7,
        
        /// <summary>
        /// Shows the window in its current state, but without activating it.
        /// </summary>
        ShowNoActivate = 8,
        
        /// <summary>
        /// Activates and displays the window. If minimized or maximized, restores it to original size.
        /// </summary>
        Restore = 9,
        
        /// <summary>
        /// Sets the show-state based on the settings the program was started with (from STARTUPINFO).
        /// </summary>
        ShowDefault = 10,
        
        /// <summary>
        /// Minimizes a window, even if the thread owning the window is not responding (good for "hung" apps).
        /// </summary>
        ForceMinimize = 11
    }
}
