namespace UniversidadeAPI.Models
{
    public class UpdateCursoRequest
    {
        public int IdCursos { get; set; }
        public string? Nome { get; set; }
        public string? CargaHoraria { get; set; }
        public int Departamentos_idDepartamentos { get; set; }
        public List<int> Professores { get; set; } = new List<int>();
    }
}
