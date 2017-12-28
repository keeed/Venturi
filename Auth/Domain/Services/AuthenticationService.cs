using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;

namespace Domain.Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IPasswordHasher _passwordHasher;

        public AuthenticationService(IUserRepository userRepository, ITokenGenerator tokenGenerator, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _tokenGenerator = tokenGenerator;
            _passwordHasher = passwordHasher;
        }

        public async Task<Token> AuthenticateCredentialsAsync(string username, string password, CancellationToken cancellationToken = default(CancellationToken))
        {
            User user = await _userRepository.GetUserByUsernameAsync(username, cancellationToken);
            if(user != null)
            {   
                if(user.SignIn(password, _passwordHasher))
                {
                    // Publish sign-in success.
                    await _userRepository.SaveAsync(user);

                    return _tokenGenerator.GenerateToken(user);
                }
                else
                {
                    // Publish sign-in failed.
                    await _userRepository.SaveAsync(user);
                }
            }

            throw new InvalidUserCredentialsException("Invalid username or password.");
        }
    }
}