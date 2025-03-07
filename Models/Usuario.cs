﻿using System;
using System.Collections.Generic;

namespace FinalMarzo.net.Data.Models
{
    public partial class Usuario
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Contrasena { get; set; } = null!;

        public string Rol { get; set; } = null!;

        public DateTime? FechaRegistro { get; set; }

        public string? Estado { get; set; }

        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpiry { get; set; }
    }
}
