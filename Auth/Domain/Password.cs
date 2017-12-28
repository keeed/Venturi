namespace Domain
{
    public class Password
    {
        private readonly string _passwordHash;

        public Password(string passwordHash)
        {
            _passwordHash = passwordHash;
        }

        public bool MatchesPassword(string plainTextPassword, IPasswordHasher passwordHasher)
        {
            return passwordHasher.VerifyPasswordHash(_passwordHash, plainTextPassword);
        }

        public Password ChangePassword(string newPlainTextPassword, IPasswordHasher passwordHasher)
        {
            return GenerateHash(newPlainTextPassword, passwordHasher);
        }

        public static Password GenerateHash(string plainTextPassword, IPasswordHasher passwordHasher)
        {
            return new Password(passwordHasher.HashPassword(plainTextPassword));
        }
    }
}