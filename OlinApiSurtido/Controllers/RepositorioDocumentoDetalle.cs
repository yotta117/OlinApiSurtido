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

                // Step 2: Get the details if not completed
                var query = from d in _contexto.DetalleDocumentos
                            join u in _contexto.Unidades on d.UNIDAD_MEDIDA equals u.ID
                            join pp in _contexto.ProductosPrecios on new { p = d.PRODUCTO, um = d.UNIDAD_BASE } equals new { p = pp.PRODUCTO, um = pp.UNIDAD_MEDIDA_EQUIVALENCIA }
                            join sd in _contexto.SurtidosDetalle on new { d.DOCUMENTO_ID, d.ID } equals new { sd.DOCUMENTO_ID, sd.ID } into surtidoJoin
                            from sd in surtidoJoin.DefaultIfEmpty() // LEFT JOIN
                            where d.DOCUMENTO_ID == id
                            orderby d.ID
                            select new GetterDocumentoDetalle
                            {
                                ID = d.ID,
                                PRODUCTO = d.PRODUCTO,
                                DESCRIPCION = d.DESCRIPCION != null ? d.DESCRIPCION.ToUpper() : "",
                                SURTIDAS = sd != null ? (float?)sd.SURTIDAS : null,
                                CANTIDAD = d.CANTIDAD,
                                ABREVIACION = u.ABREVIACION != null ? u.ABREVIACION.ToUpper() : "",
                                CODIGO_BARRAS = pp.CODIGO_BARRAS
                            };

                var detalles = await query.ToListAsync();

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
                foreach (var detalleDto in detallesSurtidos)
                {
                    // Use the composite key to find the entity
                    var entidad = await _contexto.SurtidosDetalle.FindAsync(documentoId, detalleDto.ID);

                    if (entidad != null)
                    {
                        // Update existing entity
                        entidad.SURTIDAS = detalleDto.SURTIDAS;
                        entidad.CHECADOR = detalleDto.CHECADOR;
                        entidad.FIN_SURTIDO = DateTime.Now;
                    }
                    else
                    {
                        // Insert new entity, ensuring the composite key is set
                        var nuevaEntidad = new SurtidoDetalle
                        {
                            DOCUMENTO_ID = documentoId,
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