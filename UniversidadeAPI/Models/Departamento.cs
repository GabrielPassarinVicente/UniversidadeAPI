namespace UniversidadeAPI.Models
{
    public class Departamento
    {
        public int IdDepartamentos { get; set; }
        public string Nome { get; set; }
        public string? Codigo { get; set; }
        public string? Descricao { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
