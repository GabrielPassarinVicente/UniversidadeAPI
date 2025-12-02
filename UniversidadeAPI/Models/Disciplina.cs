namespace UniversidadeAPI.Models
{
    public class Disciplina
    {
        public int IdDisciplina { get; set; }
        public string Nome { get; set; }
        public string? Codigo { get; set; }
        public int CargaHoraria { get; set; }
        public int? Creditos { get; set; }
        public string? Ementa { get; set; }
        public int Curso_IdCursos { get; set; }
        public int? Professor_IdProfessores { get; set; }
        public DateTime DataCriacao { get; set; }
        
        // Propriedades de navegação (opcional, para facilitar uso)
        public Curso? Curso { get; set; }
        public Professor? Professor { get; set; }
    }
}
