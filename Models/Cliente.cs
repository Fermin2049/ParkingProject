using System;
using System.Collections.Generic;
using FinalMarzo.net.Data.Models;

namespace FinalMarzo.net.Models
{
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

        // Asegúrate de que esta propiedad esté tipada correctamente:
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
