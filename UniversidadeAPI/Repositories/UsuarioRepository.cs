using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class UsuarioRepository : RepositoryBase, IUsuarioRepository
    {
        public UsuarioRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<Usuario> GetByUsername(string username) =>
            await QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM Usuario WHERE Username = @Username", new { Username = username });

        public async Task<Usuario> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Usuario>("SELECT * FROM Usuario WHERE Id = @Id", new { Id = id });

        public async Task<Usuario> Add(Usuario usuario)
        {
            var sql = @"
                INSERT INTO Usuario (Username, PasswordHash, Email, DataCriacao)
                VALUES (@Username, @PasswordHash, @Email, @DataCriacao);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            usuario.Id = await ExecuteScalarAsync<int>(sql, usuario);
            return usuario;
        }

        public async Task<bool> UsernameExists(string username)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Usuario WHERE Username = @Username", new { Username = username });
            return count > 0;
        }

        public async Task<bool> EmailExists(string email)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Usuario WHERE Email = @Email", new { Email = email });
            return count > 0;
        }
    }
}
