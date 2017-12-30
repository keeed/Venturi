using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class UserRegisteredEvent : IEvent
    {
        public string Username { get; }
        public string Password { get; }

        public UserRegisteredEvent(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}