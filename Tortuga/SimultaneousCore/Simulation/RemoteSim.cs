using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Simulation
{
    public class RemoteSim
    {
        public Guid Id { get; set; }
        public InstanceRole Role { get; private set; }

        private List<object> messagesToSend;

        public RemoteSim(Guid id, InstanceRole role)
        {
            Id = id;
            Role = role;
            messagesToSend = new List<object>();
        }

        public void SendMessage(object message)
        {
            messagesToSend.Add(message);
        }

        public void SendMessages(IEnumerable<object> messages)
        {
            messagesToSend.AddRange(messages);
        }

        public IEnumerable<object> TakeMessagesToSend()
        {
            foreach(var message in messagesToSend)
            {
                yield return message;
            }
            messagesToSend.Clear();
        }
    }
}
