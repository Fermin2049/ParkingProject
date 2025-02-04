using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
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

        public ClientesController(MyDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Validar que el teléfono y la placa no estén duplicados
            if (await _context.Clientes.AnyAsync(c => c.Telefono == cliente.Telefono))
                return BadRequest("El teléfono ya está registrado.");

            if (await _context.Clientes.AnyAsync(c => c.VehiculoPlaca == cliente.VehiculoPlaca))
                return BadRequest("La placa del vehículo ya está registrada.");

            cliente.FechaRegistro = DateTime.Now;
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
                .Include(c => c.Pagos)
                .Include(c => c.Historialestacionamientos)
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
                return BadRequest("El email es obligatorio.");

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.Email == request.Email
            );
            if (cliente == null)
                return NotFound("No existe un cliente con ese correo.");

            // Generar un token temporal (puede ser un código aleatorio o un enlace con un JWT)
            string resetToken = Guid.NewGuid().ToString();

            // Enviar el correo con el enlace de recuperación
            string subject = "Recuperación de Contraseña";
            string body =
                $"Hola {cliente.Nombre},\n\n"
                + "Haz clic en el siguiente enlace para restablecer tu contraseña:\n\n"
                + $"http://localhost:3000/reset-password?token={resetToken}\n\n"
                + "Si no solicitaste este cambio, ignora este mensaje.";

            if (cliente.Email != null)
            {
                await _emailService.SendEmailAsync(cliente.Email, subject, body);
            }
            else
            {
                return BadRequest("El email del cliente es nulo.");
            }

            return Ok("Se ha enviado un correo con instrucciones para recuperar tu contraseña.");
        }
    }
}
