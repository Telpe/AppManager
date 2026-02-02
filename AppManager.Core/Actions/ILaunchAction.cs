using System.ComponentModel.DataAnnotations;

namespace AppManager.Core.Actions
{
    public interface ILaunchAction
    {
        string? AppName { get; set; }
        string? ExecutablePath { get; set; }
        string? Arguments { get; set; }

        /// <summary>
        /// Time in milliseconds to wait before validating app launched.
        /// </summary>
        [Range(-1, int.MaxValue)]
        public int? TimeoutMs { get; set; }
    }
}