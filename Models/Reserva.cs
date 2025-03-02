using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FinalMarzo.net.Models;

namespace FinalMarzo.net.Data.Models
{
    public class Reserva
    {
        public int IdReserva { get; set; }
        public int IdCliente { get; set; }
        public int IdEspacio { get; set; }
        public DateTime FechaReserva { get; set; }
        public DateTime FechaExpiracion { get; set; }

        [Required]
        public string Estado { get; set; } = string.Empty;

        // Estas propiedades de navegación no se requieren en la solicitud,
        // por lo que se ignoran en el binding.
        [JsonIgnore]
        public Cliente? IdClienteNavigation { get; set; }

        [JsonIgnore]
        public Espaciosestacionamiento? IdEspacioNavigation { get; set; }

        public List<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
