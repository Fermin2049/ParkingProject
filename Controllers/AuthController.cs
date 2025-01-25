using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Models;
using FinalMarzo.net.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginView model)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Usuario);
            if (user == null || !_passwordService.VerifyPassword(model.Clave, user.Contrasena))
            {
                return Unauthorized("Usuario o contraseña incorrectos.");
            }

            var token = _tokenService.GenerateJwtToken(user.Email, user.Rol, user.IdUsuario);
            return Ok(new { token });
        }
    }
}
