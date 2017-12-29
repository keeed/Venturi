using System.Linq;

namespace Domain.PasswordManagement.PasswordRequirements
{
    public class PasswordHasNumbericCharactersRequirement : IPasswordRequirement
    {
        public string ErrorMessage { get; }

        public PasswordHasNumbericCharactersRequirement()
        {
            ErrorMessage = $"Password must contain numeric characters.";
        }

        public bool IsSatisfiedBy(string plainTextPassword)
        {
            return plainTextPassword.Any(c => char.IsDigit(c));
        }
    }
}