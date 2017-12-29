namespace Domain.PasswordManagement.PasswordRequirements
{
    public class PasswordLengthRequirement : IPasswordRequirement
    {
        private readonly int _minLength;
        private readonly int _maxLength;

        public string ErrorMessage { get; }

        public PasswordLengthRequirement(int minLength, int maxLength)
        {
            _maxLength = maxLength;
            _minLength = minLength; 
            
            ErrorMessage = $"Password must be between {_minLength} to {_maxLength} characters.";
        }

        public bool IsSatisfiedBy(string plainTextPassword)
        {
            if(string.IsNullOrEmpty(plainTextPassword))
            {
                return false;
            }

            if(plainTextPassword.Length < _minLength || 
               plainTextPassword.Length > _maxLength)
            {
                return false;
            }

            return true;
        }
    }
}