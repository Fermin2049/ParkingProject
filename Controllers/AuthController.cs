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

        public AuthController(
            MyDbContext context,
            TokenService tokenService,
            PasswordService passwordService
        )
        {
            _context = context;
            _tokenService = tokenService;
            _passwordService = passwordService;
        }

        // ✅ LOGIN CLIENTE
        [HttpPost("login-cliente")] // 🔹 URL: /api/auth/login-cliente
        public async Task<IActionResult> LoginCliente([FromBody] LoginRequest model)
        {
            if (
                model == null
                || string.IsNullOrEmpty(model.Email)
                || string.IsNullOrEmpty(model.Password)
            )
                return BadRequest("Email y contraseña son requeridos.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == model.Email);
            if (
                cliente == null
                || !_passwordService.VerifyPassword(model.Password, cliente.Password)
            )
                return Unauthorized("Correo o contraseña incorrectos.");

            // Generar Token
            if (string.IsNullOrEmpty(cliente.Email))
                return BadRequest("El email del cliente no puede ser nulo o vacío.");

            var token = _tokenService.GenerateJwtToken(cliente.Email, "Cliente", cliente.IdCliente);
            return Ok(new { token });
        }

        // ✅ LOGIN ADMINISTRADOR O EMPLEADO
        [HttpPost("login-usuario")] // 🔹 URL: /api/auth/login-usuario
        public async Task<IActionResult> LoginUsuario([FromBody] LoginRequest model)
        {
            if (
                model == null
                || string.IsNullOrEmpty(model.Email)
                || string.IsNullOrEmpty(model.Password)
            )
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
    }

    // ✅ Clase para recibir datos de login
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
