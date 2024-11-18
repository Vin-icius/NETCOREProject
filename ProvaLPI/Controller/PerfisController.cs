using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvaLPI.Domain;
using ProvaLPI.Service;
using ProvaLPI.ViewModel;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace ProvaLPI.Controller
{
    /// <summary>
    /// •    Este Controller gerencia os perfis.
    /// </summary>
    [Authorize("APIAuth")]
    [Route("api/[controller]")]
    [ApiController]
    public class PerfisController : ControllerBase
    {

        private readonly PerfilService _perfilService;

        public PerfisController(PerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        /// <summary>
        ///•	O objetivo deste endpoint é retornar todos os perfis associados ao usuário.
        ///•	Cada perfil possui um nome e um tipo(normal ou infantil). O Tipo deverá ser um ENUM.
        ///•	A resposta deve incluir todos os perfis vinculados ao usuário.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpGet("/{usuarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult ListarPerfis(int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return BadRequest("Requisição inválida.");
                }

                var lista = _perfilService.Listar(usuarioId);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao processar a solicitação.");
            }
        }
        
        /// <summary>
        ///•	No corpo da requisição, devem ser enviados o nome do perfil e seu tipo (normal ou infantil).
        ///•	Caso já exista um perfil com o mesmo nome, a requisição deverá ser rejeitada com uma mensagem informando o motivo da recusa.
        ///•	Outra regra é o limite de 4 perfis por usuário.Caso o perfil ultrapassar o limite a requisição deve ser rejeitada com uma mensagem informando o motivo da recusa.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpPost("/{usuarioId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult AdicionarPerfil(int usuarioId, PerfilViewModel perfilView)
        {
            try
            {
                if (perfilView == null)
                {
                    return BadRequest("Requisição inválida.");
                }

                _perfilService.Inserir(usuarioId,perfilView);
                return Ok("Perfil inserido com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao processar a solicitação.");
            }
        }
        
        /// <summary>
        ///•	Este endpoint é utilizado para atualizar apenas o nome de um perfil.
        ///•	O corpo da requisição deve incluir o novo nome do perfil.
        ///•	Caso já exista outro perfil com o mesmo nome(exceto o perfil sendo alterado), a requisição deve ser rejeitada, retornando uma mensagem informando o motivo da recusa.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpPatch("/{usuarioId}/{perfilId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult AtualizarPerfil(int usuarioId,int perfilId, PerfilAlterarViewModel perfilView)
        {
            try
            {
                if (perfilView == null)
                {
                    return BadRequest("Requisição inválida.");
                }

                _perfilService.Atualizar(usuarioId,perfilId,perfilView);
                return Ok("Perfil alterado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao processar a solicitação.");
            }
        }


    }
}
