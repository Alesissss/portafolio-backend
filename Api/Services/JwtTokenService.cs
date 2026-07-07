using Api.Configurations;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class JwtTokenService(IOptions<JwtOptions> _jwtOptions) : IJwtTokenService
    {
        private readonly JwtOptions _options = _jwtOptions.Value;
        public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
        {
            var expiraEn = DateTime.UtcNow.AddMinutes(_options.ExpireMinutes);

            var claims = new List<Claim>
            {
                new("sub", usuario.IdUsuario.ToString()),
                new("username", usuario.Username),
                new("name", $"{usuario.Nombres} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno}"),
                new("role", usuario.IdRol.ToString()),
            };

            var llaveSeguridad = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var credenciales = new SigningCredentials(llaveSeguridad, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiraEn,
                signingCredentials: credenciales
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiraEn);
        }
    }
}
