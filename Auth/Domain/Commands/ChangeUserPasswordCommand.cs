using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class ChangeUserPasswordCommand : Command
    {
        public string Username { get; }
        public string NewPassword { get; }

        public ChangeUserPasswordCommand(string username, string newPassword)
        {
            Username = username;
            NewPassword = newPassword;
        }
    }

    public class ChangeUserPasswordCommandHandler : ICommandAsyncHandler<ChangeUserPasswordCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;


        public ChangeUserPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }
        public async Task HandleAsync(ChangeUserPasswordCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            User user = await _userRepository.GetUserByUsernameAsync(command.Username, cancellationToken).ConfigureAwait(false);
            if(user == null)
            {
                throw new UserNotFoundException("User not found.");
            }

            user.ChangePassword(command.NewPassword, _passwordHasher);

            await _userRepository.SaveAsync(user);
        }
    }
}