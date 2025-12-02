using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface IDisciplinaService
    {
        Task<IEnumerable<Disciplina>> GetAllDisciplinas();
        Task<Disciplina> GetDisciplinaById(int id);
        Task<IEnumerable<Disciplina>> GetDisciplinasByCurso(int cursoId);
        Task<IEnumerable<Disciplina>> GetDisciplinasByProfessor(int professorId);
        Task<Disciplina> AddDisciplina(Disciplina disciplina);
        Task<bool> UpdateDisciplina(Disciplina disciplina);
        Task<bool> DeleteDisciplina(int id);
    }
}
