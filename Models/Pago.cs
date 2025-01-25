using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int IdCliente { get; set; }

    public decimal Monto { get; set; }

    public string MetodoPago { get; set; } = null!;

    public DateTime? FechaPago { get; set; }

    public string? Estado { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;
}
