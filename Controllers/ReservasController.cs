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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ReservasController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            var reserva = await _context
                .Reservas.Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEspacioNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id && r.IdCliente == cliente.IdCliente);

            if (reserva == null)
            {
                return NotFound("Reserva no encontrada.");
            }

            return reserva;
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

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

        // PUT: api/Reservas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return BadRequest();
            }

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Reservas.Any(e => e.IdReserva == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Reservas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
            {
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            var reserva = await _context.Reservas.FirstOrDefaultAsync(r =>
                r.IdReserva == id && r.IdCliente == cliente.IdCliente
            );

            if (reserva == null)
            {
                return NotFound("Reserva no encontrada.");
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
