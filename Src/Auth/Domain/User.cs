using System;
using System.Collections.Generic;
using Domain.DomainEvents;
using Domain.Exceptions;
using Domain.PasswordManagement;
using Xer.Cqrs.EventStack;

namespace Domain
{
    public class User : IEventOriginator, IStateContainer<UserState> // Aggregate
    {
        private readonly List<IEvent> _events = new List<IEvent>();

        private UserId _id;
        private string _username;
        private Password _password;
        private bool _isLockedOut;
        private DateTime? _lockoutExpiry;
        private int _numberOfFailedSignInAttempts;

        public User(UserId userId, string username, Password password)
            : this(new UserState(userId, username, password, false, null, 0))
        {
        }

        public User(UserState userState)
        {
            if (userState == null)
            {
                throw new ArgumentNullException(nameof(userState));
            }

            _id = userState.Id;
            _username = userState.Username;
            _password = userState.Password;
            _isLockedOut = userState.IsLockedOut;
            _lockoutExpiry = userState.LockoutExpiry;
            _numberOfFailedSignInAttempts = userState.NumberOfFailedSignInAttempts;
        }
        
        public void ChangePassword(string newPassword, PasswordValidator passwordValidator, IPasswordHasher passwordHasher)
        {
            _password = _password.ChangePassword(newPassword, passwordValidator, passwordHasher);
            _events.Add(new UserPasswordChangedEvent(_username, newPassword));
        }

        public bool SignIn(string password, IPasswordHasher passwordHasher)
        {
            if(_isLockedOut)
            {
                throw new UserLockedOutException("Sign-in failed. User has been locked out.");
            }

            if(_password.MatchesPassword(password, passwordHasher))
            {
                // Reset.
                _numberOfFailedSignInAttempts = 0;

                _events.Add(new UserSignedInEvent(_username));
                return true;
            }
            
            // Increment.
            _numberOfFailedSignInAttempts++;

            _events.Add(new UserSignInFailedEvent(_username));
            return false;
        }

        public bool HasExceededMaxFailedSignInAttempts(int maximumAttempts)
        {
            return _numberOfFailedSignInAttempts > maximumAttempts;
        }

        public void Lockout(TimeSpan duration)
        {
            _isLockedOut = true;
            _lockoutExpiry = DateTime.Now.Add(duration);

            _events.Add(new UserLockedOutEvent(_username, _lockoutExpiry.Value));
        }

        public void ReleaseLock()
        {
            _isLockedOut = false;
            _lockoutExpiry = DateTime.MaxValue;
        }

        public IEnumerable<IEvent> GetEvents() => _events.AsReadOnly();
        public void ClearEvents() => _events.Clear();

        public UserState GetCurrentState()
        {
            return new UserState(_id, _username, _password, _isLockedOut, _lockoutExpiry, _numberOfFailedSignInAttempts);
        }
    }

    public class UserState
    {
        public UserId Id { get; private set; }
        public string Username { get; private set; }
        public Password Password { get; private set; }
        public bool IsLockedOut { get; private set; }
        public DateTime? LockoutExpiry { get; private set; }
        public int NumberOfFailedSignInAttempts { get; private set; }

        public UserState(UserId userId, 
                         string username, 
                         Password password, 
                         bool isLockedOut, 
                         DateTime? lockoutExpiry, 
                         int numberOfFailedSignInAttempts)
        {
            Id = userId;
            Username = username;
            Password = password;
            IsLockedOut = isLockedOut;
            LockoutExpiry = lockoutExpiry;
            NumberOfFailedSignInAttempts = numberOfFailedSignInAttempts;
        }
    }
}