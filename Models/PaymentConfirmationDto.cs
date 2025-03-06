using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalMarzo.net.Models
{
    public class PaymentConfirmationDto
    {
        // ID de la reserva que se va a confirmar
        public int ReservaId { get; set; }

        // Monto pagado (puede servir para validar)
        public double Monto { get; set; }

        // Método de pago utilizado (por ejemplo "MercadoPago")
        public string? MetodoPago { get; set; }

        // Opcional: ID del pago en Mercado Pago para registro
        public string? PaymentId { get; set; }

        public string? FechaExpiracion { get; set; }
    }
}
