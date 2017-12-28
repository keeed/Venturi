using System;
using System.Collections.Generic;
using Domain.DomainEvents;
using Domain.Exceptions;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class User : IEventSource // Aggregate
    {
        private readonly List<IEvent> _events = new List<IEvent>();

        public UserId Id { get; private set; }
        public string Username { get; private set; }
        public Password Password { get; private set; }
        public bool IsLockedOut { get; private set; }
        public DateTime LockoutExpiry { get; private set; }
        public int NumberOfFailedSignInAttempts { get; private set; }

        public User(UserId userId, string username, Password password)
            : this(userId, username, password, false, DateTime.MaxValue, 0)
        {
        }

        public User(UserId userId, string username, Password password, bool isLockedOut, DateTime lockoutExpiry, int numberOfFailedSignInAttempts)
        {
            Id = userId;
            Username = username;
            Password = password;
            IsLockedOut = isLockedOut;
            LockoutExpiry = lockoutExpiry;
            NumberOfFailedSignInAttempts = numberOfFailedSignInAttempts;
        }
        
        public void ChangePassword(string newPassword, IPasswordHasher passwordHasher)
        {
            Password = Password.ChangePassword(newPassword, passwordHasher);
            _events.Add(new UserPasswordChangedEvent(Username, newPassword));
        }

        public bool SignIn(string password, IPasswordHasher passwordHasher)
        {
            if(IsLockedOut)
            {
                throw new UserLockedOutException("Sign-in failed. User has been locked out.");
            }

            if(Password.MatchesPassword(password, passwordHasher))
            {
                // Reset.
                NumberOfFailedSignInAttempts = 0;

                _events.Add(new UserSignedInEvent(Username));
                return true;
            }
            
            // Increment.
            NumberOfFailedSignInAttempts++;

            _events.Add(new UserFailedSignInEvent(Username));
            return false;
        }

        public bool HasReachedMaximumSignInAttempts(int maximum)
        {
            return NumberOfFailedSignInAttempts == maximum;
        }

        public void Lockout(TimeSpan duration)
        {
            IsLockedOut = true;
            LockoutExpiry = DateTime.Now.Add(duration);

            _events.Add(new UserLockedOutEvent(Username, LockoutExpiry));
        }

        public void ReleaseLock()
        {
            IsLockedOut = false;
            LockoutExpiry = DateTime.MaxValue;
        }

        public IEnumerable<IEvent> GetEvents() => _events.AsReadOnly();
        public void ClearEvents() => _events.Clear();
    }
}