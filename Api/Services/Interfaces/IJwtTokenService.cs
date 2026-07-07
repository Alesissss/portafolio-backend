using Api.Models;

namespace Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
    }
}
