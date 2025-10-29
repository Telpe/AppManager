using AppManager.Profile;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;

namespace AppManager.Pages
{
    /// <summary>
    /// Interaction logic for Apps.xaml
    /// </summary>
    public partial class AppPage : Page , IPageWithParameter
    {
        private string _PageName = "";

        public AppPage()
        {
            InitializeComponent();

            try
            {
                //LoadAppsList();
                EnableButtons();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainWindow error");
                Debug.WriteLine(ex.Message.ToString());
                Debug.WriteLine(ex.StackTrace.ToString());
            }
        }

        public void SetPageName(string pageName)
        {
            string smallPageName = pageName.ToLower();
            AppManagedModel AppManagedModel = App.CurrentProfile.Apps.Where(app => app.AppName.ToLower() == smallPageName).FirstOrDefault();
            if (null == AppManagedModel)
            {
                // TODO: Use default page if none in use.
                return;
            }


            _PageName = pageName;

            AppManaged managedApp = new(AppManagedModel);

            AppManagedEdit.Content = managedApp;
            // Use the pageName parameter as needed
            // For example, you can set it to a label or use it for navigation logic
            // This is just a placeholder implementation
            Debug.WriteLine($"Page name set to: {pageName}");
        }

        private void EnableButtons()
        {
            /*CloseSelectedAppsButton.Click += CloseSelectedAppsButton_Click;
            AddAppButton.Click += AddAppButton_Click;

            AppScanStartStop.Click += AppScanStartStop_Click;  // Add this line
            // Also initialize the button state based on the timer's initial state
            AppScanStartStop.Background = CheckIfAppsRunningValue.Enabled ?
                System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Yellow;
            AppScanStartStop.Content = CheckIfAppsRunningValue.Enabled ? "Stop Scan" : "Start Scan";

            CloseSelectedAppsButton.Focus();*/
        }

        

        
    }
}
