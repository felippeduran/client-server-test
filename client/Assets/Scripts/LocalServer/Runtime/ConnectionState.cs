using System;
using Core.Runtime;

namespace LocalServer.Runtime
{
    [Serializable]
    public class ConnectionState
    {
        public string AccountId;
        public SessionState SessionState;
    }
}