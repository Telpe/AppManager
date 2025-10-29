using System.Threading.Tasks;

namespace AppManager.Actions
{
    public interface IAppAction
    {
        string Description { get; }
        Task<bool> ExecuteAsync(ActionModel model);
        bool CanExecute(ActionModel model);
    }
}