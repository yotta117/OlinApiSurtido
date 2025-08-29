using MiApi.Models;

namespace MiApi.Interfaces
{
    public interface IRepositorioDocumentoDetalle
    {
        Task<(List<GetterDocumentoDetalle>? Detalles, bool EsSurtido, string ErrorMessage)> GetDetallesPorDocumentoIdAsync(int id);
        Task<bool> ActualizarSurtidoAsync(int documentoId, List<SetterSurtidos_Detalle> detallesSurtidos);
    }
}