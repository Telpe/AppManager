using AppManager.Config.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace AppManager.Config.Interfaces
{
    public interface IInputEditControl
    {
        public event TrueEventHandler? OnEdited;
        
        public event TrueEventHandler? OnCancel;
        
        public event TrueEventHandler<InputEditEventArgs>? OnSave;

    }
}
