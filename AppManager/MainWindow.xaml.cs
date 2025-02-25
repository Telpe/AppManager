using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

namespace AppManager
{


    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer CheckIfAppsRunning = new System.Timers.Timer();
        
        private string[] AppsListLoaded = { "Steam", "GOG", "Galaxy", "Epic", "Battle", "wgc", "Discord", "msedge", };

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                this.Closing += Window_Closing;

                LoadAppsList();

                EnableButtons();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MainWindow error");
                Debug.WriteLine(ex.Message.ToString());
                Debug.WriteLine(ex.StackTrace.ToString());
            }
        }

        private void EnableButtons()
        {
            CloseSelectedAppsButton.Click += CloseSelectedAppsButton_Click;
            AddAppButton.Click += AddAppButton_Click;


            CloseSelectedAppsButton.Focus();
        }

        private void LoadAppsList()
        {

            if (ManagedAppsFileExists())
            {
                AppManaged[] apps = LoadAppsListFromFile();
                AddAppsListToList(apps);
            }
            else
            {
                AddAppsListToList(AppsListLoaded);
            }

            AddAppsListHeader();

            CheckIfAppsRunning.Interval = 2500;
            CheckIfAppsRunning.AutoReset = true;
            CheckIfAppsRunning.Elapsed += new ElapsedEventHandler(CheckRunningHandler);
            CheckIfAppsRunning.Start();
        }

        private void AddAppsListHeader()
        {
            AppsHeader.Children.Add(new AppManaged() { AppName = "All Apps", Selected = false, IncludeSimilar=false, IncludeChildren=true, ForceExit=false });
        }

        private AppManaged[] LoadAppsListFromFile()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
            string fileText = File.ReadAllText(filePath);
            AppManaged[] apps = JsonSerializer.Deserialize<AppManaged[]>(fileText);
            return apps;
        }

        private bool ManagedAppsFileExists()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
            return File.Exists(filePath);
        }

        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {

            AddAppToList(new AppManaged());
        }

        private void AddAppsListToList(string[] appNames)
        {
            foreach (string appName in appNames) { AddAppToList(appName); }
        }

        private void AddAppsListToList(AppManaged[] apps)
        {
            foreach (AppManaged app in apps) { AddAppToList(app); }
        }

        private void AddAppToList(string name, bool selected = true)
        {
            var app = new AppManaged() { AppName = name, Selected = selected };
            AddAppToList(app);

        }

        private void AddAppToList(AppManaged appToManage)
        {
            AppsList.Children.Add(appToManage);
            
        }

        public void StoreAppsList(AppManagedModel[] models)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");

                File.WriteAllText(filePath, JsonSerializer.Serialize(models));

                Debug.WriteLine("Done saving");
            }
            catch(Exception e) 
            {
                Debug.Write($"{e.Message}\n{e.StackTrace}\n{e.TargetSite.Name}\n");
            }
        }

        public AppManagedModel[] AppsManagedToModels()
        {
            IEnumerable<AppManagedModel> apps = Enumerable.Empty<AppManagedModel>();

            foreach (AppManaged app in AppsList.Children)
            {
                if (0 < app.AppName.Length)
                {
                    apps = apps.Append((AppManagedModel)app);
                }
            }

            return apps.ToArray();
        }
        
        private void CloseSelectedAppsButton_Click(Object sender, RoutedEventArgs e)
        {
            var options = new CloserOptions
            {
                ExitWhenDone = false,
                ForceKill = true,
                IncludeChildren = true,
                IncludeTasksLikeGiven = true
            };

            foreach (AppManaged toClose in AppsList.Children)
            {
                if (toClose.Selected)
                {
                    var closer = new Close(toClose.AppName, options);

                    closer.DoClose();
                }

            }

        }

        private void CheckRunningHandler(object sender, ElapsedEventArgs eve)
        {
            Application.Current.Dispatcher.Invoke(CheckRunning);
        }

        private void CheckRunning()
        {
            for (int i = 0; i < AppsList.Children.Count; i++)
            {
                AppManaged app;
                try
                {
                    app = (AppManaged)AppsList.Children[i];
                }
                catch(Exception e)
                {
                    app = new AppManaged();
                    Debug.WriteLine(e.Message + "\n" + e.StackTrace);
                }

                if (0 < app.AppName.Length)
                {
                    var processs = Process.GetProcessesByName(app.AppName);
                    app.IsRunning = processs.Length;
                    
                }

            }

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            CheckIfAppsRunning.Stop();
            StoreAppsList(AppsManagedToModels());
            
            e.Cancel = false;
        }

        private void ConsoleWriteline(string text)
        {
            ConsolePanel.Children.Add(new Label() { Content = text });
        }
    }
}
