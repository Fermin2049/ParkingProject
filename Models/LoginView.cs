using System.ComponentModel.DataAnnotations;

namespace FinalMarzo.net.Models
{
    public class LoginView
    {
        [Required(ErrorMessage = "El usuario es requerido.")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Clave { get; set; } = string.Empty;
    }
}
