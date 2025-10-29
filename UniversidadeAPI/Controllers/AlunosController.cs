using Microsoft.AspNetCore.Mvc;
using UniversidadeAPI.Models;
using UniversidadeAPI.Services;
using System;
using System.Collections.Generic; 
using System.Threading.Tasks; 

namespace UniversidadeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlunosController : ControllerBase
    {
        private readonly IAlunoService _alunoService;

        public AlunosController(IAlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Aluno>>> GetAlunos()
        {
            var alunos = await _alunoService.GetAllAlunos();
            return Ok(alunos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Aluno>> GetAluno(int id)
        {
             var aluno = await _alunoService.GetAlunoById(id);

            if (aluno == null)
            {
                return NotFound(); 
            }
            return Ok(aluno); 
        }

        [HttpPost]
        public async Task<ActionResult<Aluno>> PostAluno(Aluno aluno)
        {
            try
            {
                var newAluno = await _alunoService.AddAluno(aluno);

                return CreatedAtAction(nameof(GetAluno), new { id = newAluno.IdAluno }, newAluno);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAluno(int id, Aluno aluno)
        {
            if (id != aluno.IdAluno)
            {
                return BadRequest("O ID na URL não corresponde ao ID do aluno no corpo da requisição.");
            }

            try
            {
                // Usa 'await'
                var updated = await _alunoService.UpdateAluno(aluno);

                if (updated)
                {
                    return NoContent(); 
                }

                return NotFound(); 
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); 
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAluno(int id)
        {
            if (await _alunoService.DeleteAluno(id))
            {
                return NoContent(); 
            }

            return NotFound(); 
        }
    }
}