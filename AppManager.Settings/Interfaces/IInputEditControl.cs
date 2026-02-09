using AppManager.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Settings.Interfaces
{
    public interface IInputEditControl
    {
        public event EventHandler? OnEdited;
        
        public event EventHandler? OnCancel;
        
        public event EventHandler<InputEditEventArgs>? OnSave;

    }
}
