namespace UniversidadeAPI.Models
{
    public class Professor
    {
        public int IdProfessores { get; set; }
        public string? Nome { get; set; }

        // Propriedades de navegação
        public ICollection<CursoProfessor> CursosProfessores { get; set; } = new List<CursoProfessor>();
        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
    }
}
