using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface IProfessorRepository
    {
        Task<IEnumerable<Professor>> GetAll();
        Task<Professor> GetById(int id);
        Task<Professor> Add(Professor professor);
        Task<bool> Update(Professor professor);
        Task<bool> Delete(int id);
    }
}
