public interface ILogger
{
    void Log(string message);
    void LogWarning(string message);
    void LogError(string message);
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
}

public class NullLogger : ILogger
{
    public void Log(string message) { }
    public void LogWarning(string message) { }
    public void LogError(string message) { }
}
