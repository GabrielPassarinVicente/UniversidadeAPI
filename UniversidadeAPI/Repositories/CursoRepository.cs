using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class CursoRepository : RepositoryBase, ICursoRepository
    {
        public CursoRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Curso>> GetAll() =>
            await QueryAsync<Curso>("SELECT * FROM Cursos");

        public async Task<Curso> GetById(int id) =>
            await QueryFirstOrDefaultAsync<Curso>("SELECT * FROM Cursos WHERE IdCursos = @IdCursos", new { IdCursos = id });

        public async Task<Curso> GetByIdWithProfessores(int id) =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
                    SELECT DISTINCT c.IdCursos, c.Nome, c.CargaHoraria, c.Departamentos_idDepartamentos
                    FROM Cursos c
                    LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
                    WHERE c.IdCursos = @IdCursos";

                var curso = await conexao.QueryFirstOrDefaultAsync<Curso>(sql, new { IdCursos = id });

                if (curso != null)
                {
                    var sqlProfessores = @"
                        SELECT p.IdProfessores, p.Nome
                        FROM Professores p
                        INNER JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
                        WHERE cp.Cursos_IdCursos = @CursoId
                        ORDER BY p.Nome";

                    var professores = await conexao.QueryAsync<Professor>(sqlProfessores, new { CursoId = id });
                    curso.Professores = professores.ToList();
                }

                return curso;
            });

        public async Task<IEnumerable<Curso>> GetAllWithProfessores() =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
                    SELECT DISTINCT c.IdCursos, c.Nome, c.CargaHoraria, c.Departamentos_idDepartamentos
                    FROM Cursos c
                    LEFT JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
                    ORDER BY c.Nome";

                var cursos = await conexao.QueryAsync<Curso>(sql);

                foreach (var curso in cursos)
                {
                    var sqlProfessores = @"
                        SELECT p.IdProfessores, p.Nome
                        FROM Professores p
                        INNER JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
                        WHERE cp.Cursos_IdCursos = @CursoId
                        ORDER BY p.Nome";

                    var professores = await conexao.QueryAsync<Professor>(sqlProfessores, new { CursoId = curso.IdCursos });
                    curso.Professores = professores.ToList();
                }

                return cursos;
            });

        public async Task<Curso> Add(Curso curso)
        {
            var sql = @"
                INSERT INTO Cursos (Nome, CargaHoraria, Departamentos_idDepartamentos)
                VALUES (@Nome, @CargaHoraria, @Departamentos_idDepartamentos);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            curso.IdCursos = await ExecuteScalarAsync<int>(sql, curso);
            return curso;
        }

        public async Task<bool> Update(Curso curso)
        {
            var sql = @"
                UPDATE Cursos
                SET Nome = @Nome,
                    CargaHoraria = @CargaHoraria,
                    Departamentos_idDepartamentos = @Departamentos_idDepartamentos
                WHERE IdCursos = @IdCursos;";

            var affectedRows = await ExecuteAsync(sql, curso);
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Cursos WHERE IdCursos = @IdCursos;", new { IdCursos = id });
            return affectedRows > 0;
        }
    }
}
