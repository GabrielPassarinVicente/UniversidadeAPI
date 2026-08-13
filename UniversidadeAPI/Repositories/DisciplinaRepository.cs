using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class DisciplinaRepository : RepositoryBase, IDisciplinaRepository
    {
        public DisciplinaRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task<IEnumerable<Disciplina>> GetAll() =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
            SELECT
                d.*,
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores";

                return await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    splitOn: "IdCursos,IdProfessores"
                );
            });

        public async Task<Disciplina> GetById(int id) =>
            await WithConnectionAsync(async conexao =>
            {
                var sql = @"
            SELECT
                d.*,
                c.IdCursos, c.Nome AS CursoNome, c.CargaHoraria AS CursoCargaHoraria, c.Departamentos_idDepartamentos,
                p.IdProfessores, p.Nome AS ProfessorNome
            FROM Disciplinas d
            LEFT JOIN Cursos c ON d.Curso_IdCursos = c.IdCursos
            LEFT JOIN Professores p ON d.Professor_IdProfessores = p.IdProfessores
            WHERE d.IdDisciplina = @IdDisciplina";

                var disciplinas = await conexao.QueryAsync<Disciplina, Curso, Professor, Disciplina>(
                    sql,
                    (disciplina, curso, professor) =>
                    {
                        disciplina.Curso = curso;
                        disciplina.Professor = professor;
                        return disciplina;
                    },
                    new { IdDisciplina = id },
                    splitOn: "IdCursos,IdProfessores"
                );

                return disciplinas.FirstOrDefault();
            });

        public async Task<IEnumerable<Disciplina>> GetByCurso(int cursoId) =>
            await QueryAsync<Disciplina>("SELECT * FROM Disciplinas WHERE Curso_IdCursos = @CursoId", new { CursoId = cursoId });

        public async Task<IEnumerable<Disciplina>> GetByProfessor(int professorId) =>
            await QueryAsync<Disciplina>("SELECT * FROM Disciplinas WHERE Professor_IdProfessores = @ProfessorId", new { ProfessorId = professorId });

        public async Task<Disciplina> Add(Disciplina disciplina)
        {
            var sql = @"
                INSERT INTO Disciplinas (Nome, Codigo, CargaHoraria, Creditos, Ementa, Curso_IdCursos, Professor_IdProfessores)
                VALUES (@Nome, @Codigo, @CargaHoraria, @Creditos, @Ementa, @Curso_IdCursos, @Professor_IdProfessores);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            disciplina.IdDisciplina = await ExecuteScalarAsync<int>(sql, new
            {
                disciplina.Nome,
                disciplina.Codigo,
                disciplina.CargaHoraria,
                disciplina.Creditos,
                disciplina.Ementa,
                disciplina.Curso_IdCursos,
                disciplina.Professor_IdProfessores
            });
            return disciplina;
        }

        public async Task<bool> Update(Disciplina disciplina)
        {
            var sql = @"
                UPDATE Disciplinas SET
                    Nome = @Nome,
                    Codigo = @Codigo,
                    CargaHoraria = @CargaHoraria,
                    Creditos = @Creditos,
                    Ementa = @Ementa,
                    Curso_IdCursos = @Curso_IdCursos,
                    Professor_IdProfessores = @Professor_IdProfessores
                WHERE IdDisciplina = @IdDisciplina;";

            var affectedRows = await ExecuteAsync(sql, new
            {
                disciplina.IdDisciplina,
                disciplina.Nome,
                disciplina.Codigo,
                disciplina.CargaHoraria,
                disciplina.Creditos,
                disciplina.Ementa,
                disciplina.Curso_IdCursos,
                disciplina.Professor_IdProfessores
            });
            return affectedRows > 0;
        }

        public async Task<bool> Delete(int id)
        {
            var affectedRows = await ExecuteAsync("DELETE FROM Disciplinas WHERE IdDisciplina = @IdDisciplina", new { IdDisciplina = id });
            return affectedRows > 0;
        }

        public async Task<bool> CodigoExists(string codigo)
        {
            var count = await ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Disciplinas WHERE Codigo = @Codigo", new { Codigo = codigo });
            return count > 0;
        }
    }
}
