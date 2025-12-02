using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UniversidadeAPI.Models;
using UniversidadeAPI.Repositories; 

namespace UniversidadeAPI.Services
{
    public class CursoService : ICursoService
    {
        private readonly ICursoRepository _cursoRepository;
        private readonly ICursoProfessorRepository _cursoProfessorRepository;
        private readonly IProfessorRepository _professorRepository;

        public CursoService(
            ICursoRepository cursoRepository,
            ICursoProfessorRepository cursoProfessorRepository,
            IProfessorRepository professorRepository)
        {
            _cursoRepository = cursoRepository;
            _cursoProfessorRepository = cursoProfessorRepository;
            _professorRepository = professorRepository;
        }

        public async Task<IEnumerable<Curso>> GetAllCursos()
        {
            return await _cursoRepository.GetAll();
        }

        public async Task<IEnumerable<CursoResponseComProfessores>> GetAllCursosWithProfessores()
        {
            var cursos = await _cursoRepository.GetAllWithProfessores();
            return cursos.Select(MapToCursoResponse).ToList();
        }

        public async Task<Curso> GetCursoById(int id)
        {
            return await _cursoRepository.GetById(id);
        }

        public async Task<CursoResponseComProfessores> GetCursoByIdWithProfessores(int id)
        {
            var curso = await _cursoRepository.GetByIdWithProfessores(id);
            if (curso == null)
                return null;

            return MapToCursoResponse(curso);
        }

        public async Task<Curso> AddCurso(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }
            return await _cursoRepository.Add(curso);
        }

        public async Task<CursoResponseComProfessores> AddCursoWithProfessores(CreateCursoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }

            // Validar se departamento existe
            if (request.Departamentos_idDepartamentos <= 0)
            {
                throw new ArgumentException("Departamento inválido.");
            }

            // Validar se todos os professores existem (apenas se houver)
            List<int> professoresUnicos = new List<int>();
            if (request.Professores?.Count > 0)
            {
                professoresUnicos = request.Professores.Distinct().ToList();
                foreach (var professorId in professoresUnicos)
                {
                    var professor = await _professorRepository.GetById(professorId);
                    if (professor == null)
                    {
                        throw new ArgumentException($"Professor com ID {professorId} não existe.");
                    }
                }
            }

            // Criar e salvar o curso
            var curso = new Curso
            {
                Nome = request.Nome,
                CargaHoraria = request.CargaHoraria,
                Departamentos_idDepartamentos = request.Departamentos_idDepartamentos
            };

            var cursoAdicionado = await _cursoRepository.Add(curso);

            // Adicionar professores ao curso
            foreach (var professorId in professoresUnicos)
            {
                await _cursoProfessorRepository.AddCursoProfessor(cursoAdicionado.IdCursos, professorId);
            }

            // Retornar o curso com professores
            return await GetCursoByIdWithProfessores(cursoAdicionado.IdCursos);
        }

        public async Task<bool> UpdateCurso(Curso curso)
        {
            if (string.IsNullOrWhiteSpace(curso.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }
            return await _cursoRepository.Update(curso);
        }

        public async Task<bool> UpdateCursoWithProfessores(UpdateCursoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                throw new ArgumentException("Nome do curso é obrigatório.");
            }

            // Validar se departamento existe
            if (request.Departamentos_idDepartamentos <= 0)
            {
                throw new ArgumentException("Departamento inválido.");
            }

            // Validar se todos os professores existem (apenas se houver)
            List<int> professoresUnicos = new List<int>();
            if (request.Professores?.Count > 0)
            {
                professoresUnicos = request.Professores.Distinct().ToList();
                foreach (var professorId in professoresUnicos)
                {
                    var professor = await _professorRepository.GetById(professorId);
                    if (professor == null)
                    {
                        throw new ArgumentException($"Professor com ID {professorId} não existe.");
                    }
                }
            }

            // Atualizar curso
            var curso = new Curso
            {
                IdCursos = request.IdCursos,
                Nome = request.Nome,
                CargaHoraria = request.CargaHoraria,
                Departamentos_idDepartamentos = request.Departamentos_idDepartamentos
            };

            var updated = await _cursoRepository.Update(curso);

            if (updated)
            {
                // Remover todos os professores antigos
                await _cursoProfessorRepository.RemoveAllProfessoresByCurso(request.IdCursos);

                // Adicionar novos professores
                foreach (var professorId in professoresUnicos)
                {
                    await _cursoProfessorRepository.AddCursoProfessor(request.IdCursos, professorId);
                }
            }

            return updated;
        }

        public async Task<bool> DeleteCurso(int id)
        {
            // Remover associações com professores (cascade)
            await _cursoProfessorRepository.RemoveAllProfessoresByCurso(id);
            
            // Deletar curso
            return await _cursoRepository.Delete(id);
        }

        private CursoResponseComProfessores MapToCursoResponse(Curso curso)
        {
            return new CursoResponseComProfessores
            {
                IdCursos = curso.IdCursos,
                Nome = curso.Nome,
                CargaHoraria = curso.CargaHoraria,
                Departamentos_idDepartamentos = curso.Departamentos_idDepartamentos,
                Professores = curso.Professores?.Select(p => new ProfessorDTO
                {
                    IdProfessores = p.IdProfessores,
                    Nome = p.Nome
                }).ToList() ?? new List<ProfessorDTO>()
            };
        }
    }
}