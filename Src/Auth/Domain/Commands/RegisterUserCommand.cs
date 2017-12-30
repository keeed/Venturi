using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Exceptions;
using Domain.PasswordManagement;
using Domain.Repositories;
using Xer.Cqrs.CommandStack;

namespace Domain.Commands
{
    public class RegisterUserCommand : Command
    {
        public Guid UserId { get; }
        public string Username { get; }
        public string Password { get; }

        public RegisterUserCommand(Guid userId, string username, string password)
        {
            UserId = userId;
            Username = username;
            Password = password;
        }
    }

    public class RegisterUserCommandHandler : ICommandAsyncHandler<RegisterUserCommand>
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordValidator _passwordValidator;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(IUserRepository userRepository, PasswordValidator passwordValidator, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordValidator = passwordValidator;
            _passwordHasher = passwordHasher;
        }

        public async Task HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            User user = await _userRepository.GetUserByUsernameAsync(command.Username, cancellationToken);
            if (user != null)
            {
                throw new UsernameAlreadyTakenException("Username is already taken.");
            }

            await _userRepository.SaveAsync(new User(new UserId(command.UserId),
                                                     command.Username,
                                                     Password.Create(command.Password, _passwordValidator, _passwordHasher)));
        }
    }
}