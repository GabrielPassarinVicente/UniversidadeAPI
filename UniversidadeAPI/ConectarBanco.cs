using MySql.Data.MySqlClient;
using System.Data.Common;

namespace UniversidadeAPI
{
    public class ConectarBanco
    {

        private readonly string _connectionString;
        public ConectarBanco(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString),
                    "A string de conexão 'DefaultConnection' não foi encontrada no appsettings.json.");
            }
        }

        public DbConnection CriarConexao()
        {
            return new MySqlConnection(_connectionString);
        }
    }

}

