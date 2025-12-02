using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories; 

namespace UniversidadeAPI.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _cursoRepository;

        public CursoService(ICursoRepository cursoRepository)
        {
            _cursoRepository = cursoRepository;
        }


        public async Task<IEnumerable<Curso>> GetAllCursos()
        {
            return await _cursoRepository.GetAll();
        }

        public async Task<Curso> GetCursoById(int id)
        {
            return await _cursoRepository.GetById(id);
        }

        public async Task<Curso> AddCurso(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }
            return await _cursoRepository.Add(curso);
        }

        public async Task<bool> UpdateCurso(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }
            return await _cursoRepository.Update(curso);
        }

        public async Task<bool> DeleteCurso(int id)
        {
            return await _cursoRepository.Delete(id);
        }
    }
}