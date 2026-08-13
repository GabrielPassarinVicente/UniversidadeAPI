using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class ProfessorRepository : RepositoryBase, IProfessorRepository
    {
        public ProfessorRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Professor>> GetAll() =>
            await QueryAsync<Professor>("SELECT * FROM Professores");

        public async Task<Professor> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Professor>("SELECT * FROM Professores WHERE IdProfessores = @IdProfessores", new { IdProfessores = id });

        public async Task<Professor> Add(Professor professor)
        {
            var sql = @"
                INSERT INTO Professores (Nome)
                VALUES (@Nome);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            professor.IdProfessores = await ExecuteScalarAsync<int>(sql, professor);
            return professor;
        }

        public async Task<bool> Update(Professor professor)
        {
            var sql = @"
                UPDATE Professores SET
                    Nome = @Nome
                WHERE IdProfessores = @IdProfessores;";

            var affectedRows = await ExecuteAsync(sql, professor);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Professores WHERE IdProfessores = @IdProfessores;", new { IdProfessores = id });
            return affectedRows > 0;
        }
    }
}
