using Microsoft.IdentityModel.Tokens;

namespace Domain
{
    public interface ITokenGenerator
    {
        Token GenerateToken(User user);
    }
}