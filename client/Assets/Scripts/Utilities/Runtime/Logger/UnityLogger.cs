using System;
using UnityEngine;

// Ideally, this should be moved to a separate assembly that depends on ILogger' assembly to avoid transitive dependencies on UnityEngine.
public class UnityLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void LogError(string message)
    {
        Debug.LogError(message);
    }

    public void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }
}