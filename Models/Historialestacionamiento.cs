using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Models;

public partial class Historialestacionamiento
{
    public int IdHistorial { get; set; }

    public int IdCliente { get; set; }

    public int IdEspacio { get; set; }

    public DateTime FechaEntrada { get; set; }

    public DateTime? FechaSalida { get; set; }

    public string? Estado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Espaciosestacionamiento IdEspacioNavigation { get; set; } = null!;
}
