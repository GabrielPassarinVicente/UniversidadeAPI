using Dapper;

namespace UniversidadeAPI.Repositories
{
    public class CursoProfessorRepository : ICursoProfessorRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public CursoProfessorRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task AddCursoProfessor(int cursoId, int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores)
                    VALUES (@CursoId, @ProfessorId);";

                await conexao.ExecuteAsync(sql, new { CursoId = cursoId, ProfessorId = professorId });
            }
        }

        public async Task RemoveAllProfessoresByCurso(int cursoId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = "DELETE FROM CursoProfessor WHERE Cursos_IdCursos = @CursoId;";
                await conexao.ExecuteAsync(sql, new { CursoId = cursoId });
            }
        }
    }
}
