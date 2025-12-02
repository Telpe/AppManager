using System.Diagnostics;
using System.Windows.Controls;

namespace AppManager.Settings.AppGroupEdit
{
    /// <summary>
    /// Interaction logic for ShortcutsPage.xaml
    /// </summary>
    public partial class ShortcutsPage : Page, IPageWithParameter
    {
        private string PageNameStored = "";

        public ShortcutsPage()
        {
            InitializeComponent();
        }

        public void LoadPageByName(string pageName)
        {
            

            PageNameStored = pageName;
            // Use the pageName parameter as needed
            // For example, you can set it to a label or use it for navigation logic
            // This is just a placeholder implementation
            Debug.WriteLine($"Page name set to: {pageName}");
        }

        public bool HasUnsavedChanges()
        {
            throw new System.NotImplementedException();
        }
    }
}