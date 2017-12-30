using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domain.Commands;
using Domain.Exceptions;
using Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Xer.Cqrs.CommandStack;

namespace Api.Controllers
{
    [Route("api")]
    public class AccountController : Controller
    {
        private readonly AuthenticationService _authenticationService;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ICommandAsyncDispatcher _commandDispatcher;

        public AccountController(
            ICommandAsyncDispatcher commandDispatcher,
            ITokenGenerator tokenGenerator,
            AuthenticationService authenticationService
        )
        {
            _commandDispatcher = commandDispatcher;
            _tokenGenerator = tokenGenerator;
            _authenticationService = authenticationService;
        }
        
        [Route("tokens")]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody]LoginDto model)
        {
            try
            {
                Token token = await _authenticationService.AuthenticateCredentialsAsync(model.Username, model.Password);
                if (token != null)
                {
                    return Ok(token.ToString());
                }

                return BadRequest("Invalid username or password.");
            }
            catch(InvalidUserCredentialsException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(UserLockedOutException ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        [Route("accounts")]
        [HttpPost]
        public async Task<IActionResult> Accounts([FromBody]RegisterDto model)
        {
            try
            {
                await _commandDispatcher.DispatchAsync(new RegisterUserCommand(Guid.NewGuid(), model.Username, model.Password));
            }
            catch(PasswordDidNotMeetRequirementsException ex)
            {
                ex.Failures.ToList().ForEach(failure => ModelState.AddModelError("Password", failure.ErrorMessage));

                return BadRequest(ModelState);
            }

            return Accepted();
        }

        // TODO: Change password
        
        public class LoginDto
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Password { get; set; }
        }
        
        public class RegisterDto
        {
            [Required]
            public string Username { get; set; }

            [Required]
            public string Password { get; set; }
        }
    }
}