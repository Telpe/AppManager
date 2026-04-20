using AppManager.OsApi.Events;
using AppManager.OsApi.Models;

namespace AppManager.OsApi.Interfaces
{
    public interface IInputControl
    {
        ObservableEvent<object?, HotkeyModel> KeyEvent { get; }


    }
}
