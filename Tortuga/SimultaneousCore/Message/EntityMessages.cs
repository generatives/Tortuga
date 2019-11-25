using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Message
{
    public class EntityMessage<T>
    {
        public Guid EntityId { get; set; }
        public T Message { get; set; }

        public EntityMessage(Guid id, T message)
        {
            EntityId = id;
            Message = message;
        }
    }
}
