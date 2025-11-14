using AppManager.Utils;
using System.IO;

namespace AppManager.Conditions
{
    public class FileNotExistsCondition : BaseCondition
    {
        public override ConditionTypeEnum ConditionType => ConditionTypeEnum.FileNotExists;
        public override string Description => "Checks if a specific file does NOT exist";

        public override bool Execute()
        {
            try
            {
                var targetPath = Model?.FilePath ?? Model?.ExecutablePath;
                if (string.IsNullOrEmpty(targetPath))
                {
                    LogConditionResult(true, "No file path specified - treating as 'file not exists'");
                    return true;
                }

                bool notExists = !FileManager.FileExists(targetPath);
                LogConditionResult(notExists, $"File '{targetPath}' does not exist: {notExists}");
                return notExists;
            }
            catch (System.Exception ex)
            {
                LogConditionResult(false, $"Error checking file: {ex.Message}");
                return false;
            }
        }
    }
}