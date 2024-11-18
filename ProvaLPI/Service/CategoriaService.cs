using MySql.Data.MySqlClient;
using ProvaLPI.Domain;
using ProvaLPI.ViewModel;

namespace ProvaLPI.Service
{
    public class CategoriaService
    {

        private readonly ILogger<CategoriaService> _logger;
        private readonly BD _bd;

        public CategoriaService(ILogger<CategoriaService> logger, BD bd)
        {
            _logger = logger;
            _bd = bd;
        }
        public List<Categoria> ListarCategorias(string? query)
        {
            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                SELECT * FROM Categoria
                WHERE @Query IS NULL OR Nome LIKE CONCAT('%', @Query, '%')", connection))
                    {
                        cmd.Parameters.AddWithValue("@Query", string.IsNullOrEmpty(query) ? (object)DBNull.Value : query);

                        using (var dr = cmd.ExecuteReader())
                        {
                            var categorias = MapCategorias(dr);
                            return categorias;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar categorias com a consulta: {Query}", query);
                throw;  // Mantém a stack trace original
            }
        }

        // Método auxiliar para mapear as categorias
        private List<Categoria> MapCategorias(MySqlDataReader dr)
        {
            var categorias = new List<Categoria>();
            while (dr.Read())
            {
                var categoria = new Categoria
                {
                    CategoriaId = dr.GetInt32("CategoriaId"),
                    Nome = dr.GetString("Nome"),
                    // Adicione outros campos conforme necessário
                };
                categorias.Add(categoria);
            }
            return categorias;
        }
    }
}
