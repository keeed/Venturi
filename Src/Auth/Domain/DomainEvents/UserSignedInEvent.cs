using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class UserSignedInEvent : IEvent
    {
        public string Username { get; }

        public UserSignedInEvent(string username)
        {
            Username = username;
        }
    }
}