using AppManager.Settings;
using AppManager.Utils;
using System;
using System.Diagnostics;

namespace AppManager.Profile
{
    public static class ProfileManager
    {
        public static readonly string DefaultProfileFilename = "default";

        private static ProfileData _CurrentProfile;
        public static ProfileData CurrentProfile 
        { 
            get => _CurrentProfile ??= LoadProfile(); 
            private set => _CurrentProfile = value; 
        }

        public static ProfileData LoadProfile(string profileName = null)
        {
            ProfileData profile = null;
            try
            {
                string profileFile = FileManager.GetProfilePath(profileName ?? DefaultProfileFilename);
                profile = FileManager.LoadJsonFile<ProfileData>(profileFile);
                
                if (profile.Name != null) // Check if file existed and was loaded
                {
                    Debug.WriteLine($"Profile '{profile.Name}' loaded successfully");
                }
                else
                {
                    Debug.WriteLine($"Profile {profileName} not found.");
                    profile = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading profile: {ex.Message}");
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

        public static void SaveProfile(ProfileData profile = null)
        {
            if (profile == null)
            {
                profile = _CurrentProfile;
            }

            if(profile == null)
            {
                Debug.WriteLine("No profile to save");
                return;
            }

            // Update version info
            profile.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

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

        public static ProfileData CreateNewProfile(string profileName = null)
        {
            ProfileData profile = new ProfileData
            {
                Name = profileName ?? DefaultProfileFilename,
                Username = Environment.UserName,
                Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            };
            
            Debug.WriteLine($"New profile {profile.Name} created for user: {profile.Username}");

            return profile;
        }

        public static bool ProfileExists(string profileName = null)
        {
            string profileFile = FileManager.GetProfilePath(profileName ?? DefaultProfileFilename);
            return FileManager.FileExists(profileFile);
        }

        public static void ResetProfile()
        {
            _CurrentProfile = new ProfileData
            {
                Name = DefaultProfileFilename,
                Username = Environment.UserName
            };
            SaveProfile(_CurrentProfile);
            Debug.WriteLine("Profile reset to defaults");
        }
    }
}