using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AppManager.Core.Utilities 
{ 
    public static class ProfileManager
    {
        public static readonly string DefaultProfileFilename = "default";

        public static ProfileModel NewDefaultProfile
        {
            get => new()
            {
                Name = DefaultProfileFilename,
                Username = Environment.UserName
            };
        }

        private static ProfileModel? CurrentProfileValue;
        public static ProfileModel CurrentProfile 
        { 
            get => CurrentProfileValue ??= LoadLastUsedProfile();
        }

        public static ProfileModel LoadLastUsedProfile()
        {
            return LoadProfile(SettingsManager.CurrentSettings.LastUsedProfileName);
        }

        public static ProfileModel LoadProfile(string profileName)
        {   
            try
            {
                ProfileModel profile;

                string profileFile = FileManager.GetProfilePath(profileName);
                profile = FileManager.LoadJsonFile<ProfileModel>(profileFile);

                Log.WriteLine($"Profile '{profile.Name}' loaded successfully");

                return profile;
            }
            catch(Exception e)
            {
                Log.WriteLine($"Profile '{profileName}' may be broken. Loading caused error:\n{e.Message}\n{e.StackTrace}");

                if (DefaultProfileFilename == profileName) 
                {
                    Log.WriteLine($"Creating new default profile instead. Not saved.");
                    return NewDefaultProfile;
                }

                Log.WriteLine($"Loading default profile instead");
                return LoadProfile(DefaultProfileFilename);
            }
        }

        /// <summary>
        /// Loads a specific profile and updates the current profile and settings
        /// </summary>
        /// <param name="profileName">Name of the profile to load</param>
        /// <returns>True if profile was loaded successfully</returns>
        public static bool LoadAndSetProfile(string profileName)
        {
            try
            {
                var profile = LoadProfile(profileName);
                if (null == profile)
                {
                    profile = CreateNewProfile(profileName);
                    Log.WriteLine($"Default profile '{profile.Name}' created");
                    return false;
                }

                CurrentProfileValue = profile;

                // Update the settings with the new profile name
                UpdateLastUsedProfileInSettings(CurrentProfileValue.Name);

                Log.WriteLine($"Successfully switched to profile: {CurrentProfileValue.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error loading and setting profile '{profileName}': {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// Updates the LastUsedProfileName in settings and saves it
        /// </summary>
        /// <param name="profileName">The profile name to save as last used</param>
        private static void UpdateLastUsedProfileInSettings(string profileName)
        {
            try
            {
                SettingsManager.CurrentSettings.LastUsedProfileName = profileName ?? DefaultProfileFilename;
                Log.WriteLine($"Updated LastUsedProfileName to: {SettingsManager.CurrentSettings.LastUsedProfileName}");
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error updating LastUsedProfileName in settings: {ex.Message}");
            }
        }

        public static void SaveProfile(ProfileModel? profile = null)
        {
            profile ??= CurrentProfileValue;

            if (null == profile)
            {
                Log.WriteLine("No profile to save");
                return;
            }

            try 
            {
                profile.Version = FileManager.LoadVersion();

                string profileFile = FileManager.GetProfilePath(profile.Name);

                FileManager.SaveJsonFile(profile, profileFile);

                Log.WriteLine($"Profile {profile.Name} saved successfully");
            }
            catch(Exception ex)
            {
                Log.WriteLine($"Error saving profile '{profile.Name}': {ex.Message}");
                return;
            }
        }

        public static ProfileModel CreateNewProfile(string? profileName = null)
        {
            ProfileModel profile = new ProfileModel
            {
                Name = profileName ?? DefaultProfileFilename,
                Username = Environment.UserName,
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Core.Version() { Exspansion = 0, Patch = 0, Hotfix = 0, Work = 1 }
            };
            
            Log.WriteLine($"New profile {profile.Name} created for user: {profile.Username}");

            return profile;
        }

        public static bool ProfileExist(string profileName)
        {
            return FileManager.FileExists(FileManager.GetProfilePath(profileName));
        }

        public static void ClearCache() { CurrentProfileValue = null; }

        public static void ResetDefaultProfile()
        {
            SaveProfile(NewDefaultProfile);
            Log.WriteLine("Profile default resat");
        }
    }
}