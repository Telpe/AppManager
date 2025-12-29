using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace AppManager.Core.Utils 
{ 
    public static class ProfileManager
    {
        public static readonly string DefaultProfileFilename = "default";

        private static ProfileModel? CurrentProfileValue;
        public static ProfileModel CurrentProfile 
        { 
            get => CurrentProfileValue ??= LoadLastUsedProfile();
        }

        public static ProfileModel LoadLastUsedProfile()
        {
            string profileName = SettingsManager.CurrentSettings.LastUsedProfileName ?? DefaultProfileFilename;
            return LoadProfile(profileName);
        }

        public static ProfileModel LoadProfile(string profileName)
        {   
            try
            {
                ProfileModel profile;

                string profileFile = FileManager.GetProfilePath(profileName);
                profile = FileManager.LoadJsonFile<ProfileModel>(profileFile);

                Debug.WriteLine($"Profile '{profile.Name}' loaded successfully");

                return profile;
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Profile {profileName} may be broken.\nLoading caused error:\n{e.Message}\n{e.StackTrace}");
                Debug.WriteLine($"Loading default profile instead");

                if (DefaultProfileFilename == profileName) { throw new Exception($"Error loading default profile", e); }

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
                    Debug.WriteLine($"Default profile '{profile.Name}' created");
                    return false;
                }

                CurrentProfileValue = profile;

                // Update the settings with the new profile name
                UpdateLastUsedProfileInSettings(CurrentProfileValue.Name);

                Debug.WriteLine($"Successfully switched to profile: {CurrentProfileValue.Name}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading and setting profile '{profileName}': {ex.Message}");
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
                Debug.WriteLine($"Updated LastUsedProfileName to: {SettingsManager.CurrentSettings.LastUsedProfileName}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating LastUsedProfileName in settings: {ex.Message}");
            }
        }

        public static void SaveProfile(ProfileModel? profile = null)
        {
            if (profile == null)
            {
                profile = CurrentProfileValue;

                if (profile == null)
                {
                    Debug.WriteLine("No profile to save");
                    return;
                }
            }

            // Update version info
            profile.Version = FileManager.LoadVersion();

            string profileFile = FileManager.GetProfilePath(profile.Name);
            bool success = FileManager.SaveJsonFile(profile, profileFile);
            
            if (success)
            {
                Debug.WriteLine($"Profile {profile.Name} saved successfully");
            }
            else
            {
                Debug.WriteLine($"Failed to save profile {profile.Name}");
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
            
            Debug.WriteLine($"New profile {profile.Name} created for user: {profile.Username}");

            return profile;
        }

        public static bool ProfileExist(string? profileName = null)
        {
            return FileManager.FileExists(FileManager.GetProfilePath(profileName ?? DefaultProfileFilename));
        }

        public static void ClearCache() { CurrentProfileValue = null; }

        public static void ResetDefaultProfile()
        {
            CurrentProfileValue = new ProfileModel
            {
                Name = DefaultProfileFilename,
                Username = Environment.UserName
            };
            SaveProfile(CurrentProfileValue);
            Debug.WriteLine("Profile reset to defaults");
        }
    }
}