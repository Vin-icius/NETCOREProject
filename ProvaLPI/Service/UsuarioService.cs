using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using ProvaLPI.Domain;
using ProvaLPI.ViewModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProvaLPI.Service
{
    public class UsuarioService
    {
        private readonly ILogger<UsuarioService> _logger;
        private readonly BD _bd;

        public UsuarioService(ILogger<UsuarioService> logger, BD bd)
        {
            _logger = logger;
            _bd = bd;
        }

        public void CadastrarBD(Domain.Usuario usuario)
        {
            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                INSERT INTO Usuario (Nome, Senha, Email) 
                VALUES (@Nome, @Senha, @Email)", connection))
                    {
                        cmd.Parameters.AddWithValue("@Nome", usuario.Nome);
                        cmd.Parameters.AddWithValue("@Senha", usuario.Senha);
                        cmd.Parameters.AddWithValue("@Email", usuario.Email);

                        cmd.ExecuteNonQuery();
                        usuario.UsuarioId = (int)cmd.LastInsertedId; // Pega o ID Gerado
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inserir usuario: {Nome}, {Email}", usuario.Nome, usuario.Email);
                throw;  // Mantém a stack trace original
            }
        }

        public int Cadastrar(UsuarioViewModel request)
        {
            // Validações de entrada
            if (string.IsNullOrEmpty(request.Nome) || string.IsNullOrEmpty(request.Senha) || string.IsNullOrEmpty(request.Email))
            {
                _logger.LogWarning("Dados de usuário inválidos.");
                throw new ArgumentException("Dados de usuário inválidos.");
            }
            if (!request.Senha.Equals(request.Senha2))
            {
                _logger.LogWarning("Senhas não coincidem.");
                throw new ArgumentException("Senhas não coincidem.");
            }

            var usuario = new Domain.Usuario
            {
                Nome = request.Nome,
                Senha = request.Senha,
                Email = request.Email
            };

            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                        SELECT * FROM Usuario
                        WHERE Email = @Email", connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", usuario.Email);

                        using (var dr = cmd.ExecuteReader())
                        {
                            var usuario2 = Map(dr).FirstOrDefault();
                            if (usuario2 != null && usuario2.Email.Equals(usuario.Email))
                            {
                                throw new Exception("Email já cadastrado.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar email de usuario: {Nome}, {Email}", usuario.Nome, usuario.Email);
                throw;  // Mantém a stack trace original
            }

            // Cadastra o usuário
            CadastrarBD(usuario);

            _logger.LogInformation("Usuário cadastrado com sucesso.");
            return usuario.UsuarioId;
        }


        public string Validar(UsuarioValidarViewModel usuarioView)
        {
            if (usuarioView == null)
            {
                throw new ArgumentException("Requisição inválida: usuarioView é nulo.");
            }

            if (string.IsNullOrEmpty(usuarioView.Email) || string.IsNullOrEmpty(usuarioView.Senha))
            {
                throw new ArgumentException("E-mail e senha devem ser fornecidos.");
            }

            try
            {
                using (var connection = _bd.CriarConexao())
                {
                    connection.Open();
                    using (var cmd = new MySqlCommand(@"
                            SELECT * FROM Usuario
                            WHERE Email = @Email AND Senha = @Senha", connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", usuarioView.Email);
                        cmd.Parameters.AddWithValue("@Senha", usuarioView.Senha);

                        using (var dr = cmd.ExecuteReader())
                        {
                            var usuario = Map(dr).FirstOrDefault();
                            if (usuario != null)
                            {
                                // Gerar o token JWT
                                var token = GerarToken(usuario);
                                return token; // Retorna o token gerado
                            }
                            else
                            {
                                throw new UnauthorizedAccessException("Email ou senha não correspondem a um usuário.");
                            }
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Tentativa de login falhou para o e-mail: {Email}", usuarioView.Email);
                throw; // Re-throwing the exception to indicate authentication failure
            }
            catch (MySqlException sqlEx)
            {
                _logger.LogError(sqlEx, "Erro ao acessar o banco de dados durante a validação do usuário.");
                throw new Exception("Erro ao acessar o banco de dados. Tente novamente mais tarde.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar usuário: {Email}", usuarioView.Email);
                throw; // Mantém a stack trace original
            }
        }

        // Método para gerar o token JWT
        private string GerarToken(Domain.Usuario usuario)
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("id", usuario.UsuarioId.ToString())
            };

            var identidade = new ClaimsIdentity(userClaims);
            var handler = new JwtSecurityTokenHandler();

            // Configuração da chave secreta
            string minhaKey = "minha-chave-secreta-minha-chave-secreta"; // Deve ser uma chave segura
            SecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(minhaKey));
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "Usuários da API",
                Issuer = "Unoeste",
                NotBefore = DateTime.Now,
                Expires = DateTime.Now.AddHours(1), // Ajuste a expiração conforme necessário
                Subject = identidade,
                SigningCredentials = signingCredentials
            };

            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token); // Retorna o token JWT
        }

        private List<Domain.Usuario> Map(MySqlDataReader dr)
        {
            List<Domain.Usuario> usuarios = new List<Domain.Usuario>();
            while (dr.Read())
            {
                var usuario = new Domain.Usuario
                {
                    UsuarioId = (int)dr["UsuarioId"],
                    Nome = dr["Nome"].ToString(),
                    Senha = dr["Senha"].ToString(),
                    Email = dr["Email"].ToString()
                };

                usuarios.Add(usuario);
            }

            return usuarios;
        }


    }
}
