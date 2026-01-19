namespace AppManager.Core.Conditions
{
    public enum ConditionTypeEnum
    {
        None,
        ProcessRunning,
        WindowExists,
        WindowFocused,
        WindowMinimized,
        FileExists,
        NetworkPortOpen,
        TimeRange,
        DayOfWeek,
        SystemUptime,
        PreviousActionSuccess
    }
}