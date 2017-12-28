using System;

namespace Domain.Exceptions
{
    public class UserLockedOutException : Exception
    {
        public UserLockedOutException(string message) : base(message)
        {
        }

        public UserLockedOutException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}