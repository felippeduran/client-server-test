using System;

namespace Utilities.Runtime.Clock
{
    public class OffsetClock : IClock
    {
        readonly TimeSpan timeOffset;

        public OffsetClock(DateTime serverUtcNow)
        {
            timeOffset = DateTime.UtcNow - serverUtcNow;
        }

        public DateTime Now()
        {
            return DateTime.UtcNow + timeOffset;
        }
    }
}