using UniversidadeAPI.Models;

namespace UniversidadeAPI.Services
{
    public interface IDepartamentoService
    {
        Task<IEnumerable<Departamento>> GetAllDepartamentos();
        Task<Departamento> GetDepartamentoById(int id);
        Task<Departamento> AddDepartamento(Departamento departamento);
        Task<bool> UpdateDepartamento(Departamento departamento);
        Task<bool> DeleteDepartamento(int id);
    }
}
