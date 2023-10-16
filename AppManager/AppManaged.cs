using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AppManager
{
    public class AppManaged : DockPanel, IAppManaged
    {
        public TextBox AppNameBox;
        public CheckBox SelectedBox, ForceKillBox, IncludeChildrenBox, IncludeTasksLikeGivenBox;
        public Label RunningStateLabel;
        protected int _IsRunning = 0;

        public string AppName
        {
            get { return AppNameBox.Text; }
            set { AppNameBox.Text = value; }
        }
        public int IsRunning
        {
            get { return _IsRunning; }
            set
            {
                _IsRunning = value;

                RunningState = value.ToString();
                
            }
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

        public string RunningState
        {
            get
            {
                return RunningStateLabel.Content.ToString();
            }
            set
            {
                RunningStateLabel.Content = value;
            }
        }

        public AppManaged()
        {
            AppNameBox = new TextBox();

            SelectedBox = new CheckBox();
            ForceKillBox = new CheckBox();
            IncludeChildrenBox = new CheckBox();
            IncludeTasksLikeGivenBox = new CheckBox();
            RunningStateLabel = new Label();

            this.Children.Add(SelectedBox);
            this.Children.Add(AppNameBox);
            this.Children.Add(IncludeTasksLikeGivenBox);
            this.Children.Add(IncludeChildrenBox);
            this.Children.Add(ForceKillBox);
            this.Children.Add(RunningStateLabel);


        }

    }
}
