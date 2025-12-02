namespace UniversidadeAPI.Models
{
    public class CursoResponseComProfessores
    {
        public int IdCursos { get; set; }
        public string? Nome { get; set; }
        public string? CargaHoraria { get; set; }
        public int Departamentos_idDepartamentos { get; set; }
        public List<ProfessorDTO> Professores { get; set; } = new List<ProfessorDTO>();
    }

    public class ProfessorDTO
    {
        public int IdProfessores { get; set; }
        public string? Nome { get; set; }
    }
}
