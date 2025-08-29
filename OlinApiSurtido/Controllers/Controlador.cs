using MiApi.Interfaces;
using MiApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace MiApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoDetalleController : ControllerBase
    {
        private readonly IRepositorioDocumentoDetalle repositorio;
        public DocumentoDetalleController(IRepositorioDocumentoDetalle repository) { repositorio = repository; }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<GetterDocumentoDetalle>>> GetDetallesDocumento(int id)
        {

            var (detalles, esSurtido, errorMessage) = await repositorio.GetDetallesPorDocumentoIdAsync(id);
            if (!string.IsNullOrEmpty(errorMessage)) return StatusCode(500, new { message = errorMessage });
            if (detalles == null || detalles.Count == 0) return NotFound(new { message = $"No se encontró ningún documento con el ID: {id}" });
            if (esSurtido) return Conflict(new { message = "El documento ya está completamente surtido." }); 
            return Ok(detalles);
        }

        [HttpPatch("{documentoId}")]
        public async Task<IActionResult> UpdateSurtidoDetalle(int documentoId, [FromBody] List<SetterSurtidos_Detalle> detallesSurtidos)
        {
            if (detallesSurtidos == null || detallesSurtidos.Count == 0)
            {
                return BadRequest(new { message = "La lista de detalles no puede estar vacía." });
            }
            try
            {
                // The repository will now handle the logic, including the date.
                bool updateSuccess = await repositorio.ActualizarSurtidoAsync(documentoId, detallesSurtidos);
                if (!updateSuccess)
                {
                    return StatusCode(500, new { message = "No se pudieron actualizar todos los detalles." });
                }
                return Ok(new { message = "Detalles de surtido actualizados exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" });
            }
        }
    }
}