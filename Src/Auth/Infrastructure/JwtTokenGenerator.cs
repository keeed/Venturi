using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain;
using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly string _jwtIssuer;
        private readonly string _jwtKey;
        private readonly TimeSpan _expirationSpan;

        public JwtTokenGenerator(string jwtIssuer, string jwtKey, TimeSpan expirationSpan)
        {
            _jwtIssuer = jwtIssuer;
            _jwtKey = jwtKey;
            _expirationSpan = expirationSpan;
        }

        public Token GenerateToken(User user)
        {
            UserState state = user.GetCurrentState();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, state.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.NameId, state.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddDays(_expirationSpan.Days);

            var token = new JwtSecurityToken(
                _jwtIssuer,
                _jwtIssuer, // TODO: Audience
                claims,
                DateTime.Now,
                expiration,
                creds
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new Token(tokenString);
        }
    }
}