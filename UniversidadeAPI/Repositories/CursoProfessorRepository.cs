using Dapper;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public class CursoProfessorRepository : ICursoProfessorRepository
    {
        private readonly ConectarBanco _conectarBanco;

        public CursoProfessorRepository(ConectarBanco conectarBanco)
        {
            _conectarBanco = conectarBanco;
        }

        public async Task<IEnumerable<Professor>> GetProfessoresByCursoId(int cursoId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    SELECT p.IdProfessores, p.Nome
                    FROM Professores p
                    INNER JOIN CursoProfessor cp ON p.IdProfessores = cp.Professores_IdProfessores
                    WHERE cp.Cursos_IdCursos = @CursoId
                    ORDER BY p.Nome";

                return await conexao.QueryAsync<Professor>(sql, new { CursoId = cursoId });
            }
        }

        public async Task<IEnumerable<Curso>> GetCursosByProfessorId(int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    SELECT c.IdCursos, c.Nome, c.CargaHoraria, c.Departamentos_idDepartamentos
                    FROM Cursos c
                    INNER JOIN CursoProfessor cp ON c.IdCursos = cp.Cursos_IdCursos
                    WHERE cp.Professores_IdProfessores = @ProfessorId
                    ORDER BY c.Nome";

                return await conexao.QueryAsync<Curso>(sql, new { ProfessorId = professorId });
            }
        }

        public async Task<bool> AddCursoProfessor(int cursoId, int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                // Verificar se já existe
                var existe = await ProfessorExistInCurso(cursoId, professorId);
                if (existe)
                    return true; // Já existe, retorna sucesso

                var sql = @"
                    INSERT INTO CursoProfessor (Cursos_IdCursos, Professores_IdProfessores, DataVinculacao)
                    VALUES (@CursoId, @ProfessorId, @DataVinculacao)";

                var affectedRows = await conexao.ExecuteAsync(sql, new 
                { 
                    CursoId = cursoId, 
                    ProfessorId = professorId,
                    DataVinculacao = DateTime.UtcNow
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> RemoveCursoProfessor(int cursoId, int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    DELETE FROM CursoProfessor 
                    WHERE Cursos_IdCursos = @CursoId 
                    AND Professores_IdProfessores = @ProfessorId";

                var affectedRows = await conexao.ExecuteAsync(sql, new 
                { 
                    CursoId = cursoId, 
                    ProfessorId = professorId 
                });

                return affectedRows > 0;
            }
        }

        public async Task<bool> RemoveAllProfessoresByCurso(int cursoId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    DELETE FROM CursoProfessor 
                    WHERE Cursos_IdCursos = @CursoId";

                var affectedRows = await conexao.ExecuteAsync(sql, new { CursoId = cursoId });
                return affectedRows >= 0; // Retorna true mesmo se não houver registros para deletar
            }
        }

        public async Task<bool> ProfessorExistInCurso(int cursoId, int professorId)
        {
            await using (var conexao = _conectarBanco.CriarConexao())
            {
                var sql = @"
                    SELECT COUNT(*) FROM CursoProfessor 
                    WHERE Cursos_IdCursos = @CursoId 
                    AND Professores_IdProfessores = @ProfessorId";

                var count = await conexao.ExecuteScalarAsync<int>(sql, new 
                { 
                    CursoId = cursoId, 
                    ProfessorId = professorId 
                });

                return count > 0;
            }
        }
    }
}
