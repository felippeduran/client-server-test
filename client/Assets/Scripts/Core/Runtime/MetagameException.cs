using System;

namespace Core.Runtime
{
    public class MetagameException : Exception
    {
        public MetagameException(string description) : base(description) { }
    }
}
