﻿using System;
using FinalMarzo.net.Data.Models;

namespace FinalMarzo.net.Models
{
    public partial class Pago
    {
        public int IdPago { get; set; }
        public int IdCliente { get; set; }
        public int IdReserva { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; } = null!;
        public DateTime? FechaPago { get; set; }
        public string? Estado { get; set; }

        public virtual Cliente IdClienteNavigation { get; set; } = null!;
        public virtual Reserva IdReservaNavigation { get; set; } = null!;
    }
}
