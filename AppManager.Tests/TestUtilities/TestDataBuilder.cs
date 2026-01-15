using AppManager.Core.Actions;
using AppManager.Core.Conditions;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using System.IO;
using System.Printing;

namespace AppManager.Tests.TestUtilities
{
    /// <summary>
    /// Builder class for creating test data objects
    /// </summary>
    public static class TestDataBuilder
    {
        /// <summary>
        /// Creates a basic ActionModel for testing
        /// </summary>
        public static ActionModel CreateBasicActionModel(
            AppActionTypeEnum actionType = AppActionTypeEnum.Launch,
            string appName = "notepad")
        {
            return new ActionModel
            {
                ActionType = actionType,
                AppName = appName,
                ExecutablePath = GetTestExecutablePath(appName),
                Arguments = string.Empty,
                WindowTitle = null,
                ForceOperation = false,
                IncludeChildProcesses = false,
                IncludeSimilarNames = false,
                TimeoutMs = 5000
            };
        }

        /// <summary>
        /// Creates a basic TriggerModel for testing
        /// </summary>
        public static TriggerModel CreateBasicTriggerModel(
            TriggerTypeEnum triggerType = TriggerTypeEnum.Keybind,
            string name = "TestTrigger")
        {
            return new TriggerModel
            {
                TriggerType = triggerType,
                Inactive = false,
                Key = triggerType == TriggerTypeEnum.Keybind ? System.Windows.Input.Key.F12 : null,
                Modifiers = triggerType == TriggerTypeEnum.Keybind ? System.Windows.Input.ModifierKeys.Control : null,
                Actions = [CreateBasicActionModel()]
            };
        }

        /// <summary>
        /// Creates test executable paths for common applications
        /// </summary>
        public static string GetTestExecutablePath(string appName)
        {
            return appName.ToLower() switch
            {
                "notepad" => "C:\\Windows\\System32\\notepad.exe",
                "CalculatorApp" or "CalculatorAppulator" => "C:\\Windows\\System32\\CalculatorApp.exe",
                "mspaint" or "paint" => "start mspaint",
                "cmd" or "command" => "C:\\Windows\\System32\\cmd.exe",
                _ => $"{appName}.exe"
            };
        }

        /// <summary>
        /// Creates a ProfileModel for testing
        /// </summary>
        public static ProfileModel CreateTestProfile(string profileName = "TestProfile")
        {
            return new ProfileModel
            {
                Name = profileName,
                Username = "TestUser",
                ScanInterval = 1000,
                AutoStart = false,
                FavoriteApps = new[] { "notepad", "CalculatorApp" },
                SelectedNav1Menu = "Apps",
                SelectedNav1List = "",
                Triggers = [CreateBasicTriggerModel()]
            };
        }

        /// <summary>
        /// Creates multiple ActionModels for bulk testing
        /// </summary>
        public static ActionModel[] CreateMultipleActionModels(int count = 3)
        {
            var actions = new List<ActionModel>();
            var actionTypes = Enum.GetValues<AppActionTypeEnum>();
            var appNames = new[] { "notepad", "CalculatorApp", "mspaint", "cmd", "explorer" };

            for (int i = 0; i < count; i++)
            {
                actions.Add(CreateBasicActionModel(
                    actionTypes[i % actionTypes.Length],
                    appNames[i % appNames.Length]));
            }

            return actions.ToArray();
        }

        /// <summary>
        /// Creates a ConditionModel for testing
        /// </summary>
        public static ConditionModel CreateBasicConditionModel()
        {
            return new ConditionModel
            {
                ConditionType = ConditionTypeEnum.ProcessRunning,
                ProcessName = "notepad",
                WindowTitle = null
            };
        }
    }
}