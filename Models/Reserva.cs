using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdCliente { get; set; }

    public int IdEspacio { get; set; }

    public DateTime FechaReserva { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public string? Estado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Espaciosestacionamiento IdEspacioNavigation { get; set; } = null!;
}
