using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task<Usuario> Register(RegistroRequest request);
        string GenerateJwtToken(Usuario usuario);
    }
}
