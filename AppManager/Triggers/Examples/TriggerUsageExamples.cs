using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AppManager.Actions;

namespace AppManager.Triggers.Examples
{
    internal static class TriggerUsageExamples
    {
        public static async Task<TriggerManager> SetupExampleTriggersAsync()
        {
            var actionManager = new ActionManager();
            var triggerManager = new TriggerManager(actionManager);

            // Example 1: Global shortcut (Ctrl+Shift+N) to launch Notepad
            var shortcutTrigger = triggerManager.CreateTrigger(TriggerTypeEnum.Shortcut, "LaunchNotepad");
            var shortcutParams = new TriggerModel
            {
                Key = Key.N,
                Modifiers = ModifierKeys.Control | ModifierKeys.Shift
            };
            await triggerManager.RegisterTriggerAsync(shortcutTrigger, shortcutParams);

            // Example 2: Monitor for Chrome launch to auto-focus it
            var chromeLaunchTrigger = triggerManager.CreateTrigger(TriggerTypeEnum.AppLaunch, "AutoFocusChrome");
            var chromeParams = new TriggerModel
            {
                ProcessName = "chrome"
            };
            await triggerManager.RegisterTriggerAsync(chromeLaunchTrigger, chromeParams);

            // Example 3: Monitor for Steam close to launch Epic Games
            var steamCloseTrigger = triggerManager.CreateTrigger(TriggerTypeEnum.AppClose, "SteamToEpic");
            var steamParams = new TriggerModel
            {
                ProcessName = "Steam"
            };
            await triggerManager.RegisterTriggerAsync(steamCloseTrigger, steamParams);

            // Example 4: Monitor system unlock to launch Discord
            var unlockTrigger = triggerManager.CreateTrigger(TriggerTypeEnum.SystemEvent, "UnlockToDiscord");
            var unlockParams = new TriggerModel
            {
                EventName = "Unlocked"
            };
            await triggerManager.RegisterTriggerAsync(unlockTrigger, unlockParams);

            // Example 5: Network port trigger for remote commands
            var networkTrigger = triggerManager.CreateTrigger(TriggerTypeEnum.NetworkPort, "RemoteCommands");
            var networkParams = new TriggerModel
            {
                Port = 8080,
                IPAddress = "127.0.0.1"
            };
            await triggerManager.RegisterTriggerAsync(networkTrigger, networkParams);

            // Subscribe to trigger events for logging
            triggerManager.TriggerActivated += (sender, args) =>
            {
                Console.WriteLine($"[{DateTime.Now}] Trigger '{args.TriggerName}' activated: {args.ActionToExecute} on {args.TargetAppName}");
            };

            return triggerManager;
        }

        public static async Task RunExampleAsync()
        {
            var triggerManager = await SetupExampleTriggersAsync();

            Console.WriteLine("Trigger system started. Press any key to stop...");
            Console.ReadKey();

            await triggerManager.StopAllTriggersAsync();
            triggerManager.Dispose();
            
            Console.WriteLine("Trigger system stopped.");
        }
    }
}