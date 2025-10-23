namespace UniversidadeAPI.Models
{
    public class Aluno
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; }
        public DateTime DataNascimento { get; set; }
        public string Cpf { get; set; } 
        public string Endereco { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public DateTime DataMatricula { get; set; }
    }
}
