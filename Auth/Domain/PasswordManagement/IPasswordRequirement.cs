namespace Domain.PasswordManagement
{
    public interface IPasswordRequirement
    {
        string ErrorMessage { get; }
        bool IsSatisfiedBy(string plainTextPassword);
    }
}