using MiApi.Interfaces;
using MiApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MiApi.Repositories
{
    public class RepositorioDocumentoDetalle : IRepositorioDocumentoDetalle
    {
        private readonly IConfiguration configuracion;

        public RepositorioDocumentoDetalle(IConfiguration configuracion)
        {
            this.configuracion = configuracion;
        }

        public async Task<(List<GetterDocumentoDetalle>? Detalles, bool EsSurtido, string ErrorMessage)> GetDetallesPorDocumentoIdAsync(int id)
        {
            var detalles = new List<GetterDocumentoDetalle>();
            bool surtido = true;
            string errorMessage = string.Empty;
            //TODO RETRABAJAR LAS QUERY PARA QUE CONSULTE PRIMERO EL ENCABEZADO Y LUEGO LOS DETALLES
            string query = "SELECT D.DOCUMENTO_ID, D.NUMERO_DOCUMENTO, D.ID, D.TIPO_DOCTO, D.PRODUCTO, D.NUMERO_DOCUMENTO, UPPER(D.DESCRIPCION)[DESCRIPCION], D.UNIDADES_SURTIDAS, D.CANTIDAD, UPPER(U.ABREVIACION)[ABREVIACION], C.CODIGO_BARRAS FROM DETALLE_DOCUMENTOS D JOIN UNIDADES U ON D.UNIDAD_MEDIDA = U.ID JOIN PRODUCTOS_PRECIOS C ON D.PRODUCTO = C.PRODUCTO AND D.UNIDAD_BASE = C.UNIDAD_MEDIDA_EQUIVALENCIA  WHERE D.DOCUMENTO_ID = @ID ORDER BY D.ID";
            string conexionString = configuracion.GetConnectionString("OlinCeConnection") ?? throw new ArgumentNullException("Connection string not found.");

            using (SqlConnection conexionSQL = new SqlConnection(conexionString))
            {
                using (SqlCommand comandoSQL = new SqlCommand(query, conexionSQL))
                {
                    comandoSQL.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                    try
                    {
                        await conexionSQL.OpenAsync();
                        using (SqlDataReader leectorSQL = await comandoSQL.ExecuteReaderAsync())
                        {
                            if (leectorSQL.HasRows)
                            {
                                while (await leectorSQL.ReadAsync())
                                {
                                    var detalle = new GetterDocumentoDetalle
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
                                    if (detalle.UNIDADES_SURTIDAS < detalle.CANTIDAD)
                                    {
                                        surtido = false;
                                    }
                                    detalles.Add(detalle);
                                }
                            }
                            else
                            {
                                errorMessage = $"No se encontró ningún documento con el ID: {id}";
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        errorMessage = $"Error de base de datos: {ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Error interno: {ex.Message}";
                    }
                }
            }
            return (detalles, surtido, errorMessage);
        }

        public async Task<bool> ActualizarDetallesAsync(List<SetterDocumentoDetalle> detallesSetter, DateTime fechaHoraActual)
        {
            string query = "UPDATE DETALLE_DOCUMENTOS SET UNIDADES_SURTIDAS = @UNIDADES_SURTIDAS, FECHA_ENTREGA = @FECHA_ENTREGA WHERE DOCUMENTO_ID = @DOCUMENTO_ID AND ID = @ID";
            string conexionString = configuracion.GetConnectionString("OlinCeConnection") ?? throw new ArgumentNullException("Connection string not found.");
            int lineasAfectadas = 0;

            using (SqlConnection conexionSQL = new SqlConnection(conexionString))
            {
                await conexionSQL.OpenAsync();
                using (SqlTransaction transaccion = conexionSQL.BeginTransaction())
                {
                    try
                    {
                        foreach (var detalle in detallesSetter)
                        {
                            using (SqlCommand comandoSQL = new SqlCommand(query, conexionSQL, transaccion))
                            {
                                comandoSQL.Parameters.Add(new SqlParameter("@UNIDADES_SURTIDAS", SqlDbType.Float) { Value = detalle.UNIDADES_SURTIDAS });
                                comandoSQL.Parameters.Add(new SqlParameter("@FECHA_ENTREGA", SqlDbType.DateTime) { Value = fechaHoraActual });
                                comandoSQL.Parameters.Add(new SqlParameter("@DOCUMENTO_ID", SqlDbType.Int) { Value = detalle.DOCUMENTO_ID });
                                comandoSQL.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = detalle.ID });

                                lineasAfectadas += await comandoSQL.ExecuteNonQueryAsync();
                            }
                        }
                        transaccion.Commit();
                        return lineasAfectadas == detallesSetter.Count;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}