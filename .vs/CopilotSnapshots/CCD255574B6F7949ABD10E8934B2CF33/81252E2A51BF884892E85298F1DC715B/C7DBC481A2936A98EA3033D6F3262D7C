using System.Collections.Generic;
using System.Threading.Tasks;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface ICursoService
    {
        Task<IEnumerable<Curso>> GetAllCursos();
        Task<Curso> GetCursoById(int id);
        Task<Curso> AddCurso(Curso curso);
        Task<bool> UpdateCurso(Curso curso);
        Task<bool> DeleteCurso(int id);
    }
}