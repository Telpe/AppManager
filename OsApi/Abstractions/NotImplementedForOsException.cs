using System;
using System.Collections.Generic;
using System.Text;

namespace AppManager.OsApi.Abstractions
{
    public class NotImplementedForOsException : Exception
    {
        public NotImplementedForOsException()
            : base("This method has not been implemented or tested for the current operating system.")
        {
        }

        public NotImplementedForOsException(string message)
            : base(message)
        {
        }

        public NotImplementedForOsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public NotImplementedForOsException(Exception innerException)
            : base("This method has not been implemented or tested for the current operating system. See inner exception for details.", innerException)
        {
        }

        public NotImplementedForOsException(string methodName, string osName)
            : base($"Method '{methodName}' has not been implemented or tested for {osName}.")
        {
        }

        public NotImplementedForOsException(string methodName, string osName, Exception innerException)
            : base($"Method '{methodName}' has not been implemented or tested for {osName}. See inner exception for details.", innerException)
        {
        }
    }
}
