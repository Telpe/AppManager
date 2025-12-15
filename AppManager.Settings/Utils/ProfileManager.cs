using AppManager.Core.Models;
using AppManager.Core.Utils;
using AppManager;
using System;
using System.Diagnostics;

namespace AppManager.Settings.Utils 
{ 
    public static class ProfileManager
    {
        public static readonly string DefaultProfileFilename = "default";

        private static ProfileModel? _CurrentProfile;
        public static ProfileModel CurrentProfile 
        { 
            get => _CurrentProfile ??= LoadProfile(); 
            private set => _CurrentProfile = value; 
        }

        public static ProfileModel LoadProfile(string? profileName = null)
        {
            ProfileModel profile;
            
            string profileFile = FileManager.GetProfilePath(profileName ?? DefaultProfileFilename);
            profile = FileManager.LoadJsonFile<ProfileModel>(profileFile);
                
            if (profile.Name != null)
            {
                Debug.WriteLine($"Profile '{profile.Name}' loaded successfully");
            }
            else
            {
                Debug.WriteLine($"Profile {profileName} may be broken.");
            }

            return profile;
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

                _CurrentProfile = profile;

                // Update the settings with the new profile name
                UpdateLastUsedProfileInSettings(_CurrentProfile.Name);

                Debug.WriteLine($"Successfully switched to profile: {_CurrentProfile.Name}");
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

        public static void SaveProfile(ProfileModel profile = null)
        {
            if (profile == null)
            {
                profile = _CurrentProfile;

                if (profile == null)
                {
                    Debug.WriteLine("No profile to save");
                    return;
                }
            }

            // Update version info
            profile.Version = App.Version;

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

        public static void ResetProfile()
        {
            _CurrentProfile = new ProfileModel
            {
                Name = DefaultProfileFilename,
                Username = Environment.UserName
            };
            SaveProfile(_CurrentProfile);
            Debug.WriteLine("Profile reset to defaults");
        }
    }
}