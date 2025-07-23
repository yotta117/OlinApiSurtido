using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MiApi.Models;

namespace MiApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoDetalleController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public DocumentoDetalleController(IConfiguration configuration) { _configuration = configuration; }
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentoDetalle>> GetDocumento(int id)
        {
            string? connectionString = _configuration.GetConnectionString("OlinCeConnection");
            List<DocumentoDetalle> detalles = new List<DocumentoDetalle>();
            bool surtido = true;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT D.DOCUMENTO_ID, D.NUMERO_DOCUMENTO, D.ID, D.TIPO_DOCTO, D.PRODUCTO, D.NUMERO_DOCUMENTO, UPPER(D.DESCRIPCION)[DESCRIPCION], D.UNIDADES_SURTIDAS, D.CANTIDAD, UPPER(U.ABREVIACION)[ABREVIACION], C.CODIGO_BARRAS FROM DETALLE_DOCUMENTOS D JOIN UNIDADES U ON D.UNIDAD_MEDIDA = U.ID JOIN PRODUCTOS_PRECIOS C ON D.PRODUCTO = C.PRODUCTO AND D.UNIDAD_BASE = C.UNIDAD_MEDIDA_EQUIVALENCIA  WHERE D.DOCUMENTO_ID = @ID ORDER BY D.ID";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID", id);
                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader leectorSQL = await command.ExecuteReaderAsync())
                        {
                            if (leectorSQL.HasRows)
                            {
                                foreach (var columna in leectorSQL)
                                {
                                    DocumentoDetalle detalle = new DocumentoDetalle
                                    {
                                        DOCUMENTO_ID = id,
                                        TIPO_DOCTO = leectorSQL.GetInt32(leectorSQL.GetOrdinal("TIPO_DOCTO")),
                                        NUMERO_DOCUMENTO = leectorSQL.GetString(leectorSQL.GetOrdinal("NUMERO_DOCUMENTO")),
                                        ID = leectorSQL.GetInt32(leectorSQL.GetOrdinal("ID")),
                                        PRODUCTO = leectorSQL.GetInt32(leectorSQL.GetOrdinal("PRODUCTO")),
                                        DESCPRIPCION = leectorSQL.GetString(leectorSQL.GetOrdinal("DESCRIPCION")),
                                        UNIDADES_SURTIDAS = leectorSQL.GetFloat(leectorSQL.GetOrdinal("UNIDADES_SURTIDAS")),
                                        CANTIDAD = leectorSQL.GetFloat(leectorSQL.GetOrdinal("CANTIDAD")),
                                        ABREVIACION = leectorSQL.GetString(leectorSQL.GetOrdinal("ABREVIACION")),
                                        CODIGO_BARRAS = leectorSQL.GetString(leectorSQL.GetOrdinal("CODIGO_BARRAS"))
                                    };
                                    if (detalle.UNIDADES_SURTIDAS < detalle.CANTIDAD) { surtido = false; }
                                    detalles.Add(detalle);
                                }
                            }
                        }
                    }
                    catch (SqlException ex) { return StatusCode(500, $"Error al conectar o consultar la base de datos: {ex.Message}"); }
                    catch (Exception ex) { return StatusCode(500, $"Error interno del servidor: {ex.Message}"); }
                }
            }
            if (detalles.Count == 0) { return StatusCode(404, $"No se encontró ningún documento con el ID: {id}"); }
            if (surtido) { return StatusCode(204, "El documento ya está completamente surtido."); }
            return Ok(detalles);
        }
    }
}