using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetByUsername(string username);
        Task<Usuario> GetById(int id);
        Task<Usuario> Add(Usuario usuario);
        Task<bool> UsernameExists(string username);
        Task<bool> EmailExists(string email);
    }
}
