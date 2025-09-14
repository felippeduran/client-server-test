using System;

namespace Networking.Runtime.Http
{
    [Serializable]
    public struct Config
    {
        public string BaseUrl;
        public TimeSpan Timeout;
        public bool UseConnectivity;
        public ConnectivityHttpMessageHandler.ConnectivityConfig Connectivity;
    }
}