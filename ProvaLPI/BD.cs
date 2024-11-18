using MySql.Data.MySqlClient;
using System.Configuration;

namespace ProvaLPI
{
    public class BD
    {
        private readonly Configuration configuration;
        public MySqlConnection CriarConexao()
        {
            string strCon = Environment.GetEnvironmentVariable("stringConexao");
            MySqlConnection conexao = new MySqlConnection(strCon);
            return conexao;
        }

    }
}
