using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
//using System.Text.Json;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace closeapps
{


    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Timers.Timer CheckIfAppsRunning = new System.Timers.Timer();
        
        private string[] AppsListLoaded = { "Steam", "GOG", "Galaxy", "Epic", "Battle", "wgc", "Discord", "mbam", "action", "dsclock", "msedge", "hMailServer", "ScpService", "SRService" };

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += Window_Closing;

            LoadAppsList();

            EnableButtons();
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
            AppsHeader.Children.Add(new AppManaged() { AppName = "All Apps", IsSelected = false, IsSimilarIncluded=false, IsChildrenIncluded=true, IsExitForced=false });
        }

        private AppManaged[] LoadAppsListFromFile()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
            string fileText = File.ReadAllText(filePath);
            AppManaged[] apps = JsonConvert.DeserializeObject<AppManaged[]>(fileText);
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
            for (int i = 0; i < appNames.Length; i++)
            {
                AddAppToList(appNames[i]);
            }
        }

        private void AddAppsListToList(AppManaged[] apps)
        {
            for (int i = 0; i < apps.Length; i++)
            {
                AddAppToList(apps[i]);
            }
        }

        private void AddAppToList(string name, bool selected = true)
        {
            AddAppToList(new AppManaged() { AppName = name, IsSelected=selected });
        }

        private void AddAppToList(AppManaged appToManage)
        {
            AppsList.Children.Add(appToManage);
            appToManage.AppNameBox.MinWidth = 32;
        }

        public void StoreAppsList(AppManagedModel[] models)
        {
                Debug.WriteLine("Thread StoreAppsList: " + Thread.CurrentThread.ManagedThreadId.ToString());
                try
                {
                    string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");

                    File.WriteAllText(filePath, JsonConvert.SerializeObject(models, Formatting.Indented));

                    Debug.WriteLine("Done saving");
                }
                catch(Exception e) 
                {
                    Debug.Write($"{e.Message}\n{e.StackTrace}\n{e.TargetSite.Name}\n");
                }
        }

        public AppManagedModel[] AppsManagedToModels()
        {
            AppManagedModel[] apps = new AppManagedModel[AppsList.Children.Count];

                AppManaged app;
            int i = 0, index = 0;
                while (i < AppsList.Children.Count)
                {
                    try
                    {
                        app = (AppManaged)AppsList.Children[i];
                        if (0 < app.AppName.Length)
                        {
                            apps[index] = (AppManagedModel)app;
                            index++;
                        }
                    }
                    catch { }

                    i++;
                }

            

            if (i != index)
            {
                AppManagedModel[] appsT = new AppManagedModel[index];
                Array.ConstrainedCopy(apps, 0, appsT, 0, index);
                return appsT;
            }

            return apps;
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
                if (toClose.IsSelected)
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
