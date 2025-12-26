using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Settings.UI
{
    public interface IInputEditControl
    {
        public event EventHandler? OnEdit;
        
        public event EventHandler? OnCancel;
        
        public event EventHandler<InputEditEventArgs>? OnSave;

    }
}
