using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public UsuarioRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task<Usuario> GetByUsername(string username)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Usuario WHERE Username = @Username";
                return await conexao.QueryFirstOrDefaultAsync<Usuario>(sql, new { Username = username });
            }
        }

        public async Task<Usuario> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Usuario WHERE Id = @Id";
                return await conexao.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
            }
        }

        public async Task<Usuario> Add(Usuario usuario)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Usuario (Username, PasswordHash, Email, DataCriacao)
                    VALUES (@Username, @PasswordHash, @Email, @DataCriacao);
                    SELECT LAST_INSERT_ID();";

                var newId = await conexao.ExecuteScalarAsync<int>(sql, usuario);
                usuario.Id = newId;
                return usuario;
            }
        }

        public async Task<bool> UsernameExists(string username)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT COUNT(*) FROM Usuario WHERE Username = @Username";
                var count = await conexao.ExecuteScalarAsync<int>(sql, new { Username = username });
                return count > 0;
            }
        }

        public async Task<bool> EmailExists(string email)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT COUNT(*) FROM Usuario WHERE Email = @Email";
                var count = await conexao.ExecuteScalarAsync<int>(sql, new { Email = email });
                return count > 0;
            }
        }
    }
}
