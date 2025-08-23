using MiApi.Models;

namespace MiApi.Interfaces
{
    public interface IRepositorioDocumentoDetalle
    {
        Task<(List<GetterDocumentoDetalle>? Detalles, bool EsSurtido, string ErrorMessage)> GetDetallesPorDocumentoIdAsync(int id);
        Task<bool> ActualizarDetallesAsync(List<SetterDocumentoDetalle> detallesSetter, DateTime fechaHoraActual);
    }
}