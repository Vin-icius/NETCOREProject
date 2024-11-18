using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Ocsp;
using ProvaLPI.Domain;
using ProvaLPI.Service;
using ProvaLPI.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;

namespace ProvaLPI.Controller
{
    /// <summary>
    /// •    Este Controller gerencia os usuários.
    /// </summary>
    [Authorize("APIAuth")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;

        public UsuariosController(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        ///•	Este endpoint tem como objetivo cadastrar um novo usuário.
        ///•	No corpo da requisição, é necessário fornecer o nome, senha, confirmação de senha e e-mail.
        ///•	A senha e a confirmação de senha devem coincidir; caso contrário, a requisição deverá ser rejeitada com uma mensagem explicativa.
        ///•	Se o e-mail já estiver registrado, a requisição também deve ser rejeitada, retornando uma mensagem informando o motivo.
        ///•	Se todos os dados estiverem corretos, o usuário deve ser salvo e seu ID retornado na resposta.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public ActionResult CadastrarUsuario(UsuarioViewModel usuarioView)
        {
            try
            {
                if (usuarioView == null)
                {
                    return BadRequest("Requisição inválida.");
                }

                _usuarioService.Cadastrar(usuarioView);

                return Ok("Cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao processar a solicitação.");
            }
        }

        /// <summary>
        ///•	Este endpoint tem como objetivo validar um usuário com base no e-mail e senha fornecidos.
        ///•	O corpo da requisição, é necessário fornecer conter o e-mail e a senha do usuário.
        ///•	Se o corpo da requisição estiver vazio, a requisição deve ser rejeitada com uma mensagem apropriada.
        ///•	Caso as credenciais(e-mail + senha) não correspondam a um usuário existente, rejeite a requisição e retorne uma mensagem explicando o erro.
        ///•	Se o usuário for validado corretamente, gere um token JWT que deverá ser utilizado para autenticação em requisições autenticadas.
        ///•	O E-mail e  Id do usuário deverá estar dentro do token.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpPost("/validar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public ActionResult ValidarUsuario(UsuarioValidarViewModel usuarioView)
        {
            try
            {
                if (usuarioView == null)
                {
                    return BadRequest("Requisição inválida.");
                }

                var token = _usuarioService.Validar(usuarioView);

                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao processar a solicitação.");
            }
        }

    }
}
