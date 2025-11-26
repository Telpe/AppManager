using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AppManager.Core.Actions;
using AppManager.Core.Models;

namespace AppManager.Core.Triggers.Examples
{
    internal static class TriggerUsageExamples
    {
        public static async Task<TriggerManager> SetupExampleTriggersAsync()
        {
            var actionManager = new ActionManager();
            var triggerManager = new TriggerManager(actionManager);

            // Example 1: Global shortcut (Ctrl+Shift+N) to launch Notepad
            var shortcutModel = new TriggerModel 
            { 
                TriggerType = TriggerTypeEnum.Shortcut,
                Key = Key.N,
                Modifiers = ModifierKeys.Control | ModifierKeys.Shift,
                IsActive = true
            };
            var shortcutTrigger = triggerManager.CreateTrigger(shortcutModel);
            shortcutTrigger.Name = "LaunchNotepad";
            triggerManager.RegisterTrigger(shortcutTrigger);

            // Example 2: Monitor for Chrome launch to auto-focus it
            var chromeLaunchModel = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.AppLaunch,
                ProcessName = "chrome",
                IsActive = true
            };
            var chromeLaunchTrigger = triggerManager.CreateTrigger(chromeLaunchModel);
            chromeLaunchTrigger.Name = "AutoFocusChrome";
            triggerManager.RegisterTrigger(chromeLaunchTrigger);

            // Example 3: Monitor for Steam close to launch Epic Games
            var steamCloseModel = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.AppClose,
                ProcessName = "Steam",
                IsActive = true
            };
            var steamCloseTrigger = triggerManager.CreateTrigger(steamCloseModel);
            steamCloseTrigger.Name = "SteamToEpic";
            triggerManager.RegisterTrigger(steamCloseTrigger);

            // Example 4: Monitor system unlock to launch Discord
            var unlockModel = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.SystemEvent,
                EventName = "Unlocked",
                IsActive = true
            };
            var unlockTrigger = triggerManager.CreateTrigger(unlockModel);
            unlockTrigger.Name = "UnlockToDiscord";
            triggerManager.RegisterTrigger(unlockTrigger);

            // Example 5: Network port trigger for remote commands
            var networkModel = new TriggerModel
            {
                TriggerType = TriggerTypeEnum.NetworkPort,
                Port = 8080,
                IPAddress = "127.0.0.1",
                IsActive = true
            };
            var networkTrigger = triggerManager.CreateTrigger(networkModel);
            networkTrigger.Name = "RemoteCommands";
            triggerManager.RegisterTrigger(networkTrigger);

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

            triggerManager.StopAllTriggersAsync();
            triggerManager.Dispose();
            
            Console.WriteLine("Trigger system stopped.");
        }
    }
}