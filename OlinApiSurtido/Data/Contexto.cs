using Microsoft.EntityFrameworkCore;
using MiApi.Models;

namespace MiApi.Context
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {
        } 
        public DbSet<DocumentoDetalle> Detalle { get; set; }
    }
}