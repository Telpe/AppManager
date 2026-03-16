using AppManager.Core.Models;
using AppManager.Core.Utilities;
using System;
using System.IO;

namespace AppManager.Core.Conditions.FileExists
{
    public class FileExistsCondition(ConditionModel model) : BaseCondition(model) , IFileExistsCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.FileExists;
        public override string Description { get; set; } = "Checks if a specific file exists";
        public string? FilePath { get; set; } = model.FilePath;

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    LogConditionResult(false, "No file path specified");
                    return false;
                }

                bool exists = FileManager.FileExists(FilePath);
                //LogConditionResult(exists, $"File '{FilePath}' exists: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                LogConditionResult(false, $"Error checking file: '{FilePath}'\n{ex.Message}");
                return false;
            }
        }

        public override ConditionModel ToModel()
        {
            return ToConditionModel<IFileExistsCondition>();
        }
    }
}