namespace Domain
{
    public interface IPasswordHasher
    {
        string HashPassword(string clearTextPassword);
        bool VerifyPasswordHash(string passwordHash, string clearTextPassword);
    }
}