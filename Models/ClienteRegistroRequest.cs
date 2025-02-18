using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalMarzo.net.Models
{
    public class ClienteRegistroRequest
    {
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? VehiculoPlaca { get; set; }
        public string? Password { get; set; }
        public string? HcaptchaToken { get; set; } // Token de hCaptcha
    }
}
