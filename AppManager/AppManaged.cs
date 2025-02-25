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
        public bool IncludeSimilar
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
        public bool ForceExit
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
        public bool IncludeChildren
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
            Height = 22;
            double checkboxNewWidth = 40;
            double checkboxWidth = 16;

            SelectedBox = new CheckBox();
            SelectedBox.Width = checkboxWidth;
            SelectedBox.Height = checkboxWidth;
            int leftright = (int)((checkboxNewWidth - checkboxWidth - SelectedBox.Padding.Left - SelectedBox.Padding.Right) * 0.5);
            SelectedBox.Margin = new Thickness(leftright, 1, leftright, 1);
            SelectedBox.VerticalAlignment = VerticalAlignment.Center;

            AppNameBox = new TextBox();
            AppNameBox.Width = 160;
            AppNameBox.Padding = new Thickness(0, 0, 0, 0);
            AppNameBox.Margin = new Thickness(0, 1, 0, 1);
            AppNameBox.VerticalAlignment = VerticalAlignment.Center;

            
            ForceKillBox = new CheckBox();
            ForceKillBox.Width = checkboxWidth;
            ForceKillBox.Height = checkboxWidth;
            ForceKillBox.Margin = new Thickness(leftright, 1, leftright, 1);
            ForceKillBox.VerticalAlignment = VerticalAlignment.Center;

            IncludeChildrenBox = new CheckBox();
            IncludeChildrenBox.Width = checkboxWidth;
            IncludeChildrenBox.Height = checkboxWidth;
            IncludeChildrenBox.Margin = new Thickness(leftright, 1, leftright, 1);
            IncludeChildrenBox.VerticalAlignment = VerticalAlignment.Center;

            IncludeTasksLikeGivenBox = new CheckBox();
            IncludeTasksLikeGivenBox.Width = checkboxWidth;
            IncludeTasksLikeGivenBox.Height = checkboxWidth;
            IncludeTasksLikeGivenBox.Margin = new Thickness(leftright, 1, leftright, 1);
            IncludeTasksLikeGivenBox.VerticalAlignment = VerticalAlignment.Center;

            RunningStateLabel = new Label();
            RunningStateLabel.Padding = new Thickness(0, 0, 0, 0);
            RunningStateLabel.Margin = new Thickness(1, 1, 1, 1);

            this.Children.Add(SelectedBox);
            this.Children.Add(AppNameBox);
            this.Children.Add(IncludeTasksLikeGivenBox);
            this.Children.Add(IncludeChildrenBox);
            this.Children.Add(ForceKillBox);
            this.Children.Add(RunningStateLabel);


        }

    }
}
