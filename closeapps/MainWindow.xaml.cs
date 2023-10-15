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

namespace closeapps
{


    public delegate AppManagedModel[] AppsManagedToModelsCallback();

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task CheckAppsRunning;
        //private Task LoadApps = null;
        private bool CheckingIfRunning = false;

        private string[] AppsListLoaded = { "Steam", "GOG", "Galaxy", "Epic", "Battle", "wgc", "Discord", "mbam", "action", "dsclock", "msedge", "hMailServer", "ScpService", "SRService" };

        public MainWindow()
        {
            Debug.WriteLine("Thread MainWindow: " + Thread.CurrentThread.ManagedThreadId.ToString());
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

            CheckAppsRunning = CheckRunning();
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

        public Task StoreAppsList(AppManagedModel[] models)
        {
            Task work = Task.Run(() =>
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
                
            });
            return work;
        }

        public AppManagedModel[] AppsManagedToModels()
        {
            Debug.WriteLine("Thread AppsManagedToModels: " + Thread.CurrentThread.ManagedThreadId.ToString());
            
            
                Debug.WriteLine("Thread AppsManagedToModels invoked: " + Thread.CurrentThread.ManagedThreadId.ToString());
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

        private Task CheckRunning()
        {
            var Options = new TaskContinuationOptions();
            CheckingIfRunning = true;
            Task CheckIfRunning = Task.Run(() =>
            {
                try
                {
                    while (CheckingIfRunning)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            for (int index = 0; index < AppsList.Children.Count; index++)
                            {
                                AppManaged app = new AppManaged();
                                try
                                {
                                    app = (AppManaged)AppsList.Children[index];
                                    if (0 < app.AppName.Length)
                                    {
                                        Debug.WriteLine("Checking app " + (index + 1).ToString() + "/" + AppsList.Children.Count.ToString());
                                        if (Process.GetProcessesByName(app.AppName).Length > 0)
                                        {
                                            Debug.WriteLine("Running");
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Stopped");
                                        }
                                    }


                                }
                                catch (Exception e) { Debug.WriteLine("Checked app " + (index + 1).ToString() + "/" + app.AppName.Length.ToString()); }

                            }

                        });

                        Task.Delay(2500).Wait();
                    }
                }
                catch (Exception e)
                {
                    Debug.Write($"{e.Message}\n{e.StackTrace}\n{e.TargetSite.Name}\n");
                }

            });

            return CheckIfRunning;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            CheckingIfRunning = false;
            Debug.WriteLine("Exit wait");
            Task saveWork = StoreAppsList(AppsManagedToModels());
            //Task delay = Task.Run(()=> { Task.Delay(6000).Wait(); }); // Task.Delay does not start until awaited!
            //Debug.WriteLine("Exit in 6");

            Debug.WriteLine("CheckApps " + CheckAppsRunning.Status.ToString());
            CheckAppsRunning.Wait();
            Debug.WriteLine("CheckApps " + CheckAppsRunning.Status.ToString());

            Debug.WriteLine("saveWork " + saveWork.Status.ToString());
            saveWork.Wait();
            Debug.WriteLine("saveWork " + saveWork.Status.ToString());

            //Debug.WriteLine("delay " + delay.Status.ToString());
            //delay.Wait();
            //Debug.WriteLine("delay " + delay.Status.ToString());

            Debug.WriteLine("Exit");
            e.Cancel = false;
        }

        private void ConsoleWriteline(string text)
        {
            ConsolePanel.Children.Add(new Label() { Content = text });
        }
    }
}
