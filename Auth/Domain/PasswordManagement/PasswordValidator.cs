using System;
using System.Collections.Generic;
using Domain.PasswordManagement.PasswordRequirements;

namespace Domain.PasswordManagement
{
    public class PasswordValidator
    {
        private readonly IEnumerable<IPasswordRequirement> _passwordRequirements;

        public PasswordValidator(IEnumerable<IPasswordRequirement> passwordRequirement)
        {
            _passwordRequirements = passwordRequirement;
        }

        public PasswordValidationResult Validate(string plainTextPassword)
        {
            if(string.IsNullOrEmpty(plainTextPassword))
            {
                throw new ArgumentException("Password is null or empty.", nameof(plainTextPassword));
            }
            
            PasswordValidationResult result = new PasswordValidationResult();

            foreach(IPasswordRequirement requirement in _passwordRequirements)
            {
                if(!requirement.IsSatisfiedBy(plainTextPassword))
                {
                    result.AddValidationFailure(requirement.ErrorMessage);
                }
            }

            return result;
        }
    }

    public class PasswordValidationResult
    {        
        private readonly List<PasswordValidationFailure> _failures = new List<PasswordValidationFailure>();

        public IReadOnlyCollection<PasswordValidationFailure> Failures => _failures.AsReadOnly();
        public bool IsSuccessful => _failures.Count == 0;

        public void AddValidationFailure(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException("Error message is null or empty.", nameof(errorMessage));
            }

            _failures.Add(new PasswordValidationFailure(errorMessage));
        }

        public void AddValidationFailure(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            AddValidationFailure(exception.Message);
        }
    }

    public class PasswordValidationFailure
    {
        public string ErrorMessage { get; }

        public PasswordValidationFailure(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}