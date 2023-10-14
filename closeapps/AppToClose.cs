using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace closeapps
{
    public class AppToClose : DockPanel, IAppManaged
    {
        public TextBox AppNameBox;
        public CheckBox SelectedBox, ForceKillBox, IncludeChildrenBox, IncludeTasksLikeGivenBox;
        protected bool? _IsRunning = false;

        public string AppName
        {
            get { return AppNameBox.Text; }
            set { AppNameBox.Text = value; }
        }
        public bool? IsRunning
        {
            get { return _IsRunning; }
        }
        public bool IsSelected
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
        public bool IsSimilarIncluded
        {
            get
            {
                return (bool)IncludeTasksLikeGivenBox.IsChecked;
            }
            set
            {
                IncludeTasksLikeGivenBox.IsChecked = value;
            }
        }
        public bool IsExitForced
        {
            get
            {
                return (bool)ForceKillBox.IsChecked;
            }
            set
            {
                ForceKillBox.IsChecked = value;
            }
        }
        public bool IsChildrenIncluded
        {
            get
            {
                return (bool)IncludeChildrenBox.IsChecked;
            }
            set
            {
                IncludeChildrenBox.IsChecked = value;
            }
        }


        public AppToClose()
        {
            AppNameBox = new TextBox();

            SelectedBox = new CheckBox();
            ForceKillBox = new CheckBox();
            IncludeChildrenBox = new CheckBox();
            IncludeTasksLikeGivenBox = new CheckBox();

            this.Children.Add(SelectedBox);
            this.Children.Add(AppNameBox);
            this.Children.Add(IncludeTasksLikeGivenBox);
            this.Children.Add(IncludeChildrenBox);
            this.Children.Add(ForceKillBox);

        }

    }
}
