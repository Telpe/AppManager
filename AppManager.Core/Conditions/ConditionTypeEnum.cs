namespace AppManager.Core.Conditions
{
    public enum ConditionTypeEnum
    {
        None,
        ProcessRunning,
        ProcessNotRunning,
        WindowExists,
        WindowNotExists,
        WindowFocused,
        WindowNotFocused,
        WindowMinimized,
        WindowNotMinimized,
        FileExists,
        FileNotExists,
        NetworkPortOpen,
        NetworkPortClosed,
        TimeRange,
        DayOfWeek,
        SystemUptime
    }
}