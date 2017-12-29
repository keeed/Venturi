using Domain.Exceptions;

namespace Domain.PasswordManagement
{
    public class Password
    {
        private readonly string _passwordHash;

        private Password(string passwordHash)
        {
            _passwordHash = passwordHash;
        }

        public bool MatchesPassword(string plainTextPassword, IPasswordHasher passwordHasher)
        {
            return passwordHasher.VerifyPasswordHash(_passwordHash, plainTextPassword);
        }

        public Password ChangePassword(string newPlainTextPassword, PasswordValidator validator, IPasswordHasher passwordHasher)
        {
            return Create(newPlainTextPassword, validator, passwordHasher);
        }

        public static Password Create(string plainTextPassword, PasswordValidator validator, IPasswordHasher passwordHasher)
        {
            PasswordValidationResult result = validator.Validate(plainTextPassword);
            if(!result.IsSuccessful)
            {
                throw new PasswordDidNotMeetRequirementsException("Password does not requirements.", result);
            }

            return new Password(passwordHasher.HashPassword(plainTextPassword));
        }
    }
}