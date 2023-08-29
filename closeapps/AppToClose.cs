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
        public CheckBox SelectedBox;
        protected bool? _IsRunning = false;
        public bool? IsRunning
        {
            get { return _IsRunning; }
        }
        public bool Selected
        {
            get
            {
                return (bool)SelectedBox.IsChecked;
            }
            set
            {
                SelectedBox.IsChecked = value;
            }
        }
        

        public AppToClose(string appname, bool selected = true)
        {
            Appname = new TextBox
            {
                Text = appname
            };
            
            SelectedBox = new CheckBox
            {
                IsChecked = selected
            };

            this.Children.Add(Appname);
            this.Children.Add(SelectedBox);
        }
    }
}
