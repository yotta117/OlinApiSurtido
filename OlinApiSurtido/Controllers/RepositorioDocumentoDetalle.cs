using MiApi.Context;
using MiApi.Interfaces;
using MiApi.Models;
using Microsoft.EntityFrameworkCore;

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
                var query = from d in _contexto.DetalleDocumentos
                            join u in _contexto.Unidades on d.UNIDAD_MEDIDA equals u.ID
                            join c in _contexto.ProductosPrecios on new { p = d.PRODUCTO, um = d.UNIDAD_BASE } equals new { p = c.PRODUCTO, um = c.UNIDAD_MEDIDA_EQUIVALENCIA }
                            where d.DOCUMENTO_ID == id
                            orderby d.ID
                            select new GetterDocumentoDetalle
                            {
                                DOCUMENTO_ID = d.DOCUMENTO_ID,
                                TIPO_DOCTO = d.TIPO_DOCTO,
                                NUMERO_DOCUMENTO = d.NUMERO_DOCUMENTO ?? string.Empty,
                                ID = d.ID,
                                PRODUCTO = d.PRODUCTO,
                                DESCRIPCION = d.DESCRIPCION != null ? d.DESCRIPCION.ToUpper() : string.Empty,
                                UNIDADES_SURTIDAS = d.UNIDADES_SURTIDAS,
                                CANTIDAD = d.CANTIDAD,
                                ABREVIACION = u.ABREVIACION != null ? u.ABREVIACION.ToUpper() : string.Empty,
                                CODIGO_BARRAS = c.CODIGO_BARRAS
                            };

                var detalles = await query.ToListAsync();

                if (detalles == null || detalles.Count == 0)
                {
                    return (null, false, $"No se encontró ningún documento con el ID: {id}");
                }

                bool esSurtido = detalles.All(d => d.UNIDADES_SURTIDAS >= d.CANTIDAD);

                return (detalles, esSurtido, errorMessage);
            }
            catch (Exception ex)
            {
                errorMessage = $"Error interno del servidor: {ex.Message}";
                return (null, false, errorMessage);
            }
        }

        public async Task<bool> ActualizarDetallesAsync(List<SetterDocumentoDetalle> detallesSetter, DateTime fechaHoraActual)
        {
            try
            {
                foreach (var detalle in detallesSetter)
                {
                    var entidad = await _contexto.DetalleDocumentos.FindAsync(detalle.DOCUMENTO_ID, detalle.ID);
                    if (entidad != null)
                    {
                        entidad.UNIDADES_SURTIDAS = detalle.UNIDADES_SURTIDAS;
                        entidad.FECHA_ENTREGA = fechaHoraActual;
                    }
                }
                int lineasAfectadas = await _contexto.SaveChangesAsync();
                return lineasAfectadas >= detallesSetter.Count;
            }
            catch
            {
                return false;
            }
        }
    }
}