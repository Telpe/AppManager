using AppManager.Core.Utils;
using System.IO;

namespace AppManager.Core.Conditions
{
    public class FileExistsCondition : BaseCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.FileExists;
        public override string Description => "Checks if a specific file exists";

        public override bool Execute()
        {
            try
            {
                var targetPath = Model?.FilePath ?? Model?.ExecutablePath;
                if (string.IsNullOrEmpty(targetPath))
                {
                    LogConditionResult(false, "No file path specified");
                    return false;
                }

                bool exists = FileManager.FileExists(targetPath);
                LogConditionResult(exists, $"File '{targetPath}' exists: {exists}");
                return exists;
            }
            catch (System.Exception ex)
            {
                LogConditionResult(false, $"Error checking file: {ex.Message}");
                return false;
            }
        }
    }
}