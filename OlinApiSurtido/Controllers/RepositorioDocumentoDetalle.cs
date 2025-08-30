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

                // Step 2: Get the details if not completed, using Include for related data
                var detalles = await _contexto.DetalleDocumentos
                    .Include(d => d.SurtidoDetalle) // Eager load the related SurtidoDetalle
                    .Where(d => d.DOCUMENTO_ID == id)
                    .OrderBy(d => d.ID)
                    .Select(d => new GetterDocumentoDetalle
                    {
                        ID = d.ID,
                        PRODUCTO = d.PRODUCTO,
                        DESCRIPCION = d.DESCRIPCION != null ? d.DESCRIPCION.ToUpper() : "",
                        SURTIDAS = d.SurtidoDetalle != null ? (float?)d.SurtidoDetalle.SURTIDAS : null,
                        CANTIDAD = d.CANTIDAD,
                        // The following joins are still needed as they are not on the main entity
                        ABREVIACION = _contexto.Unidades
                                        .Where(u => u.ID == d.UNIDAD_MEDIDA)
                                        .Select(u => u.ABREVIACION != null ? u.ABREVIACION.ToUpper() : "")
                                        .FirstOrDefault() ?? "",
                        CODIGO_BARRAS = _contexto.ProductosPrecios
                                        .Where(pp => pp.PRODUCTO == d.PRODUCTO && pp.UNIDAD_MEDIDA_EQUIVALENCIA == d.UNIDAD_BASE)
                                        .Select(pp => pp.CODIGO_BARRAS)
                                        .FirstOrDefault()
                    })
                    .ToListAsync();

                if (detalles == null || detalles.Count == 0)
                {
                    return (null, false, $"No se encontró ningún documento con el ID: {id}");
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