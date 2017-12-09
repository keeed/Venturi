using System.Collections.Generic;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public interface IEventSource
    {
         IEnumerable<IEvent> Events { get; }
    }
}