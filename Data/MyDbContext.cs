using FinalMarzo.net.Data.Models;
using FinalMarzo.net.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Data
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Espaciosestacionamiento> EspaciosEstacionamiento { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public object Espaciosestacionamientos { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.IdCliente);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Telefono).IsUnique();
                entity.HasIndex(e => e.VehiculoPlaca).IsUnique();
            });

            modelBuilder.Entity<Espaciosestacionamiento>(entity =>
            {
                entity.HasKey(e => e.IdEspacio);
                entity.HasIndex(e => e.NumeroEspacio).IsUnique();
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.IdPago);
                entity
                    .HasOne(d => d.IdClienteNavigation)
                    .WithMany(p => p.Pagos)
                    .HasForeignKey(d => d.IdCliente)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasOne(d => d.IdReservaNavigation)
                    .WithMany(p => p.Pagos)
                    .HasForeignKey(d => d.IdReserva)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.IdReserva);
                entity
                    .HasOne(d => d.IdClienteNavigation)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.IdCliente)
                    .OnDelete(DeleteBehavior.Cascade);
                entity
                    .HasOne(d => d.IdEspacioNavigation)
                    .WithMany(p => p.Reservas)
                    .HasForeignKey(d => d.IdEspacio)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario);
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
