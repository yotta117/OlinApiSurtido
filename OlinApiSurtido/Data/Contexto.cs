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
                .HasKey(dd => dd.ID);

            modelBuilder.Entity<ProductoPrecio>()
                .HasKey(pp => new { pp.PRODUCTO, pp.UNIDAD_MEDIDA_EQUIVALENCIA });

            modelBuilder.Entity<SurtidoDetalle>()
                .HasKey(sd => sd.ID);

            // Define the one-to-one relationship using the navigation properties.
            // This explicitly tells EF Core that a DetalleDocumento has one SurtidoDetalle,
            // and a SurtidoDetalle has one DetalleDocumento, and the foreign key is on
            // SurtidoDetalle.ID, which points to DetalleDocumento.ID.
            modelBuilder.Entity<DetalleDocumento>()
                .HasOne(d => d.SurtidoDetalle)
                .WithOne(s => s.DetalleDocumento)
                .HasForeignKey<SurtidoDetalle>(s => s.ID);
        }
    }
}