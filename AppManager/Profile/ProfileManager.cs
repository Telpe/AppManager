using AppManager.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppManager.Profile
{
    public static class ProfileManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static readonly string ProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppManager");
        public static readonly string DefaultProfileFilename = "default";

        private static ProfileData _CurrentProfile;
        public static ProfileData CurrentProfile 
        { 
            get => _CurrentProfile ??= LoadProfile(); 
            private set => _CurrentProfile = value; 
        }

        private static string GetProfileFilepath(string profileName = null)
        {
            return Path.Combine(ProfilePath, $"{profileName ?? DefaultProfileFilename}.json");
        }

        public static ProfileData LoadProfile(string profileName = null)
        {
            ProfileData profile = null;
            try
            {
                string profileFile = GetProfileFilepath(profileName);
                if (File.Exists(profileFile))
                {
                    profile = JsonSerializer.Deserialize<ProfileData>(File.ReadAllText(profileFile), JsonOptions);
                    Debug.WriteLine($"Profile '{profile.Name}' loaded successfully");
                }
                else
                {
                    Debug.WriteLine($"Profile {profileName} not found.");
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

            // Major, Minor, Build, Revision
            profile.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            try
            {
                Directory.CreateDirectory(ProfilePath);
                File.WriteAllText(GetProfileFilepath(profile.Name), JsonSerializer.Serialize(profile, JsonOptions));
                
                Debug.WriteLine($"Profile {profile.Name} saved successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving profile: {ex.Message}");
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
            string profileFile = profileName != null 
                ? Path.Combine(ProfilePath, $"{profileName}.json")
                : GetProfileFilepath();
            return File.Exists(profileFile);
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