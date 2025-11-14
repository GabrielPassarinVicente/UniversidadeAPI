using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DepartamentoRepository : IDepartamentoRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public DepartamentoRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task<IEnumerable<Departamento>> GetAll()
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Departamentos";
                return await conexao.QueryAsync<Departamento>(sql);
            }
        }

        public async Task<Departamento> GetById(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT * FROM Departamentos WHERE IdDepartamentos = @Id";
                return await conexao.QueryFirstOrDefaultAsync<Departamento>(sql, new { Id = id });
            }
        }

        public async Task<Departamento> Add(Departamento departamento)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO Departamentos (Nome, Codigo, Descricao, DataCriacao)
                    VALUES (@Nome, @Codigo, @Descricao, @DataCriacao);
                    SELECT LAST_INSERT_ID();";

                var newId = await conexao.ExecuteScalarAsync<int>(sql, departamento);
                departamento.IdDepartamentos = newId;
                return departamento;
            }
        }

        public async Task<bool> Update(Departamento departamento)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    UPDATE Departamentos SET 
                        Nome = @Nome, 
                        Codigo = @Codigo, 
                        Descricao = @Descricao
                    WHERE IdDepartamentos = @IdDepartamentos;";

                var affectedRows = await conexao.ExecuteAsync(sql, departamento);
                return affectedRows > 0;
            }
        }

        public async Task<bool> Delete(int id)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM Departamentos WHERE IdDepartamentos = @Id;";
                var affectedRows = await conexao.ExecuteAsync(sql, new { Id = id });
                return affectedRows > 0;
            }
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "SELECT COUNT(*) FROM Departamentos WHERE Codigo = @Codigo";
                var count = await conexao.ExecuteScalarAsync<int>(sql, new { Codigo = codigo });
                return count > 0;
            }
        }
    }
}
