﻿// <auto-generated />
using System;
using FinalMarzo.net.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FinalMarzo.net.Migrations
{
    [DbContext(typeof(MyDbContext))]
    partial class MyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseCollation("utf8mb4_general_ci")
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.HasCharSet(modelBuilder, "utf8mb4");
            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("FinalMarzo.net.Models.Cliente", b =>
                {
                    b.Property<int>("IdCliente")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idCliente");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdCliente"));

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("email");

                    b.Property<DateTime?>("FechaRegistro")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("fechaRegistro")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("nombre");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ResetToken")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ResetTokenExpiry")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Telefono")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
                        .HasColumnName("telefono");

                    b.Property<string>("VehiculoPlaca")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)")
                        .HasColumnName("vehiculoPlaca");

                    b.HasKey("IdCliente")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "Email" }, "email")
                        .IsUnique();

                    b.HasIndex(new[] { "Telefono" }, "telefono")
                        .IsUnique();

                    b.HasIndex(new[] { "VehiculoPlaca" }, "vehiculoPlaca")
                        .IsUnique();

                    b.ToTable("clientes", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Espaciosestacionamiento", b =>
                {
                    b.Property<int>("IdEspacio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idEspacio");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdEspacio"));

                    b.Property<string>("Estado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Disponible','Ocupado','Reservado')")
                        .HasColumnName("estado")
                        .HasDefaultValueSql("'Disponible'");

                    b.Property<DateTime?>("FechaActualizacion")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime")
                        .HasColumnName("fechaActualizacion")
                        .HasDefaultValueSql("current_timestamp()");

                    MySqlPropertyBuilderExtensions.UseMySqlComputedColumn(b.Property<DateTime?>("FechaActualizacion"));

                    b.Property<int>("NumeroEspacio")
                        .HasColumnType("int(11)")
                        .HasColumnName("numeroEspacio");

                    b.Property<string>("Sector")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("sector");

                    b.Property<string>("TipoEspacio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Normal','Discapacitados')")
                        .HasColumnName("tipoEspacio")
                        .HasDefaultValueSql("'Normal'");

                    b.HasKey("IdEspacio")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "NumeroEspacio" }, "numeroEspacio")
                        .IsUnique();

                    b.ToTable("espaciosestacionamiento", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Historialestacionamiento", b =>
                {
                    b.Property<int>("IdHistorial")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idHistorial");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdHistorial"));

                    b.Property<string>("Estado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Activo','Completado')")
                        .HasColumnName("estado")
                        .HasDefaultValueSql("'Activo'");

                    b.Property<DateTime>("FechaEntrada")
                        .HasColumnType("datetime")
                        .HasColumnName("fechaEntrada");

                    b.Property<DateTime?>("FechaSalida")
                        .HasColumnType("datetime")
                        .HasColumnName("fechaSalida");

                    b.Property<int>("IdCliente")
                        .HasColumnType("int(11)")
                        .HasColumnName("idCliente");

                    b.Property<int>("IdEspacio")
                        .HasColumnType("int(11)")
                        .HasColumnName("idEspacio");

                    b.HasKey("IdHistorial")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "IdCliente" }, "idCliente");

                    b.HasIndex(new[] { "IdEspacio" }, "idEspacio");

                    b.ToTable("historialestacionamiento", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Pago", b =>
                {
                    b.Property<int>("IdPago")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idPago");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdPago"));

                    b.Property<string>("Estado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Exitoso','Fallido')")
                        .HasColumnName("estado")
                        .HasDefaultValueSql("'Exitoso'");

                    b.Property<DateTime?>("FechaPago")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("fechaPago")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<int>("IdCliente")
                        .HasColumnType("int(11)")
                        .HasColumnName("idCliente");

                    b.Property<string>("MetodoPago")
                        .IsRequired()
                        .HasColumnType("enum('Efectivo','Tarjeta','Transferencia')")
                        .HasColumnName("metodoPago");

                    b.Property<decimal>("Monto")
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("monto");

                    b.HasKey("IdPago")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "IdCliente" }, "idCliente")
                        .HasDatabaseName("idCliente1");

                    b.ToTable("pagos", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Reserva", b =>
                {
                    b.Property<int>("IdReserva")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idReserva");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdReserva"));

                    b.Property<string>("Estado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Activa','Cancelada','Expirada')")
                        .HasColumnName("estado")
                        .HasDefaultValueSql("'Activa'");

                    b.Property<DateTime>("FechaExpiracion")
                        .HasColumnType("datetime")
                        .HasColumnName("fechaExpiracion");

                    b.Property<DateTime>("FechaReserva")
                        .HasColumnType("datetime")
                        .HasColumnName("fechaReserva");

                    b.Property<int>("IdCliente")
                        .HasColumnType("int(11)")
                        .HasColumnName("idCliente");

                    b.Property<int>("IdEspacio")
                        .HasColumnType("int(11)")
                        .HasColumnName("idEspacio");

                    b.HasKey("IdReserva")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "IdCliente" }, "idCliente")
                        .HasDatabaseName("idCliente2");

                    b.HasIndex(new[] { "IdEspacio" }, "idEspacio")
                        .HasDatabaseName("idEspacio1");

                    b.ToTable("reservas", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Usuario", b =>
                {
                    b.Property<int>("IdUsuario")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int(11)")
                        .HasColumnName("idUsuario");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("IdUsuario"));

                    b.Property<string>("Contrasena")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)")
                        .HasColumnName("contrasena");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("email");

                    b.Property<string>("Estado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("enum('Activo','Inactivo')")
                        .HasColumnName("estado")
                        .HasDefaultValueSql("'Activo'");

                    b.Property<DateTime?>("FechaRegistro")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasColumnName("fechaRegistro")
                        .HasDefaultValueSql("current_timestamp()");

                    b.Property<string>("Nombre")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)")
                        .HasColumnName("nombre");

                    b.Property<string>("ResetToken")
                        .HasColumnType("longtext");

                    b.Property<DateTime?>("ResetTokenExpiry")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Rol")
                        .IsRequired()
                        .HasColumnType("enum('Administrador','Empleado')")
                        .HasColumnName("rol");

                    b.HasKey("IdUsuario")
                        .HasName("PRIMARY");

                    b.HasIndex(new[] { "Email" }, "email")
                        .IsUnique()
                        .HasDatabaseName("email1");

                    b.ToTable("usuarios", (string)null);
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Historialestacionamiento", b =>
                {
                    b.HasOne("FinalMarzo.net.Models.Cliente", "IdClienteNavigation")
                        .WithMany("Historialestacionamientos")
                        .HasForeignKey("IdCliente")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("historialestacionamiento_ibfk_1");

                    b.HasOne("FinalMarzo.net.Models.Espaciosestacionamiento", "IdEspacioNavigation")
                        .WithMany("Historialestacionamientos")
                        .HasForeignKey("IdEspacio")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("historialestacionamiento_ibfk_2");

                    b.Navigation("IdClienteNavigation");

                    b.Navigation("IdEspacioNavigation");
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Pago", b =>
                {
                    b.HasOne("FinalMarzo.net.Models.Cliente", "IdClienteNavigation")
                        .WithMany("Pagos")
                        .HasForeignKey("IdCliente")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("pagos_ibfk_1");

                    b.Navigation("IdClienteNavigation");
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Reserva", b =>
                {
                    b.HasOne("FinalMarzo.net.Models.Cliente", "IdClienteNavigation")
                        .WithMany("Reservas")
                        .HasForeignKey("IdCliente")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("reservas_ibfk_1");

                    b.HasOne("FinalMarzo.net.Models.Espaciosestacionamiento", "IdEspacioNavigation")
                        .WithMany("Reservas")
                        .HasForeignKey("IdEspacio")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("reservas_ibfk_2");

                    b.Navigation("IdClienteNavigation");

                    b.Navigation("IdEspacioNavigation");
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Cliente", b =>
                {
                    b.Navigation("Historialestacionamientos");

                    b.Navigation("Pagos");

                    b.Navigation("Reservas");
                });

            modelBuilder.Entity("FinalMarzo.net.Models.Espaciosestacionamiento", b =>
                {
                    b.Navigation("Historialestacionamientos");

                    b.Navigation("Reservas");
                });
#pragma warning restore 612, 618
        }
    }
}
