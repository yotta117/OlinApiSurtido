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

        [HttpPost]
        public async Task<IActionResult> SetDetallesDocumento([FromBody] List<SetterDocumentoDetalle> detallesSetter)
        {
            if (detallesSetter == null || detallesSetter.Count == 0) { return BadRequest(new { message = "La lista de detalles no puede estar vacía." }); }
            try
            {
                DateTime fechaHoraActual = DateTime.Now;
                bool updateSuccess = await repositorio.ActualizarDetallesAsync(detallesSetter, fechaHoraActual);
                if (!updateSuccess) { return StatusCode(500, new { message = "No se pudieron actualizar todos los detalles." }); }
                return Ok(new { message = "Detalles actualizados exitosamente.", fechaActualizacion = fechaHoraActual });
            }
            catch (SqlException ex) { return StatusCode(500, new { message = $"Error de base de datos: {ex.Message}" }); }
            catch (Exception ex) { return StatusCode(500, new { message = $"Error interno del servidor: {ex.Message}" }); }
        }
    }
}