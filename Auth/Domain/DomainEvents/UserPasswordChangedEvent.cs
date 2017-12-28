using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class UserPasswordChangedEvent : IEvent
    {
        public string Username { get; }
        public string NewPassword { get; }

        public UserPasswordChangedEvent(string username, string newPassword)
        {
            Username = username;
            NewPassword = newPassword;
        }
    }
}