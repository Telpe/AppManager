namespace AppManager.Pages
{
    /// <summary>
    /// Interface that defines the contract for pages that accept parameters
    /// </summary>
    public interface IPageWithParameter
    {
        void SetPageName(string pageName);
    }
}