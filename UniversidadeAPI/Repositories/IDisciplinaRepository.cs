using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface IDisciplinaRepository
    {
        Task<IEnumerable<Disciplina>> GetAll();
        Task<Disciplina> GetById(int id);
        Task<IEnumerable<Disciplina>> GetByCurso(int cursoId);
        Task<IEnumerable<Disciplina>> GetByProfessor(int professorId);
        Task<Disciplina> Add(Disciplina disciplina);
        Task<bool> Update(Disciplina disciplina);
        Task<bool> Delete(int id);
        Task<bool> CodigoExists(string codigo);
    }
}
