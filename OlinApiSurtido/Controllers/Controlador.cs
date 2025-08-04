using MiApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MiApi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentoDetalleController : ControllerBase
    {
        private readonly IConfiguration configuracion;
        public DocumentoDetalleController(IConfiguration configuration) { configuracion = configuration; }
        [HttpGet("{id}")]
        public async Task<ActionResult<GetterDocumentoDetalle>> GetDetallesDocumento(int id)
        {
            string query = "SELECT D.DOCUMENTO_ID, D.NUMERO_DOCUMENTO, D.ID, D.TIPO_DOCTO, D.PRODUCTO, D.NUMERO_DOCUMENTO, UPPER(D.DESCRIPCION)[DESCRIPCION], D.UNIDADES_SURTIDAS, D.CANTIDAD, UPPER(U.ABREVIACION)[ABREVIACION], C.CODIGO_BARRAS FROM DETALLE_DOCUMENTOS D JOIN UNIDADES U ON D.UNIDAD_MEDIDA = U.ID JOIN PRODUCTOS_PRECIOS C ON D.PRODUCTO = C.PRODUCTO AND D.UNIDAD_BASE = C.UNIDAD_MEDIDA_EQUIVALENCIA  WHERE D.DOCUMENTO_ID = @ID ORDER BY D.ID";
            string conexionString = configuracion.GetConnectionString("OlinCeConnection") ?? throw new ArgumentNullException("CONEXION STRING NO ENCONTRADO EN CONFIGURACION");
            List<GetterDocumentoDetalle> detalles = new List<GetterDocumentoDetalle>();
            bool surtido = true;
            using (SqlConnection conexionSQL = new SqlConnection(conexionString))
            {
                using (SqlCommand comandoSQL = new SqlCommand(query, conexionSQL))
                {
                    comandoSQL.Parameters.AddWithValue("@ID", id);
                    try
                    {
                        await conexionSQL.OpenAsync();
                        using (SqlDataReader leectorSQL = await comandoSQL.ExecuteReaderAsync())
                        {
                            if (leectorSQL.HasRows)
                            {
                                foreach (var columna in leectorSQL)
                                {
                                    GetterDocumentoDetalle detalle = new GetterDocumentoDetalle
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
                    catch (SqlException ex) { return StatusCode(500, $"ERROR AL CONECTAR O CONSULTAR LA BASE DE DATOS: {ex.Message}"); }
                    catch (Exception ex) { return StatusCode(500, $"ERROR INTERNO DEL SERVIDOR: {ex.Message}"); }
                }
            }
            if (detalles.Count == 0) { return StatusCode(404, $"NO SE ENCONTRO NINGUN DOCUMENTO CON EL ID: {id}"); }
            if (surtido) { return StatusCode(410, "EL DOCUMENTO YA ESTÁ COMPLETAMENTE SURTIDO."); }
            return Ok(detalles);
        }
        [HttpPost("post")]
        public async Task<IActionResult> SetDetallesDocumento(List<SetterDocumentoDetalle> detallesSetter)
        {
            string query = "UPDATE DETALLE_DOCUMENTOS SET UNIDADES_SURTIDAS = @UNIDADES_SURTIDAS, FECHA_ENTREGA = @FECHA_ENTREGA WHERE DOCUMENTO_ID = @DOCUMENTO_ID AND ID = @ID";
            string conexionString = configuracion.GetConnectionString("OlinCeConnection") ?? throw new ArgumentNullException("CONEXION STRING NO ENCONTRADO EN CONFIGURACION");
            DateTime fechaHoraActual = DateTime.Now;
            Console.WriteLine(detallesSetter.Count);
            if (detallesSetter == null || detallesSetter.Count < 1) { return BadRequest("LA LISTA NO PUEDE ESTAR VACIA"); }
            using (SqlConnection conexionSQL = new SqlConnection(conexionString))
            {
                await conexionSQL.OpenAsync();
                using (SqlTransaction transaccion = conexionSQL.BeginTransaction())
                {
                    try
                    {
                        foreach (SetterDocumentoDetalle detalle in detallesSetter)
                        {
                            using (SqlCommand comandoSQL = new SqlCommand(query, conexionSQL, transaccion))
                            {
                                comandoSQL.Parameters.AddWithValue("@UNIDADES_SURTIDAS", detalle.UNIDADES_SURTIDAS);
                                comandoSQL.Parameters.AddWithValue("@FECHA_ENTREGA", fechaHoraActual);
                                comandoSQL.Parameters.AddWithValue("@DOCUMENTO_ID", detalle.DOCUMENTO_ID);
                                comandoSQL.Parameters.AddWithValue("@ID", detalle.ID);
                                await comandoSQL.ExecuteNonQueryAsync();
                            }
                        }
                        transaccion.Commit();
                        return Ok("Documentos updated successfully.");
                    }
                    catch (SqlException ex)
                    {
                        transaccion.Rollback();
                        return StatusCode(500, $"ERROR AL CONSULTAR BASE DE DATOS: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        transaccion.Rollback();
                        return StatusCode(500, $"ERROR INTERNO DEL SERVIDOR: {ex.Message}");
                    }
                }
            }
        }
    }
} 