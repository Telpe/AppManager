using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace AppManager.Core.Utilities
{
    public class TrayApplication : IDisposable
    {
        private NotifyIcon TrayIconValue;
        private ContextMenuStrip ContextMenuValue;

        public TrayApplication()
        {
            ContextMenuValue = InitializeTrayMenu();
            TrayIconValue = InitializeTrayIcon();
        }

        private ContextMenuStrip InitializeTrayMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open AppManager", null, OnOpen);
            contextMenu.Items.Add("Settings", null, OnSettings);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("About", null, OnAbout);
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, OnExit);

            return contextMenu;
        }

        private NotifyIcon InitializeTrayIcon()
        {
            Icon trayIcon = LoadTrayIcon();

            // Create tray icon
            NotifyIcon notifyIcon = new NotifyIcon()
            {
                Icon = trayIcon,
                ContextMenuStrip = ContextMenuValue,
                Text = "AppManager",
                Visible = true
            };

            // Handle double-click to show main window
            notifyIcon.DoubleClick += OnOpen;

            return notifyIcon;
        }

        private Icon LoadTrayIcon()
        {
            return FileManager.GetDefaultIcon();
        }

        private void OnOpen(object? sender, EventArgs? e)
        {
            ActionManager.ExecuteAction(AppActionTypeEnum.Launch, "AppManager");
        }

        private void OnSettings(object? sender, EventArgs? e)
        {
            ActionManager.ExecuteAction(AppActionTypeEnum.Launch, "AppManager.Settings");
        }

        private void OnAbout(object? sender, EventArgs? e)
        {
            MessageBox.Show("AppManager v1.0\nApplication Manager", "About AppManager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnExit(object? sender, EventArgs? e)
        {
            Application.Exit();
        }

        public void Dispose()
        {
            TrayIconValue?.Dispose();
            ContextMenuValue?.Dispose();
        }
    }
}
