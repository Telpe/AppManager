using AppManager.Core.Actions;
using AppManager.Core.Models;
using AppManager.Core.Utilities;
using AppManager.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AppManager.Core.Triggers;
using AppManager.OsApi.Models;

namespace AppManager.Tests.Create
{
    [TestClass]
    public class ProfileFile
    {
        [TestMethod]
        [TestCategory("Create")]
        [TestCategory("Profile")]
        public async Task ManyTriggersProfile()
        {
            // Arrange
            const string profileName = "manyTriggers";
            
            // Act - Create and save the profile
            var profile = ProfileManager.CreateNewProfile(profileName);

            // Create 30 triggers with specified tagging structure
            var triggers = new List<TriggerModel>();
            
            // 10 triggers with AppName:Discord, 4 of them have Work:On, 3 have Work:Off, 3 without Work
            for (int i = 1; i <= 10; i++)
            {
                var trigger = new TriggerModel
                {
                    TriggerType = TriggerTypeEnum.Keybind,
                    Id = $"discord_trigger_{i}",
                    Keybind = new HotkeyModel(ModifierKey.Control, Key.F1 + (i - 1)),
                    Actions = new[] { TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Launch, "Discord") },
                    Tags = new Dictionary<string, string> { { "AppName", "Discord" } }
                };
                
                // Add Work category: 4 with 'On', 3 with 'Off', 3 without Work
                if (i <= 4)
                {
                    trigger.Tags.Add("Work", "On");
                }
                else if (i <= 7)
                {
                    trigger.Tags.Add("Work", "Off");
                }
                
                triggers.Add(trigger);
            }
            
            // 6 triggers with AppName:Steam, 3 of them have Work:On, 3 have Work:Off
            for (int i = 1; i <= 6; i++)
            {
                var trigger = new TriggerModel
                {
                    TriggerType = TriggerTypeEnum.AppLaunch,
                    Id = $"steam_trigger_{i}",
                    ProcessName = "steam",
                    Actions = new[] { TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Launch, "Steam") },
                    Tags = new Dictionary<string, string> { { "AppName", "Steam" } }
                };
                
                // Add Work category: 3 with 'On', 3 with 'Off'
                if (i <= 3)
                {
                    trigger.Tags.Add("Work", "On");
                }
                else
                {
                    trigger.Tags.Add("Work", "Off");
                }
                
                triggers.Add(trigger);
            }
            
            // 4 triggers with AppName:notepad, none have Work category
            for (int i = 1; i <= 4; i++)
            {
                var trigger = new TriggerModel
                {
                    TriggerType = TriggerTypeEnum.AppClose,
                    Id = $"notepad_trigger_{i}",
                    ProcessName = "notepad",
                    Actions = new[] { TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Close, "notepad") },
                    Tags = new Dictionary<string, string> { { "AppName", "notepad" } }
                };
                
                triggers.Add(trigger);
            }
            
            // 10 triggers without tags
            for (int i = 1; i <= 10; i++)
            {
                var trigger = new TriggerModel
                {
                    TriggerType = TriggerTypeEnum.Button,
                    Id = $"untagged_trigger_{i}",
                    Actions = new[] { TestDataBuilder.CreateBasicActionModel(ActionTypeEnum.Launch, "cmd") }
                };
                
                triggers.Add(trigger);
            }
            
            profile.Triggers = triggers.ToArray();

            ProfileManager.SaveProfile(profile);
            
            // Assert - Check that the file exists
            string expectedFilePath = FileManager.GetProfilePath(profileName);
            Assert.IsTrue(File.Exists(expectedFilePath), $"Profile file '{expectedFilePath}' should exist after saving");
            
        }
    }
}
