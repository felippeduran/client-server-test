using System;

namespace Utilities.Runtime.Logging
{
    public interface ILogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogException(Exception exception);
    }

    public static class Logger
    {
        public static ILogger Instance { get; set; }

        static Logger()
        {
            Instance = new NullLogger();
        }

        public static void Log(string message)
        {
            Instance.Log(message);
        }

        public static void LogWarning(string message)
        {
            Instance.LogWarning(message);
        }

        public static void LogError(string message)
        {
            Instance.LogError(message);
        }

        public static void LogException(Exception exception)
        {
            Instance.LogException(exception);
        }
    }

    public class NullLogger : ILogger
    {
        public void Log(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogException(Exception exception) { }
    }

}