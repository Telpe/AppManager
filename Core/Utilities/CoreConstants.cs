namespace AppManager.Core.Utilities
{
    /// <summary>
    /// Common constants used across the Core project
    /// </summary>
    public static class CoreConstants
    {
        /// <summary>
        /// Default delay time in milliseconds for test actions that require timing
        /// </summary>
        public const int DefaultActionDelay = 3000;

        /// <summary>
        /// Standard delay time in milliseconds for UI operations that require timing
        /// </summary>
        public const int StandardUIDelay = 100;

        /// <summary>
        /// Standard delay time in milliseconds for process restart operations
        /// </summary>
        public const int ProcessRestartDelay = 100;

        /// <summary>
        /// Minimal delay time in milliseconds for thread synchronization
        /// </summary>
        public const int MinimalSyncDelay = 1;
    }
}