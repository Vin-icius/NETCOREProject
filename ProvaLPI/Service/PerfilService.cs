using MySql.Data.MySqlClient;
using ProvaLPI.Domain;
using ProvaLPI.ViewModel;

namespace ProvaLPI.Service
{
    public class PerfilService
    {
        private readonly ILogger<PerfilService> _logger;
        private readonly BD _bd;

        public PerfilService(ILogger<PerfilService> logger, BD bd)
        {
            _logger = logger;
            _bd = bd;
        }

        public List<Perfil> Listar(int usuarioId)
        {
            if (usuarioId <= 0)
            {
                throw new Exception("Usuário inválido.");
            }

            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                SELECT * FROM Perfil
                WHERE UsuarioId = @UsuarioId", connection))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        using (var dr = cmd.ExecuteReader())
                        {
                            var perfis = Map(dr);
                            if (perfis != null)
                            {
                                return perfis;
                            }
                            else
                            {
                                throw new Exception("Id não corresponde a algum usuário.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Erro ao validar perfil: {Id}", usuarioId);
                throw;  // Mantém a stack trace original
            }
        }

        public void InserirBD(Perfil perfil)
        {
            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                        INSERT INTO Perfil (Nome, Tipo, UsuarioId) 
                        VALUES (@Nome, @Tipo, @UsuarioId)", connection))
                    {
                        cmd.Parameters.AddWithValue("@Nome", perfil.Nome);
                        cmd.Parameters.AddWithValue("@Tipo", perfil.Tipo);
                        cmd.Parameters.AddWithValue("@UsuarioId", perfil.UsuarioId);

                        cmd.ExecuteNonQuery();
                        perfil.PerfilId = (int)cmd.LastInsertedId; // Pega o ID Gerado
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir perfil: {Nome}, {Email}", perfil.Nome);
                throw;  // Mantém a stack trace original
            }
        }

        public void Inserir(int usuarioId, PerfilViewModel request)
        {
            // Validações de entrada
            if (request == null)
            {
                _logger.LogWarning("Dados de perfil inválidos.");
                throw new ArgumentException("Dados de perfil inválidos.");
            }

            if (string.IsNullOrWhiteSpace(request.Nome))
            {
                _logger.LogWarning("Nome do perfil não pode ser vazio.");
                throw new ArgumentException("Nome do perfil não pode ser vazio.");
            }

            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();

                    // Verifica se o nome do perfil já existe
                    using (var cmd = new MySqlCommand(@"
                SELECT * FROM Perfil
                WHERE Nome = @Nome", connection))
                    {
                        cmd.Parameters.AddWithValue("@Nome", request.Nome);
                        using (var dr = cmd.ExecuteReader())
                        {
                            var perfis = Map(dr);
                            if (perfis.Count > 0) // Se existe algum perfil com o mesmo nome
                            {
                                throw new Exception("Nome já cadastrado.");
                            }
                        }
                    }

                    // Verifica a contagem de perfis do usuário
                    using (var cmd = new MySqlCommand(@"
                SELECT COUNT(*) FROM Perfil
                WHERE UsuarioId = @UsuarioId", connection))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        // Verifica se o usuário já possui 4 perfis
                        if (count >= 4)
                        {
                            throw new Exception("Limite de 4 perfis por usuário alcançado.");
                        }
                    }

                    // Insere o novo perfil no banco de dados
                    InserirBD(new Perfil
                    {
                        Nome = request.Nome,
                        Tipo = request.Tipo,
                        UsuarioId = usuarioId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir perfil para o usuário {UsuarioId}: {Nome}", usuarioId, request.Nome);
                throw;  // Mantém a stack trace original
            }
        }

        public void Atualizar(int usuarioId, int perfilId, PerfilAlterarViewModel perfilView)
        {
            // Validações de entrada
            if (perfilView == null)
            {
                _logger.LogWarning("Novo nome do perfil não pode ser vazio.");
                throw new ArgumentException("Novo nome do perfil não pode ser vazio.");
            }

            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();

                    // Verifica se já existe um perfil com o mesmo nome, exceto o atual
                    using (var cmd = new MySqlCommand(@"
                        SELECT COUNT(*) FROM Perfil
                        WHERE Nome = @Nome AND PerfilId != @PerfilId", connection))
                    {
                        cmd.Parameters.AddWithValue("@Nome", perfilView.Nome);
                        cmd.Parameters.AddWithValue("@PerfilId", perfilId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        // Se já existe um perfil com o mesmo nome, rejeita a atualização
                        if (count > 0)
                        {
                            throw new Exception("Já existe um perfil com esse nome.");
                        }
                    }

                    // Atualiza o nome do perfil
                    using (var cmd = new MySqlCommand(@"
                        UPDATE Perfil
                        SET Nome = @NovoNome
                        WHERE PerfilId = @PerfilId AND UsuarioId = @UsuarioId", connection))
                    {
                        cmd.Parameters.AddWithValue("@NovoNome", perfilView.Nome);
                        cmd.Parameters.AddWithValue("@PerfilId", perfilId);
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("Perfil não encontrado ou usuário não autorizado.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar nome do perfil {PerfilId} para o usuário {UsuarioId}", perfilId, usuarioId);
                throw;  // Mantém a stack trace original
            }
        }


        private List<Domain.Perfil> Map(MySqlDataReader dr)
        {
            List<Domain.Perfil> perfis = new List<Domain.Perfil>();
            while (dr.Read())
            {
                var perfil = new Domain.Perfil
                {

                    Nome = dr["Nome"].ToString(),
                    Tipo = (Domain.PerfilTipo)(short)dr["Tipo"]
                };

                perfis.Add(perfil);
            }

            return perfis;
        }
    }
}
