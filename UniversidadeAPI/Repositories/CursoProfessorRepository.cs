namespace UniversidadeAPI.Repositories
{
    public class CursoProfessorRepository : RepositoryBase, ICursoProfessorRepository
    {
        public CursoProfessorRepository(ConectarBanco conectarBanco) : base(conectarBanco)
        {
        }

        public async Task AddCursoProfessor(int cursoId, int professorId)
        {
            var sql = @"
                INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores)
                VALUES (@CursoId, @ProfessorId);";

            await ExecuteAsync(sql, new { CursoId = cursoId, ProfessorId = professorId });
        }

        public async Task RemoveAllProfessoresByCurso(int cursoId)
        {
            await ExecuteAsync("DELETE FROM CursoProfessor WHERE Cursos_IdCursos = @CursoId;", new { CursoId = cursoId });
        }
    }
}
