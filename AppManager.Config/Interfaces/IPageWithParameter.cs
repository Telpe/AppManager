namespace AppManager.Config.Interfaces
{
    /// <summary>
    /// Interface that defines the contract for pages that accept parameters
    /// </summary>
    public interface IPageWithParameter
    {
        void LoadPageByName(string pageName);

        bool HasUnsavedChanges();
    }
}