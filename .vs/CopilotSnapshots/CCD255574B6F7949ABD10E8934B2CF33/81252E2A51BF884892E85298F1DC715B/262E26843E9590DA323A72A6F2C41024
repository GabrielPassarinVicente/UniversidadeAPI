namespace UniversidadeAPI.Models
{
    public class Curso
    {
        public int IdCursos { get; set; }

        public string? Nome { get; set; }

        public string? CargaHoraria { get; set; }

        public int Departamentos_idDepartamentos { get; set; }

        // Propriedades de navegação
        public Departamento? Departamento { get; set; }
        public ICollection<CursoProfessor> CursosProfessores { get; set; } = new List<CursoProfessor>();
        public ICollection<Professor> Professores { get; set; } = new List<Professor>();
    }
}