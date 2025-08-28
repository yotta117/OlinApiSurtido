using Microsoft.EntityFrameworkCore;
using MiApi.Models;

namespace MiApi.Context
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
        } 
        public DbSet<DetalleDocumento> DetalleDocumentos { get; set; }
        public DbSet<Unidad> Unidades { get; set; }
        public DbSet<ProductoPrecio> ProductosPrecios { get; set; }
    }
}