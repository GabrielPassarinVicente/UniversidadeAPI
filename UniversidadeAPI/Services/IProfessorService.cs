using System.Collections.Generic;
using System.Threading.Tasks;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface IProfessorService
    {
        Task<IEnumerable<Professor>> GetAllProfessores();
        Task<Professor?> GetProfessorById(int id);
        Task<Professor> AddProfessor(Professor professor);
        Task<bool> UpdateProfessor(Professor professor);
        Task<bool> DeleteProfessor(int id);
    }
}