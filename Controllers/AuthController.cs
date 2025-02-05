using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Models;
using FinalMarzo.net.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FinalMarzo.net.Controllers
{
    [Route("api/auth")] // 🔹 URL BASE del controlador
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly TokenService _tokenService;
        private readonly PasswordService _passwordService;
        private readonly EmailService _emailService;

        public AuthController(
            MyDbContext context,
            TokenService tokenService,
            PasswordService passwordService,
            EmailService emailService
        )
        {
            _context = context;
            _tokenService = tokenService;
            _passwordService = passwordService;
            _emailService = emailService;
        }

        // ✅ LOGIN CLIENTE
        [HttpPost("login-cliente")] // 🔹 URL: /api/auth/login-cliente
        public async Task<IActionResult> LoginCliente([FromBody] LoginRequest model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Email y contraseña son requeridos.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (
                cliente == null
                || !_passwordService.VerifyPassword(model.Password, cliente.Password)
            )
                return Unauthorized("Correo o contraseña incorrectos.");

            // Generar Token
            if (cliente.Email == null)
                return BadRequest("El email del cliente no puede ser nulo.");

            var token = _tokenService.GenerateJwtToken(cliente.Email, "Cliente", cliente.IdCliente);
            return Ok(new { token });
        }

        // ✅ LOGIN ADMINISTRADOR O EMPLEADO
        [HttpPost("login-usuario")] // 🔹 URL: /api/auth/login-usuario
        public async Task<IActionResult> LoginUsuario([FromBody] LoginRequest model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return BadRequest("Email y contraseña son requeridos.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (
                usuario == null
                || !_passwordService.VerifyPassword(model.Password, usuario.Contrasena)
            )
                return Unauthorized("Correo o contraseña incorrectos.");

            // Generar Token con el rol correcto
            var token = _tokenService.GenerateJwtToken(
                usuario.Email,
                usuario.Rol,
                usuario.IdUsuario
            );
            return Ok(new { token });
        }

        [HttpPost("solicitar-recuperacion")]
        public async Task<IActionResult> SolicitarRecuperacion(
            [FromBody] ResetPasswordRequest request
        )
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("El email es obligatorio.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.Email == request.Email
            );
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.Email == request.Email
            );

            if (usuario == null && cliente == null)
                return NotFound("No se encontró una cuenta con este email.");

            var resetToken = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddHours(1);

            if (usuario != null)
            {
                usuario.ResetToken = resetToken;
                usuario.ResetTokenExpiry = expiry;
            }
            else if (cliente != null)
            {
                cliente.ResetToken = resetToken;
                cliente.ResetTokenExpiry = expiry;
            }

            await _context.SaveChangesAsync();

            string resetLink = $"https://tuaplicacion.com/reset-password?token={resetToken}";
            string body =
                $"Haz clic en el siguiente enlace para restablecer tu contraseña: {resetLink}";

            await _emailService.SendEmailAsync(request.Email, "Recuperación de Contraseña", body);

            return Ok("Se ha enviado un enlace de recuperación a tu correo.");
        }

        [HttpPost("restablecer-contrasena")]
        public async Task<IActionResult> RestablecerContrasena(
            [FromBody] ResetPasswordConfirm request
        )
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("El token y la nueva contraseña son obligatorios.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.ResetToken == request.Token
            );
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.ResetToken == request.Token
            );

            Console.WriteLine($"🔹 Token recibido: {request.Token}");
            Console.WriteLine($"🔹 Fecha actual UTC: {DateTime.UtcNow}");

            if (usuario != null)
            {
                usuario.ResetTokenExpiry = usuario.ResetTokenExpiry?.ToUniversalTime();
                Console.WriteLine($"🔹 Token Usuario Expiry: {usuario.ResetTokenExpiry}");
            }
            if (cliente != null)
            {
                cliente.ResetTokenExpiry = cliente.ResetTokenExpiry?.ToUniversalTime();
                Console.WriteLine($"🔹 Token Cliente Expiry: {cliente.ResetTokenExpiry}");
            }

            if (usuario == null && cliente == null)
                return NotFound("Token inválido o expirado.");

            if (
                (usuario != null && usuario.ResetTokenExpiry < DateTime.UtcNow)
                || (cliente != null && cliente.ResetTokenExpiry < DateTime.UtcNow)
            )
            {
                return BadRequest("El token ha expirado.");
            }

            if (usuario != null)
            {
                usuario.Contrasena = _passwordService.HashPassword(request.NewPassword);
                usuario.ResetToken = null;
                usuario.ResetTokenExpiry = null;
            }
            else if (cliente != null)
            {
                cliente.Password = _passwordService.HashPassword(request.NewPassword);
                cliente.ResetToken = null;
                cliente.ResetTokenExpiry = null;
            }

            await _context.SaveChangesAsync();
            return Ok("Contraseña restablecida con éxito.");
        }

        [HttpGet("server-time")]
        public IActionResult GetServerTime()
        {
            return Ok(new { ServerTimeUtc = DateTime.UtcNow, ServerTimeLocal = DateTime.Now });
        }
    }

    // ✅ Modelos para los requests
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string? Email { get; set; }
    }

    public class ResetPasswordConfirm
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}
