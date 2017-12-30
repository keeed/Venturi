using System;
using System.Collections.Generic;
using Domain.PasswordManagement;

namespace Domain.Exceptions
{
    public class PasswordDidNotMeetRequirementsException : Exception
    {
        public IReadOnlyCollection<PasswordValidationFailure> Failures { get; }

        public PasswordDidNotMeetRequirementsException(string message, PasswordValidationResult result) : base(message)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            Failures = result.Failures;
        }
    }
}