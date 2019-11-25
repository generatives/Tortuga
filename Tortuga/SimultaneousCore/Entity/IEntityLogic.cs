using System;
using System.Collections.Generic;
using System.Text;

namespace SimultaneousCore.Entity
{
    public interface IEntityLogic
    {
        void Simulate(float deltaTime);

        object GenerateCommands();
        void ProcessCommands(IEnumerable<object> commands);

        object CalculateDeltas(object oldSnapshot, object newSnapshot);
        void ApplyDeltas(object deltas);

        object TakeSnapshot();
        void ApplySnapshot(object snapshot);

        object GetCreationInfo();
    }
}
