using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DepartamentoRepository : RepositoryBase, IDepartamentoRepository
    {
        public DepartamentoRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Departamento>> GetAll() =>
            await QueryAsync<Departamento>("SELECT * FROM Departamentos");

        public async Task<Departamento> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Departamento>("SELECT * FROM Departamentos WHERE IdDepartamentos = @Id", new { Id = id });

        public async Task<Departamento> Add(Departamento departamento)
        {
            var sql = @"
                INSERT INTO Departamentos (Nome, Codigo, Descricao, DataCriacao)
                VALUES (@Nome, @Codigo, @Descricao, @DataCriacao);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            departamento.IdDepartamentos = await ExecuteScalarAsync<int>(sql, departamento);
            return departamento;
        }

        public async Task<bool> Update(Departamento departamento)
        {
            var sql = @"
                UPDATE Departamentos SET
                    Nome = @Nome,
                    Codigo = @Codigo,
                    Descricao = @Descricao
                WHERE IdDepartamentos = @IdDepartamentos;";

            var affectedRows = await ExecuteAsync(sql, departamento);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Departamentos WHERE IdDepartamentos = @Id;", new { Id = id });
            return affectedRows > 0;
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Departamentos WHERE Codigo = @Codigo", new { Codigo = codigo });
            return count > 0;
        }
    }
}
