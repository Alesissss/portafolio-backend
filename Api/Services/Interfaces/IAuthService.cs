using Api.Dtos;

namespace Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(LoginRequestDto dto);
    }
}
