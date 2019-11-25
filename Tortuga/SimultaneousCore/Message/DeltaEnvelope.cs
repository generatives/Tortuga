using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Message
{
    public class DeltaEnvelope
    {
        public object Deltas { get; set; }
        public long SentTimestamp { get; set; }
    }
}
