using System.Collections.Generic;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public interface IEventSource
    {
         IEnumerable<IEvent> GetEvents();
         void ClearEvents();
    }
}