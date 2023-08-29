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

namespace closeapps
{
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
            InitializeComponent();

            this.Closing += Window_Closing;

            LoadAppsList();

            CloseSelectedAppsButton.Click += CloseSelectedAppsButton_Click;
            CheckAppsRunning = CheckRunning(ManagedAppsToArray());

            AddAppButton.Click += AddAppButton_Click;

            CloseSelectedAppsButton.Focus();
        }

        private void LoadAppsList()
        {
            
                if (ManagedAppsFileExist())
                {
                    AppManaged[] appsList = LoadAppsListFromFile();
                    AddAppsListToList(appsList);
                }
                else
                {
                    AddAppsListToList(AppsListLoaded);
                }
                
        }

        private AppManaged[] LoadAppsListFromFile()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
            string fileText = File.ReadAllText(filePath);
            AppManaged[] apps = JsonConvert.DeserializeObject<AppManaged[]>(fileText);
            return apps;
        }

        private bool ManagedAppsFileExist()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
            return File.Exists(filePath);
        }

        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {

            AddAppToList(new AppToClose(""));
        }

        private void AddAppsListToList(string[] appName) 
        {
            for(int i = 0; i < appName.Length; i++)
            {
                AddAppToList(new AppToClose(appName[i]));
            }
        }

        private void AddAppsListToList(AppManaged[] apps)
        {
            for (int i = 0; i < apps.Length; i++)
            {
                AddAppToList(new AppToClose(apps[i].Name, apps[i].Selected));
            }
        }

        private void AddAppToList(AppToClose appToManage)
        {
            appslist.Children.Add(appToManage);
        }

        public static Task StoreAppsList(AppManaged[] appManagedList) 
        {
            Task work = Task.Run(() =>
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string filePath = System.IO.Path.Combine(documentsPath, "ManagedAppsLists.json");
                string asJson = JsonConvert.SerializeObject(appManagedList, Formatting.Indented);
                File.WriteAllText(filePath, asJson);

                Debug.WriteLine("Done saving");
            });
            return work;
        }

        private AppManaged[] ManagedAppsToArray()
        {
            List<AppManaged> apps = new List<AppManaged>();
            int amount = appslist.Children.Count;
            for (int i = 0; i < amount; i++)
            {
                if( 0 < ((AppToClose)appslist.Children[i]).Appname.Text.Length)
                {
                    apps.Add(new AppManaged()
                    {
                        Name = ((AppToClose)appslist.Children[i]).Appname.Text,
                        Selected = (bool)((AppToClose)appslist.Children[i]).SelectedBox.IsChecked
                    });
                }
            }

            AppManaged[] AppManagedList;
            AppManagedList = apps.ToArray();

            return AppManagedList;
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

            foreach (AppToClose toClose in appslist.Children)
            {
                if (toClose.Selected)
                {
                    var closer = new Close(toClose.Appname.Text, options);

                    closer.DoClose();
                }
                
            }
            
        }

        private Task CheckRunning(AppManaged[] appManagedList)
        {
            var Options = new TaskContinuationOptions();
            CheckingIfRunning = true;
            Task CheckIfRunning = Task.Run(() =>{

                while (CheckingIfRunning)
                {
                    for (int index = 0; index < appManagedList.Length; index++)
                    {
                        try
                        {
                            Debug.WriteLine("Checking app " + (index + 1).ToString() + "/" + appManagedList.Length.ToString());
                            if (Process.GetProcessesByName(appManagedList[index].Name).Length > 0)
                            {
                                Debug.WriteLine("Running");
                            }
                            else
                            {
                                Debug.WriteLine("Stopped");
                            }
                            //throw new NotImplementedException("Checking if each listed app are running");
                        }
                        catch (Exception e) { Debug.WriteLine("Checked app " + (index + 1).ToString() + "/" + appManagedList.Length.ToString()); }
                    }

                    Task.Delay(2500).Wait();
                }
                
            });

            return CheckIfRunning;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            CheckingIfRunning = false;
            AppManaged[] apps = ManagedAppsToArray();
            Debug.WriteLine("Exit wait");
            Task saveWork = StoreAppsList(apps);
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

    }
}
