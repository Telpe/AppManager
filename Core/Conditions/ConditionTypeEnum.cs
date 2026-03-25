namespace AppManager.Core.Conditions
{
    public enum ConditionTypeEnum
    {
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