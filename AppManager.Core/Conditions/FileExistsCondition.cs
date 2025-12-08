using AppManager.Core.Models;
using AppManager.Core.Utils;
using System;
using System.IO;

namespace AppManager.Core.Conditions
{
    public class FileExistsCondition(ConditionModel model) : BaseCondition(model) , IFileExistsCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.FileExists;
        public override string Description { get; set; } = "Checks if a specific file exists";
        public string? ExecutablePath { get; set; } = model.ExecutablePath;

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrEmpty(ExecutablePath))
                {
                    LogConditionResult(false, "No file path specified");
                    return false;
                }

                bool exists = FileManager.FileExists(ExecutablePath);
                //LogConditionResult(exists, $"File '{FilePath}' exists: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                LogConditionResult(false, $"Error checking file: '{ExecutablePath}'\n{ex.Message}");
                return false;
            }
        }

        public override ConditionModel ToModel()
        {
            return new ConditionModel
            {
                ConditionType = ConditionTypeEnum.FileExists,
                IsNot = IsNot,
                ExecutablePath = ExecutablePath
            };
        }
    }
}