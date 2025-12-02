using UniversidadeAPI.Models;

namespace UniversidadeAPI.Repositories
{
    public interface IDepartamentoRepository
    {
        Task<IEnumerable<Departamento>> GetAll();
        Task<Departamento> GetById(int id);
        Task<Departamento> Add(Departamento departamento);
        Task<bool> Update(Departamento departamento);
        Task<bool> Delete(int id);
        Task<bool> CodigoExists(string codigo);
    }
}
