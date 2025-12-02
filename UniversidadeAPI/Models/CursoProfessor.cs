namespace UniversidadeAPI.Models
{
    public class CursoProfessor
    {
        public int IdCursoProfessor { get; set; }
        public int Cursos_IdCursos { get; set; }
        public int Professores_IdProfessores { get; set; }
        public DateTime DataVinculacao { get; set; }
        
        // Propriedades de navegação
        public Curso? Curso { get; set; }
        public Professor? Professor { get; set; }
    }
}
