using System;

namespace Networking.Runtime
{
    [Serializable]
    public class Error
    {
        public string Message { get; init; }

        public static bool operator ==(Error left, Error right) => Equals(left, right);
        public static bool operator !=(Error left, Error right) => !Equals(left, right);

        protected bool Equals(Error other)
        {
            return Message == other.Message;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Error)obj);
        }

        public override int GetHashCode()
        {
            return (Message != null ? Message.GetHashCode() : 0);
        }
    }
}