global using AppManager.Core.Utilities;
using AppManager.Config.EditorControls;
using AppManager.Config.Interfaces;
using AppManager.Config.Utilities;
using AppManager.Core.Actions;
using AppManager.Core.Keybinds;
using AppManager.Core.Models;
using AppManager.Core.Triggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace AppManager.Config
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private static GlobalKeyboardHook? GlobalKeyboardHookValue;

        private static Dictionary<string, string> UnsavedPages = new();
        private MainWindow? MainWindowValue = null;

        private System.Timers.Timer CheckIfAppsRunningValue = new();
        public System.Timers.Timer CheckIfAppsRunning { get { return CheckIfAppsRunningValue; } }

        public static readonly AppManager.Core.Version Version;

        public const string CommandPrefix = "/";
        public const string EditorCommand = CommandPrefix + "edit";
        public const string ItemIdCommand = CommandPrefix + "id";

        static App()
        {
            // Load version from version.json file
            Version = FileManager.LoadVersion();
            Log.WriteLine($"AppManager Config Version: {Version}");
        }

        public App()
        {
            InitializeComponent();

            //string profileToLoad = !string.IsNullOrEmpty(SettingsManager.CurrentSettings.LastUsedProfileName) 
            //    ? SettingsManager.CurrentSettings.LastUsedProfileName 
            //    : ProfileManager.DefaultProfileFilename;

            //// Load the specific profile
            //if (ProfileManager.ProfileExist(profileToLoad))
            //{
            //    ProfileManager.LoadAndSetProfile(profileToLoad);
            //    Log.WriteLine($"Loaded last used profile: {profileToLoad}");
            //}
            //else
            //{
            //    _ = ProfileManager.CurrentProfile;
            //    Log.WriteLine($"Profile '{profileToLoad}' not found, loaded default profile instead");
            //}

            //CheckIfAppsRunningValue.Interval = ProfileManager.CurrentProfile.ScanInterval;
            //CheckIfAppsRunningValue.AutoReset = true;
            //CheckIfAppsRunningValue.Elapsed += new ElapsedEventHandler(CheckRunningHandler);
            //CheckIfAppsRunningValue.Start();
        }

        public void App_Startup(object sender, StartupEventArgs e)
        {
            string? editor = null;
            string? itemId = null;

            

            for (int i = 0; i < e.Args.Length; i++)
            {
                int argsCount = 0;

                switch (e.Args[i].ToLower())
                {
                    case EditorCommand:

                        argsCount = CountNonCommandArgs(e.Args, i + 1);
                        editor = string.Join(" ", e.Args, i + 1, argsCount);
                        Log.WriteLine($"Editor: {editor}");
                        break;

                    case ItemIdCommand:

                        argsCount = CountNonCommandArgs(e.Args, i + 1);
                        itemId = string.Join(" ", e.Args, i + 1, argsCount);
                        Log.WriteLine($"Item: {itemId}");
                        break;
                }

                i += argsCount;
            }

            //if (Shared.ShouldITerminateBringingOtherToFront())
            //{
            //    Application.Current.Shutdown();
            //    return;
            //}

            MainWindowValue = new();
            
            if (!string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(editor))
            {
                MainWindowValue.ShowOverlay(GetEditor(editor, itemId));
            }
            else if(!string.IsNullOrEmpty(editor))
            {
                Log.WriteLine("Editor specified without itemId, showing main window with no overlay");
            }

            MainWindowValue.Show();

            SetCoreRunning();
        }

        private IInputEditControl GetEditor(string editorName, string itemId)
        {
            editorName = editorName.ToLowerInvariant();
            editorName = string.Join(char.ToUpperInvariant(editorName[0]), editorName[1..^0], "EditorControl");

            return editorName switch
            {
                nameof(ActionEditorControl) => EditAction(itemId),
                nameof(ConditionEditorControl) => EditCondition(itemId),
                nameof(TriggerEditorControl) => EditTrigger(itemId),
                _ => throw new ArgumentException($"Unknown editor: {editorName}"),
            };
        }

        private IInputEditControl EditAction(string itemId)
        {
            ActionModel? actionModel = null;

            foreach (TriggerModel trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (actionModel is not null) { break; }

                foreach (ActionModel action in trigger.Actions ?? [])
                {
                    if (action.Id == itemId)
                    {
                        actionModel = action.Clone();
                        break;
                    }
                }
            }

            foreach (AppManagedModel app in ProfileManager.CurrentProfile.Apps)
            {
                if (actionModel is not null) { break; }

                if (itemId == app.Click?.Id)
                {
                    actionModel = app.Click.Clone();
                }

                if (itemId == app.DoubleClick?.Id)
                {
                    actionModel = app.DoubleClick.Clone();
                }

                foreach (ActionModel action in app.Actions ?? [])
                {
                    if (action.Id == itemId)
                    {
                        actionModel = action.Clone();
                        break;
                    }
                }
            }

            if (actionModel is null)
            {
                throw new ArgumentException($"Action with id '{itemId}' not found");
            }

            ActionEditorControl editorControl = new(actionModel);

            editorControl.OnCancel += (s, ev) => { 
                MainWindowValue?.HideOverlay(); 
                MainWindowValue?.ShowOverlay(EditAction(itemId)); 
            };
            editorControl.OnSave += (s, ev) => {
                UpdateActionInProfile(actionModel);
                MainWindowValue?.HideOverlay();
                ProfileManager.SaveProfile();
                MainWindowValue?.Close(true); 
            };
            //editorControl.OnEdited += (s, ev) => {
                
            //};

            return editorControl;
        }

        private void UpdateActionInProfile(ActionModel editedAction)
        {
            foreach (TriggerModel trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (trigger.Actions is null) { continue; }
                for (int i = 0; i < trigger.Actions.Length; i++)
                {
                    if (trigger.Actions[i].Id == editedAction.Id)
                    {
                        trigger.Actions[i] = editedAction;
                        return;
                    }
                }
            }

            foreach (AppManagedModel app in ProfileManager.CurrentProfile.Apps)
            {
                if (app.Click?.Id == editedAction.Id)
                {
                    app.Click = editedAction;
                    return;
                }
                if (app.DoubleClick?.Id == editedAction.Id)
                {
                    app.DoubleClick = editedAction;
                    return;
                }

                if (app.Actions is null) { continue; }

                for (int i = 0; i < app.Actions.Length; i++)
                {
                    if (app.Actions[i].Id == editedAction.Id)
                    {
                        app.Actions[i] = editedAction;
                        return;
                    }
                }
            }

            throw new ArgumentException($"Edited action with id '{editedAction.Id}' not found in profile");
        }

        private IInputEditControl EditCondition(string itemId)
        {
            ConditionModel? conditionModel = null;

            foreach (TriggerModel trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (conditionModel is not null) { break; }

                if (trigger.TryGetConditionFromId(itemId, out conditionModel)) { break; }

                foreach (ActionModel action in trigger.Actions ?? [])
                {
                    if (action.TryGetConditionFromId(itemId, out conditionModel)) { break; }
                }
            }

            foreach (AppManagedModel app in ProfileManager.CurrentProfile.Apps)
            {
                if (conditionModel is not null) { break; }

                if (app.Click?.TryGetConditionFromId(itemId, out conditionModel) ?? false) { break; }

                if (app.DoubleClick?.TryGetConditionFromId(itemId, out conditionModel) ?? false) { break; }
                
                foreach (ActionModel action in app.Actions ?? [])
                {
                    if (action.TryGetConditionFromId(itemId, out conditionModel)) { break; }
                }
            }

            conditionModel = conditionModel?.Clone() ?? throw new ArgumentException($"Condition with id '{itemId}' not found");

            ConditionEditorControl editorControl = new(conditionModel);

            editorControl.OnCancel += (s, ev) => {
                MainWindowValue?.HideOverlay();
                MainWindowValue?.ShowOverlay(EditCondition(itemId));
            };
            editorControl.OnSave += (s, ev) => {
                UpdateConditionInProfile(conditionModel);
                MainWindowValue?.HideOverlay();
                ProfileManager.SaveProfile();
                MainWindowValue?.Close(true);
            };
            //editorControl.OnEdited += (s, ev) => {

            //};

            return editorControl;
        }

        private void UpdateConditionInProfile(ConditionModel editedCondition)
        {
            foreach (TriggerModel trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (trigger.TryUpdateCondition(editedCondition)) { return; }

                foreach (ActionModel action in trigger.Actions ?? [])
                {
                    if (action.TryUpdateCondition(editedCondition)) { return; }
                }
            }
            foreach (AppManagedModel app in ProfileManager.CurrentProfile.Apps)
            {
                if (app.Click?.TryUpdateCondition(editedCondition) is true) { return; }

                if (app.DoubleClick?.TryUpdateCondition(editedCondition) is true) { return; }

                foreach (ActionModel action in app.Actions ?? [])
                {
                    if (action.TryUpdateCondition(editedCondition)) { return; }
                }
            }
            throw new ArgumentException($"Edited condition with id '{editedCondition.Id}' not found in profile");
        }

        private IInputEditControl EditTrigger(string itemId)
        {
            TriggerModel? triggerModel = null;

            foreach (TriggerModel trigger in ProfileManager.CurrentProfile.Triggers)
            {
                if (triggerModel is not null) { break; }

                if (trigger.Id == itemId)
                {
                    triggerModel = trigger.Clone();
                    break;
                }
            }

            if (triggerModel is null)
            {
                throw new ArgumentException($"Trigger with id '{itemId}' not found");
            }

            TriggerEditorControl editorControl = new(triggerModel);

            editorControl.OnCancel += (s, ev) => {
                MainWindowValue?.HideOverlay();
                MainWindowValue?.ShowOverlay(EditTrigger(itemId));
            };
            editorControl.OnSave += (s, ev) => {
                UpdateTriggerInProfile(triggerModel);
                MainWindowValue?.HideOverlay();
                ProfileManager.SaveProfile();
                MainWindowValue?.Close(true);
            };
            //editorControl.OnEdited += (s, ev) => {

            //};

            return editorControl;
        }

        private void UpdateTriggerInProfile(TriggerModel editedTrigger)
        {
            for (int i = 0; i < ProfileManager.CurrentProfile.Triggers.Length; i++)
            {
                if (ProfileManager.CurrentProfile.Triggers[i].Id == editedTrigger.Id)
                {
                    ProfileManager.CurrentProfile.Triggers[i] = editedTrigger;
                    return;
                }
            }
            throw new ArgumentException($"Edited trigger with id '{editedTrigger.Id}' not found in profile");
        }

        private IInputEditControl EditApp(string itemId)
        {
            throw new NotImplementedException();
        }

        private int CountNonCommandArgs(string[] args, int startIndex)
        {
            int i = FindNextCommandArgIndex(args, startIndex);

            return -1 == i ? args.Length - startIndex : i - startIndex;
        }

        private int FindNextCommandArgIndex(string[] args, int startIndex)
        {
            for (; startIndex < args.Length; startIndex++)
            {
                if (args[startIndex].StartsWith(CommandPrefix)) { return startIndex; }
            }
            return -1;
        }

        protected void SetCoreRunning()
        {
            if (!ProcessManager.IsProcessRunning("AppManager.Core"))
            {
                Log.WriteLine("AppManager.Core not running, launching it");

                ActionFactory.CreateAction(new ActionModel
                {
                    ActionType = ActionTypeEnum.Launch,
                    AppName = "AppManager.Core"
                }).Execute();
                
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.WriteLine("Settings exit");
            Log.Dispose();
            base.OnExit(e);
        }

       
        
    }

}
