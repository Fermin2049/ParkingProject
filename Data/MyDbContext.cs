using System;
using System.Collections.Generic;
using FinalMarzo.net.Data.Models;
using FinalMarzo.net.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Data
{
    public partial class MyDbContext : DbContext
    {
        public MyDbContext() { }

        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options) { }

        public virtual DbSet<Cliente> Clientes { get; set; }
        public virtual DbSet<Espaciosestacionamiento> Espaciosestacionamientos { get; set; }
        public virtual DbSet<Pago> Pagos { get; set; }
        public virtual DbSet<Reserva> Reservas { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseMySql(
                    connectionString,
                    Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.28-mariadb")
                );
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_general_ci").HasCharSet("utf8mb4");

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.IdCliente).HasName("PRIMARY");

                entity.ToTable("clientes");

                entity.HasIndex(e => e.Email, "email").IsUnique();
                entity.HasIndex(e => e.Telefono, "telefono").IsUnique();
                entity.HasIndex(e => e.VehiculoPlaca, "vehiculoPlaca").IsUnique();

                entity
                    .Property(e => e.IdCliente)
                    .HasColumnType("int(11)")
                    .HasColumnName("idCliente");
                entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
                entity
                    .Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("current_timestamp()")
                    .HasColumnType("datetime")
                    .HasColumnName("fechaRegistro");
                entity.Property(e => e.Nombre).HasMaxLength(100).HasColumnName("nombre");
                entity.Property(e => e.Telefono).HasMaxLength(20).HasColumnName("telefono");
                entity
                    .Property(e => e.VehiculoPlaca)
                    .HasMaxLength(20)
                    .HasColumnName("vehiculoPlaca");
            });

            modelBuilder.Entity<Espaciosestacionamiento>(entity =>
            {
                entity.HasKey(e => e.IdEspacio).HasName("PRIMARY");

                entity.ToTable("espaciosestacionamiento");

                entity.HasIndex(e => e.NumeroEspacio, "numeroEspacio").IsUnique();

                entity
                    .Property(e => e.IdEspacio)
                    .HasColumnType("int(11)")
                    .HasColumnName("idEspacio");
                entity
                    .Property(e => e.FechaActualizacion)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("current_timestamp()")
                    .HasColumnType("datetime")
                    .HasColumnName("fechaActualizacion");
                entity
                    .Property(e => e.NumeroEspacio)
                    .HasColumnType("int(11)")
                    .HasColumnName("numeroEspacio");
                entity.Property(e => e.Sector).HasMaxLength(50).HasColumnName("sector");
            });

            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.IdPago).HasName("PRIMARY");

                entity.ToTable("pagos");

                entity.HasIndex(e => e.IdCliente, "idCliente1");
                entity.HasIndex(e => e.IdReserva, "fk_pagos_reserva");

                entity.Property(e => e.IdPago).HasColumnType("int(11)").HasColumnName("idPago");
                entity
                    .Property(e => e.FechaPago)
                    .HasDefaultValueSql("current_timestamp()")
                    .HasColumnType("datetime")
                    .HasColumnName("fechaPago");
                entity
                    .Property(e => e.IdCliente)
                    .HasColumnType("int(11)")
                    .HasColumnName("idCliente");
                entity
                    .Property(e => e.IdReserva)
                    .HasColumnType("int(11)")
                    .HasColumnName("idReserva");
                entity
                    .Property(e => e.MetodoPago)
                    .HasColumnType("enum('Efectivo','Tarjeta','Transferencia')")
                    .HasColumnName("metodoPago");
                entity.Property(e => e.Monto).HasColumnType("decimal(10,2)").HasColumnName("monto");
                entity
                    .Property(e => e.Estado)
                    .HasColumnType("enum('Exitoso','Fallido')")
                    .HasColumnName("estado");
            });

            modelBuilder.Entity<Reserva>(entity =>
            {
                entity.HasKey(e => e.IdReserva).HasName("PRIMARY");

                entity.ToTable("reservas");

                entity.HasIndex(e => e.IdCliente, "idCliente2");
                entity.HasIndex(e => e.IdEspacio, "idEspacio1");

                entity
                    .Property(e => e.IdReserva)
                    .HasColumnType("int(11)")
                    .HasColumnName("idReserva");
                entity
                    .Property(e => e.FechaReserva)
                    .HasColumnType("datetime")
                    .HasColumnName("fechaReserva");
                entity
                    .Property(e => e.FechaExpiracion)
                    .HasColumnType("datetime")
                    .HasColumnName("fechaExpiracion");
                entity
                    .Property(e => e.IdCliente)
                    .HasColumnType("int(11)")
                    .HasColumnName("idCliente");
                entity
                    .Property(e => e.IdEspacio)
                    .HasColumnType("int(11)")
                    .HasColumnName("idEspacio");
                entity
                    .Property(e => e.Estado)
                    .HasColumnType("enum('Activa','Cancelada','Expirada')")
                    .HasColumnName("estado");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario).HasName("PRIMARY");

                entity.ToTable("usuarios");

                entity.HasIndex(e => e.Email, "email1").IsUnique();

                entity
                    .Property(e => e.IdUsuario)
                    .HasColumnType("int(11)")
                    .HasColumnName("idUsuario");
                entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
                entity
                    .Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("current_timestamp()")
                    .HasColumnType("datetime")
                    .HasColumnName("fechaRegistro");
                entity.Property(e => e.Nombre).HasMaxLength(100).HasColumnName("nombre");
                entity.Property(e => e.Contrasena).HasMaxLength(255).HasColumnName("contrasena");
                entity
                    .Property(e => e.Rol)
                    .HasColumnType("enum('Administrador','Empleado')")
                    .HasColumnName("rol");
                entity
                    .Property(e => e.Estado)
                    .HasColumnType("enum('Activo','Inactivo')")
                    .HasColumnName("estado");
            });
        }
    }
}
