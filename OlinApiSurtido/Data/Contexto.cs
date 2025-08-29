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
        public DbSet<SurtidoEncabezado> SurtidosEncabezado { get; set; }
        public DbSet<SurtidoDetalle> SurtidosDetalle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DetalleDocumento>()
                .HasKey(dd => new { dd.DOCUMENTO_ID, dd.ID });

            modelBuilder.Entity<ProductoPrecio>()
                .HasKey(pp => new { pp.PRODUCTO, pp.UNIDAD_MEDIDA_EQUIVALENCIA });

            modelBuilder.Entity<SurtidoDetalle>()
                .HasKey(sd => new { sd.DOCUMENTO_ID, sd.ID });

            // Define the one-to-one relationship between DetalleDocumento and SurtidoDetalle
            modelBuilder.Entity<DetalleDocumento>()
                .HasOne<SurtidoDetalle>()
                .WithOne()
                .HasForeignKey<SurtidoDetalle>(sd => new { sd.DOCUMENTO_ID, sd.ID });
        }
    }
}