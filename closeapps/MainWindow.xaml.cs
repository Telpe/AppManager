using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace closeapps
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            appslist.Children.Add(new AppToClose("Steam"));
            appslist.Children.Add(new AppToClose("GOG"));
            appslist.Children.Add(new AppToClose("Galaxy"));
            appslist.Children.Add(new AppToClose("Epic"));
            appslist.Children.Add(new AppToClose("Battle"));
            appslist.Children.Add(new AppToClose("wgc"));
            appslist.Children.Add(new AppToClose("Discord"));
            appslist.Children.Add(new AppToClose("mbam"));
            appslist.Children.Add(new AppToClose("action"));
            appslist.Children.Add(new AppToClose("dsclock"));

            CloseSelectedApps.Click += CloseSelectedApps_Click;

            CloseSelectedApps.Focus();
        }

        private void CloseSelectedApps_Click(Object sender, RoutedEventArgs e)
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
                if (toClose.Active)
                {
                    var closer = new Close(toClose.Appname.Text, options);

                    closer.DoClose();
                }
                
            }
            
        }
    }
}
