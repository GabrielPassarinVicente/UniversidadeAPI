using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories;

namespace UniversidadeAPI.Services
{
    public class DepartamentoService : IDepartamentoService
    {
        private readonly IDepartamentoRepository _departamentoRepository;

        public DepartamentoService(IDepartamentoRepository departamentoRepository)
        {
            _departamentoRepository = departamentoRepository;
        }

        public async Task<IEnumerable<Departamento>> GetAllDepartamentos()
        {
            return await _departamentoRepository.GetAll();
        }

        public async Task<Departamento> GetDepartamentoById(int id)
        {
            return await _departamentoRepository.GetById(id);
        }

        public async Task<Departamento> AddDepartamento(Departamento departamento)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(departamento.Nome))
            {
                throw new ArgumentException("Nome do departamento é obrigatório.");
            }

            // Validar código único se fornecido
            if (!string.IsNullOrWhiteSpace(departamento.Codigo))
            {
                if (await _departamentoRepository.CodigoExists(departamento.Codigo))
                {
                    throw new ArgumentException($"Código '{departamento.Codigo}' já está em uso.");
                }
            }

            // Definir data de criação
            departamento.DataCriacao = DateTime.UtcNow;

            return await _departamentoRepository.Add(departamento);
        }

        public async Task<bool> UpdateDepartamento(Departamento departamento)
        {
            // Validações
            if (string.IsNullOrWhiteSpace(departamento.Nome))
            {
                throw new ArgumentException("Nome do departamento é obrigatório.");
            }

            // Verificar se existe
            var existingDepartamento = await _departamentoRepository.GetById(departamento.IdDepartamentos);
            if (existingDepartamento == null)
            {
                return false;
            }

            return await _departamentoRepository.Update(departamento);
        }

        public async Task<bool> DeleteDepartamento(int id)
        {
            return await _departamentoRepository.Delete(id);
        }
    }
}
