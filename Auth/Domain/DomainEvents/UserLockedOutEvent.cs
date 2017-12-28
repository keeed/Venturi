using System;
using Xer.Cqrs.EventStack;

namespace Domain.DomainEvents
{
    public class UserLockedOutEvent : IEvent
    {
        public string Username { get; }
        public DateTime LockoutExpiry { get; }

        public UserLockedOutEvent(string username, DateTime lockoutExpiry)
        {
            LockoutExpiry = lockoutExpiry;
            Username = username;
        }
    }
}