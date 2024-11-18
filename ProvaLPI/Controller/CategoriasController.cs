using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProvaLPI.Domain;
using ProvaLPI.Service;

namespace ProvaLPI.Controller
{
    /// <summary>
    /// •    Este Controller gerencia as categorias.
    /// </summary>
    [Authorize("APIAuth")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaService _categoriaService;

        public CategoriasController(CategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        /// <summary>
        ///•	Este endpoint tem como objetivo retornar as categorias dos conteúdos disponíveis.
        ///•	As categorias podem ser filtradas pelo parâmetro query, se fornecido.
        ///•	A resposta deve incluir todas as categorias encontradas.
        /// </summary>
        /// <returns>
        /// •	Retorna 200, 400 ou 404.
        /// </returns>
        [HttpGet]
        public IActionResult ObterCategorias([FromQuery] string? query = null)
        {
            try
            {
                var categorias = _categoriaService.ListarCategorias(query);
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
