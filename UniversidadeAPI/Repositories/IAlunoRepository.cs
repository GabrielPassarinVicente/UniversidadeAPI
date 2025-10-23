using System.Collections.Generic;
using System.Threading.Tasks;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface IAlunoRepository
    {
        Task<IEnumerable<Aluno>> GetAll();
        Task<Aluno> GetById(int id);
        Task<Aluno> Add(Aluno aluno);
        Task<bool> Update(Aluno aluno);
        Task<bool> Delete(int id);
    }
}