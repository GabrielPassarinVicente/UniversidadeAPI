using System;
using System.Collections.Generic;
using System.Threading.Tasks; 
using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories;

namespace UniversidadeAPI.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }


        public async Task<IEnumerable<Aluno>> GetAllAlunos()
        {
            return await _alunoRepository.GetAll();
        }

        public async Task<Aluno> GetAlunoById(int id)
        {
            return await _alunoRepository.GetById(id);
        }

        public async Task<Aluno> AddAluno(Aluno aluno)
        {
            if (string.IsNullOrWhiteSpace(aluno.Nome) ||
                string.IsNullOrWhiteSpace(aluno.Cpf))
            {
                throw new ArgumentException("Nome completo e CPF são obrigatórios.");
            }

            return await _alunoRepository.Add(aluno);
        }

        public async Task<bool> UpdateAluno(Aluno aluno)
        {
            if (string.IsNullOrWhiteSpace(aluno.Nome) ||
                string.IsNullOrWhiteSpace(aluno.Cpf))
            {
                throw new ArgumentException("Nome completo e CPF são obrigatórios.");
            }

            return await _alunoRepository.Update(aluno);
        }

        public async Task<bool> DeleteAluno(int id)
        {
            return await _alunoRepository.Delete(id);
        }
    }
}