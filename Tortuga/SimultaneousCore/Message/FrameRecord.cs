using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Message
{
    public class FrameCommands
    {
        public List<object> Commands { get; set; }
        public long SentTimestamp { get; set; }
        public float SentDelta { get; set; }
    }

    public class FrameSnapshot
    {
        public object Snapshot { get; set; }
        public long RecordedTimestamp { get; set; }
    }

    public class RecievedCmdEnvelope
    {
        public FrameCommands Envelope { get; set; }
        public long RecievedTimestamp { get; set; }
        public bool HasBeenProcessed { get; set; }
    }
}
