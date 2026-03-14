using AppManager.OsApi.Abstractions;

namespace AppManager.OsApi
{
    public static class API
    {
        private static IOsApi CurrentApiValue = OsApiLoader.Load();
        public static IOsApi CurrentApi
        {
            get => CurrentApiValue;
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                CurrentApiValue = value;
            }
        }
    }
}
