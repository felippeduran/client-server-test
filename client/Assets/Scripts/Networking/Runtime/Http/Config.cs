using System;

[Serializable]
public struct Config
{
    public string BaseUrl;
    public TimeSpan Timeout;
    public bool UseConnectivity;
    public ConnectivityHttpMessageHandler.ConnectivityConfig Connectivity;
}