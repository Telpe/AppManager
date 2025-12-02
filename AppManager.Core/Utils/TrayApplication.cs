using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;

namespace AppManager.Core.Utils
{
    public class TrayApplication : IDisposable
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _contextMenu;

        public TrayApplication()
        {
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            // Create context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Open AppManager", null, OnOpen);
            _contextMenu.Items.Add("Settings", null, OnSettings);
            _contextMenu.Items.Add("-"); // Separator
            _contextMenu.Items.Add("About", null, OnAbout);
            _contextMenu.Items.Add("-"); // Separator
            _contextMenu.Items.Add("Exit", null, OnExit);

            // Load custom icon from PNG file
            Icon customIcon;
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppManagerIcon_temp.png");
                using (var bitmap = new Bitmap(iconPath))
                {
                    customIcon = Icon.FromHandle(bitmap.GetHicon());
                }
            }
            catch
            {
                // Fallback to system icon if custom icon fails to load
                customIcon = SystemIcons.Application;
            }

            // Create tray icon
            _trayIcon = new NotifyIcon()
            {
                Icon = customIcon,
                ContextMenuStrip = _contextMenu,
                Text = "AppManager",
                Visible = true
            };

            // Handle double-click to show main window
            _trayIcon.DoubleClick += OnOpen;
        }

        private void OnOpen(object? sender, EventArgs? e)
        {
            ActionManager.ExecuteActionAsync(AppActionTypeEnum.Launch, "AppManager");
        }

        private void OnSettings(object? sender, EventArgs? e)
        {
            ActionManager.ExecuteActionAsync(AppActionTypeEnum.Launch, "AppManager.Settings");
        }

        private void OnAbout(object? sender, EventArgs? e)
        {
            MessageBox.Show("AppManager v1.0\nApplication Manager", "About AppManager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnExit(object? sender, EventArgs? e)
        {
            ActionManager.ExecuteActionAsync(AppActionTypeEnum.Close, "AppManager").Wait();
            ActionManager.ExecuteActionAsync(AppActionTypeEnum.Close, "AppManager.Settings").Wait();
            Application.Exit();
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
