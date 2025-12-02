using UniversidadeAPI.Models;
using System.Collections.Generic;

namespace UniversidadeAPI.Repositories
{
   
    public interface ICursoRepository
    {
        Task<IEnumerable<Curso>> GetAll();
        Task<Curso> GetById(int id);
        Task<Curso> Add(Curso curso);
        Task<bool> Update(Curso curso);
        Task<bool> Delete(int id);
    }
}