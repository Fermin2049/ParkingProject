using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Data.Models
{
    public partial class Espaciosestacionamiento
    {
        public int IdEspacio { get; set; }

        public int NumeroEspacio { get; set; }

        public string? TipoEspacio { get; set; }

        public string? Sector { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
