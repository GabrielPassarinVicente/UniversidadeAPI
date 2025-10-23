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
    public class ProfessoresController : ControllerBase
    {
        
        private readonly IProfessorService _professorService;

        public ProfessoresController(IProfessorService professorService)
        {
            _professorService = professorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Professor>>> GetAllProfessores()
        {
            var professores = await _professorService.GetAllProfessores();
            return Ok(professores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Professor>> GetProfessorById(int id)
        {
            
            var professor = await _professorService.GetProfessorById(id);

            if (professor == null)
            {
                return NotFound();
            }
            return Ok(professor);
        }

        [HttpPost]
        public async Task<ActionResult<Professor>> AddProfessor(Professor professor)
        {
            try
            {
                var newProfessor = await _professorService.AddProfessor(professor);

             
                return CreatedAtAction(nameof(GetProfessorById), new { id = newProfessor.IdProfessores }, newProfessor);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}