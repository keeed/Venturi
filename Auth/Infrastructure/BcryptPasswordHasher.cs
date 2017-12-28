using Domain;

namespace Infrastructure
{
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string clearTextPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(clearTextPassword);
        }

        public bool VerifyPasswordHash(string passwordHash, string clearTextPassword)
        {
            return BCrypt.Net.BCrypt.Verify(clearTextPassword, passwordHash);
        }
    }
}