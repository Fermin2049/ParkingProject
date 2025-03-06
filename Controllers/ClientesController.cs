using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Data.Models;
using FinalMarzo.net.Models;
using FinalMarzo.net.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly EmailService _emailService;
        private readonly PasswordService _passwordService;

        public ClientesController(
            MyDbContext context,
            EmailService emailService,
            PasswordService passwordService
        )
        {
            _context = context;
            _emailService = emailService;
            _passwordService = passwordService;
        }

        // ✅ Obtener todos los clientes (Solo Administradores)
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // ✅ Obtener un cliente específico (Solo Administradores)
        [HttpGet("{idCliente}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Cliente>> GetClienteById(int idCliente)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }
            return cliente;
        }

        // ✅ Obtener los datos del cliente autenticado
        [HttpGet("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<Cliente>> GetMyData()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            return cliente;
        }

        // ✅ Registrar un nuevo cliente (Público, sin autorización)
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(
            [FromBody] ClienteRegistroRequest request
        )
        {
            if (request == null || string.IsNullOrEmpty(request.HcaptchaToken))
            {
                return BadRequest("hCaptcha token is required.");
            }

            // Verificar hCaptcha antes de procesar el registro
            var hCaptchaService = new HCaptchaService(new HttpClient());
            bool isValidCaptcha = await hCaptchaService.ValidateHCaptchaAsync(
                request.HcaptchaToken
            );

            if (!isValidCaptcha)
            {
                Console.WriteLine("❌ hCaptcha verification failed.");
                return BadRequest("Invalid hCaptcha verification.");
            }

            // Validar que el teléfono y la placa no estén duplicados
            if (await _context.Clientes.AnyAsync(c => c.Telefono == request.Telefono))
            {
                return BadRequest("El teléfono ya está registrado.");
            }

            if (await _context.Clientes.AnyAsync(c => c.VehiculoPlaca == request.VehiculoPlaca))
            {
                return BadRequest("La placa del vehículo ya está registrada.");
            }

            // Crear el objeto Cliente con los datos recibidos
            var cliente = new Cliente
            {
                Nombre = request.Nombre ?? throw new ArgumentNullException(nameof(request.Nombre)),
                Telefono =
                    request.Telefono ?? throw new ArgumentNullException(nameof(request.Telefono)),
                Email = request.Email,
                VehiculoPlaca =
                    request.VehiculoPlaca
                    ?? throw new ArgumentNullException(nameof(request.VehiculoPlaca)),
                Password = _passwordService.HashPassword(
                    request.Password ?? throw new ArgumentNullException(nameof(request.Password))
                ),
                FechaRegistro = DateTime.Now,
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyData), new { id = cliente.IdCliente }, cliente);
        }

        // ✅ Actualizar los datos del cliente autenticado
        [HttpPut("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PutMyData(Cliente cliente)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.Email == email
            );
            if (clienteExistente == null)
                return NotFound("Cliente no encontrado.");

            // Validar duplicados de teléfono y placa
            if (
                await _context.Clientes.AnyAsync(c =>
                    c.Telefono == cliente.Telefono && c.IdCliente != clienteExistente.IdCliente
                )
            )
                return BadRequest("El teléfono ya está registrado por otro cliente.");

            if (
                await _context.Clientes.AnyAsync(c =>
                    c.VehiculoPlaca == cliente.VehiculoPlaca
                    && c.IdCliente != clienteExistente.IdCliente
                )
            )
                return BadRequest("La placa del vehículo ya está registrada por otro cliente.");

            // Actualizar los datos permitidos
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Telefono = cliente.Telefono;
            clienteExistente.Email = cliente.Email;
            clienteExistente.VehiculoPlaca = cliente.VehiculoPlaca;

            _context.Entry(clienteExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Actualizar cualquier cliente (Solo Administradores)
        [HttpPut("{idCliente}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutCliente(int idCliente, Cliente cliente)
        {
            if (idCliente != cliente.IdCliente)
                return BadRequest("El ID del cliente no coincide.");

            var clienteExistente = await _context.Clientes.FindAsync(idCliente);
            if (clienteExistente == null)
                return NotFound("Cliente no encontrado.");

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Telefono = cliente.Telefono;
            clienteExistente.Email = cliente.Email;
            clienteExistente.VehiculoPlaca = cliente.VehiculoPlaca;

            _context.Entry(clienteExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Eliminar la cuenta del cliente autenticado
        [HttpDelete("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context
                .Clientes.Include(c => c.Reservas)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            if (cliente.Reservas.Any(r => r.Estado == "Activa"))
                return BadRequest("No puedes eliminar tu cuenta si tienes reservas activas.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Eliminar cualquier cliente (Solo Administradores)
        [HttpDelete("{idCliente}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteCliente(int idCliente)
        {
            var cliente = await _context.Clientes.FindAsync(idCliente);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Enviar correo de recuperación de contraseña
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword(
            [FromBody] RecuperarPasswordRequest request
        )
        {
            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.Email == request.Email
            );

            if (cliente == null)
                return NotFound("No account was found with this email.");

            string resetToken = Guid.NewGuid().ToString();
            cliente.ResetToken = resetToken;
            cliente.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            string subject = "Password Recovery";
            string body =
                $"Hello, click the following link to reset your password:\n\n"
                + $"http://localhost:3000/reset-password?token={resetToken}";

            if (cliente.Email != null)
            {
                await _emailService.SendEmailAsync(cliente.Email, subject, body);
            }
            else
            {
                return BadRequest("The client's email is null.");
            }

            return Ok("An email with password reset instructions has been sent.");
        }

        [HttpPost("restablecer-password")]
        public async Task<IActionResult> RestablecerPassword(
            [FromBody] RestablecerPasswordRequest request
        )
        {
            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.NewPassword))
                return BadRequest("El token y la nueva contraseña son obligatorios.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.ResetToken == request.Token
            );

            if (cliente == null || cliente.ResetTokenExpiry < DateTime.UtcNow)
                return BadRequest("El token es inválido o ha expirado.");

            // 🔐 Encriptación de la nueva contraseña antes de guardarla
            cliente.Password = _passwordService.HashPassword(request.NewPassword);

            cliente.ResetToken = null;
            cliente.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Contraseña restablecida con éxito.");
        }
    }

    public class RecuperarPasswordRequest
    {
        public string? Email { get; set; }
    }

    public class RestablecerPasswordRequest
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}
