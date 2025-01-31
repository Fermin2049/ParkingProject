using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Controllers
{
    [Authorize] // 🔹 Todos los endpoints requieren autenticación
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ReservasController(MyDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener todas las reservas (Solo Administradores y Empleados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context
                .Reservas.Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEspacioNavigation)
                .ToListAsync();
        }

        // ✅ Obtener reservas de un cliente específico (Solo Administradores y Empleados)
        [HttpGet("cliente/{idCliente}")]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservasByCliente(int idCliente)
        {
            var reservas = await _context
                .Reservas.Where(r => r.IdCliente == idCliente)
                .Include(r => r.IdEspacioNavigation)
                .ToListAsync();

            if (!reservas.Any())
            {
                return NotFound("No se encontraron reservas para este cliente.");
            }

            return reservas;
        }

        // ✅ Obtener reservas del cliente autenticado
        [HttpGet("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetMyReservas()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var reservas = await _context
                .Reservas.Where(r => r.IdCliente == cliente.IdCliente)
                .Include(r => r.IdEspacioNavigation)
                .ToListAsync();

            return reservas;
        }

        // ✅ Obtener una reserva específica (Solo Administradores, Empleados y Cliente Propietario)
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context
                .Reservas.Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEspacioNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound("Reserva no encontrada.");
            }

            // Si es un cliente, solo puede acceder a su propia reserva
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);

            if (
                User.IsInRole("Cliente")
                && (cliente == null || reserva.IdCliente != cliente.IdCliente)
            )
            {
                return Forbid();
            }

            return reserva;
        }

        // ✅ Crear una nueva reserva (Solo Clientes)
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            // Verificar que el espacio esté disponible
            var espacio = await _context.Espaciosestacionamientos.FirstOrDefaultAsync(e =>
                e.IdEspacio == reserva.IdEspacio && e.Estado == "Disponible"
            );
            if (espacio == null)
            {
                return BadRequest("El espacio no está disponible.");
            }

            reserva.IdCliente = cliente.IdCliente;
            reserva.Estado = "Activa";

            _context.Reservas.Add(reserva);
            espacio.Estado = "Reservado"; // Marcar espacio como reservado
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.IdReserva }, reserva);
        }

        // ✅ Modificar una reserva (Solo Clientes dueños de la reserva)
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
                return BadRequest();

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var existingReserva = await _context.Reservas.FirstOrDefaultAsync(r =>
                r.IdReserva == id && r.IdCliente == cliente.IdCliente
            );

            if (existingReserva == null)
            {
                return NotFound("Reserva no encontrada.");
            }

            existingReserva.FechaReserva = reserva.FechaReserva;
            existingReserva.FechaExpiracion = reserva.FechaExpiracion;

            _context.Entry(existingReserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Cancelar o eliminar una reserva (Clientes pueden cancelar su reserva, Administradores pueden eliminar cualquier reserva)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound("Reserva no encontrada.");

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);

            // Si es un cliente, solo puede eliminar su propia reserva
            if (
                User.IsInRole("Cliente")
                && (cliente == null || reserva.IdCliente != cliente.IdCliente)
            )
            {
                return Forbid();
            }

            var espacio = await _context.Espaciosestacionamientos.FirstOrDefaultAsync(e =>
                e.IdEspacio == reserva.IdEspacio
            );
            if (espacio != null)
            {
                espacio.Estado = "Disponible"; // Liberar el espacio
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
