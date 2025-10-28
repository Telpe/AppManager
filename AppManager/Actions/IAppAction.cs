using System.Threading.Tasks;

namespace AppManager.Actions
{
    public interface IAppAction
    {
        AppActionEnum ActionName { get; }
        string Description { get; }
        Task<bool> ExecuteAsync(string appName, ActionParameters parameters = null);
        bool CanExecute(string appName, ActionParameters parameters = null);
    }
}