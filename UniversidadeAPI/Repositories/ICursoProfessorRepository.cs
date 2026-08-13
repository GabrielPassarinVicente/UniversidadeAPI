namespace UniversidadeAPI.Repositories
{
    public interface ICursoProfessorRepository
    {
        Task AddCursoProfessor(int cursoId, int professorId);
        Task RemoveAllProfessoresByCurso(int cursoId);
    }
}
