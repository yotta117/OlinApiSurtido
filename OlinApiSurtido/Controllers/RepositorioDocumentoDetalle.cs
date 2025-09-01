using MiApi.Context;
using MiApi.Interfaces;
using MiApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace MiApi.Repositories
{
    public class RepositorioDocumentoDetalle : IRepositorioDocumentoDetalle
    {
        private readonly Contexto _contexto;

        public RepositorioDocumentoDetalle(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<(List<GetterDocumentoDetalle>? Detalles, bool EsSurtido, string ErrorMessage)> GetDetallesPorDocumentoIdAsync(int id)
        {
            string errorMessage = string.Empty;
            try
            {
                // Step 1: Check the header table
                var surtidoEncabezado = await _contexto.SurtidosEncabezado.FindAsync(id);
                if (surtidoEncabezado != null && surtidoEncabezado.COMPLETADO)
                {
                    return (new List<GetterDocumentoDetalle>(), true, "El documento ya está completamente surtido.");
                }

                // Step 2: Refactored query using explicit LEFT JOINs to match the SQL query.
                var query = from d in _contexto.DetalleDocumentos
                            where d.DOCUMENTO_ID == id

                            join sd_join in _contexto.SurtidosDetalle on d.ID equals sd_join.ID into sd_group
                            from sd in sd_group.DefaultIfEmpty() // LEFT JOIN

                            join pp_join in _contexto.ProductosPrecios
                                on new { ProductId = d.PRODUCTO, UnitId = d.UNIDAD_BASE }
                                equals new { ProductId = pp_join.PRODUCTO, UnitId = pp_join.UNIDAD_MEDIDA_EQUIVALENCIA }
                                into pp_group
                            from pp in pp_group.DefaultIfEmpty() // LEFT JOIN

                            join u_join in _contexto.Unidades on d.UNIDAD_MEDIDA equals u_join.ID into u_group
                            from u in u_group.DefaultIfEmpty() // LEFT JOIN

                            orderby d.ID
                            select new GetterDocumentoDetalle
                            {
                                ID = d.ID,
                                PRODUCTO = d.PRODUCTO,
                                DESCRIPCION = d.DESCRIPCION != null ? d.DESCRIPCION.ToUpper() : "",
                                SURTIDAS = sd != null ? (float?)sd.SURTIDAS : null,
                                CANTIDAD = d.CANTIDAD,
                                ABREVIACION = u != null && u.ABREVIACION != null ? u.ABREVIACION.ToUpper() : null,
                                CODIGO_BARRAS = pp != null ? pp.CODIGO_BARRAS : null
                            };

                var detalles = await query.ToListAsync();

                if (detalles == null || detalles.Count == 0)
                {
                    return (null, false, $"No se encontró ningún documento con el ID: {id}");
                }

                // Step 3: Validate that all required fields have non-null values.
                foreach (var detalle in detalles)
                {
                    if (detalle.SURTIDAS == null)
                    {
                        return (null, false, $"Datos incompletos: El producto con ID {detalle.PRODUCTO} (Detalle ID: {detalle.ID}) no tiene valor de 'SURTIDAS'.");
                    }
                    if (detalle.ABREVIACION == null)
                    {
                        return (null, false, $"Datos incompletos: El producto con ID {detalle.PRODUCTO} (Detalle ID: {detalle.ID}) no tiene valor de 'ABREVIACION'.");
                    }
                    if (detalle.CODIGO_BARRAS == null)
                    {
                        return (null, false, $"Datos incompletos: El producto con ID {detalle.PRODUCTO} (Detalle ID: {detalle.ID}) no tiene valor de 'CODIGO_BARRAS'.");
                    }
                }

                return (detalles, false, errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error interno del servidor: {ex.Message}";
                return (null, false, errorMessage);
            }
        }

        public async Task<bool> ActualizarSurtidoAsync(int documentoId, List<SetterSurtidos_Detalle> detallesSurtidos)
        {
            using var transaction = await _contexto.Database.BeginTransactionAsync();
            try
            {
                var validIds = await _contexto.DetalleDocumentos
                                              .Where(d => d.DOCUMENTO_ID == documentoId)
                                              .Select(d => d.ID)
                                              .ToListAsync();

                foreach (var detalleDto in detallesSurtidos)
                {
                    // Validation: Ensure the detail ID belongs to the document ID
                    if (!validIds.Contains(detalleDto.ID))
                    {
                        // ID from DTO does not belong to the document in the URL, rollback and fail.
                        await transaction.RollbackAsync();
                        return false;
                    }

                    var entidad = await _contexto.SurtidosDetalle.FindAsync(detalleDto.ID);

                    if (entidad != null)
                    {
                        // Update existing entity
                        entidad.SURTIDAS = detalleDto.SURTIDAS;
                        entidad.CHECADOR = detalleDto.CHECADOR;
                        entidad.FIN_SURTIDO = DateTime.Now;
                    }
                    else
                    {
                        // Insert new entity
                        var nuevaEntidad = new SurtidoDetalle
                        {
                            ID = detalleDto.ID,
                            SURTIDAS = detalleDto.SURTIDAS,
                            CHECADOR = detalleDto.CHECADOR,
                            FIN_SURTIDO = DateTime.Now
                        };
                        _contexto.SurtidosDetalle.Add(nuevaEntidad);
                    }
                }

                await _contexto.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}