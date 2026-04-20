namespace AppManager.OsApi.Models
{
    public interface IHotkeyRequester
    {
        void OnKeysPressed(HotkeyModel pressedKeys);
        void OnKeysReleased(HotkeyModel releasedKeys);
        void OnKeysClicked(HotkeyModel clickedKeys);
    }
}