using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface ICursoProfessorRepository
    {
        Task<IEnumerable<Professor>> GetProfessoresByCursoId(int cursoId);
        Task<IEnumerable<Curso>> GetCursosByProfessorId(int professorId);
        Task<bool> AddCursoProfessor(int cursoId, int professorId);
        Task<bool> RemoveCursoProfessor(int cursoId, int professorId);
        Task<bool> RemoveAllProfessoresByCurso(int cursoId);
        Task<bool> ProfessorExistInCurso(int cursoId, int professorId);
    }
}
