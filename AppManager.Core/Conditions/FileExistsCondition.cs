using AppManager.Core.Utils;
using System.IO;

namespace AppManager.Core.Conditions
{
    public class FileExistsCondition : BaseCondition
    {
        public override string Description => "Checks if a specific file exists";

        public override bool Execute()
        {
            if (null == Model) { throw new ArgumentNullException("Condition Model can not be null."); }
            try
            {
                var targetPath = Model.FilePath ?? Model.ExecutablePath;
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