using System;

namespace Networking.Runtime.Http
{
    [Serializable]
    public struct Config
    {
        public string BaseUrl;
        public TimeSpan Timeout;
        public bool UseThrottling;
        public ThrottlingHttpMessageHandler.ThrottlingConfig Throttling;
    }
}