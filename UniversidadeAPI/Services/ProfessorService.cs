using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories;

namespace UniversidadeAPI.Services
{
    public class ProfessorService : IProfessorService
    {
        private readonly IProfessorRepository _professorRepository;
        private readonly IDisciplinaRepository _disciplinaRepository;

        public ProfessorService(
            IProfessorRepository professorRepository, 
            IDisciplinaRepository disciplinaRepository)
        {
            _professorRepository = professorRepository;
            _disciplinaRepository = disciplinaRepository;
        }

        public async Task<IEnumerable<Professor>> GetAllProfessores()
        {
            return await _professorRepository.GetAll();
        }

        public async Task<Professor?> GetProfessorById(int id)
        {
            return await _professorRepository.GetById(id);
        }

        public async Task<Professor> AddProfessor(Professor professor)
        {
            if (string.IsNullOrWhiteSpace(professor.Nome))
            {
                throw new ArgumentException("Nome do professor é obrigatório.");
            }

            return await _professorRepository.Add(professor);
        }

        public async Task<bool> UpdateProfessor(Professor professor)
        {
            if (string.IsNullOrWhiteSpace(professor.Nome))
            {
                throw new ArgumentException("Nome do professor é obrigatório.");
            }

            return await _professorRepository.Update(professor);
        }

        public async Task<bool> DeleteProfessor(int id)
        {
            // Deletar professor diretamente
            // O CASCADE DELETE no banco vai cuidar dos vínculos automaticamente:
            // - CursoProfessor: DELETE CASCADE (remove vínculos)
            // - Disciplina: SET NULL (mantém disciplina, remove professor)
            return await _professorRepository.Delete(id);
        }
    }
}