using System.Collections.Generic;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public interface IEventOriginator
    {
        IEnumerable<IEvent> GetEvents();
        void ClearEvents();
    }
}