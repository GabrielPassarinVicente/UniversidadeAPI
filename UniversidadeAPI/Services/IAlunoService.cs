using System.Collections.Generic;
using System.Threading.Tasks;
using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface IAlunoService
    {
        Task<IEnumerable<Aluno>> GetAllAlunos();
        Task<Aluno> GetAlunoById(int id);
        Task<Aluno> AddAluno(Aluno aluno);
        Task<bool> UpdateAluno(Aluno aluno);
        Task<bool> DeleteAluno(int id);
    }
}