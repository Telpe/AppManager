using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace closeapps
{
    public class AppToClose : DockPanel
    {
        public TextBox Appname;
        public CheckBox InUse;
        public bool Active
        {
            get
            {
                return (bool)InUse.IsChecked;
            }
            set
            {
                InUse.IsChecked = value;
            }
        }
        

        public AppToClose(string appname)
        {
            Appname = new TextBox
            {
                Text = appname
            };
            
            InUse = new CheckBox
            {
                IsChecked = true
            };

            this.Children.Add(Appname);
            this.Children.Add(InUse);
        }
    }
}
