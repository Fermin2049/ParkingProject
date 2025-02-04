using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Models;

public class Cliente
{
    public int IdCliente { get; set; }

    public string Nombre { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Email { get; set; }

    public string VehiculoPlaca { get; set; } = null!;

    public DateTime? FechaRegistro { get; set; }

    public string Password { get; set; } = null!;

    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }

    public virtual ICollection<Historialestacionamiento> Historialestacionamientos { get; set; } =
        new List<Historialestacionamiento>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
