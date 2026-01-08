using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Settings.UI
{
    public interface IInputEditControl
    {
        public event EventHandler? Edited;
        
        public event EventHandler? Cancel;
        
        public event EventHandler<InputEditEventArgs>? Save;

    }
}
