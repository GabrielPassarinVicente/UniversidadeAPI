using System.Data.Common;
using Dapper;

namespace UniversidadeAPI.Repositories
{
    public abstract class RepositoryBase
    {
        private readonly ConectarBanco _conectarBanco;

        protected RepositoryBase(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.QueryAsync<T>(sql, parametros);
        }

        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.QueryFirstOrDefaultAsync<T>(sql, parametros);
        }

        protected async Task<int> ExecuteAsync(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.ExecuteAsync(sql, parametros);
        }

        protected async Task<T> ExecuteScalarAsync<T>(string sql, object? parametros = null)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await conexao.ExecuteScalarAsync<T>(sql, parametros);
        }

        protected async Task<T> WithConnectionAsync<T>(Func<DbConnection, Task<T>> acao)
        {
            await using var conexao = _conectarBanco.CriarConexao();
            return await acao(conexao);
        }
    }
}
