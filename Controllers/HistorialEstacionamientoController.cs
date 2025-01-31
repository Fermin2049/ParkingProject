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
    public class HistorialEstacionamientoController : ControllerBase
    {
        private readonly MyDbContext _context;

        public HistorialEstacionamientoController(MyDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener TODO el historial de estacionamiento (Solo Administradores y Empleados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<
            ActionResult<IEnumerable<Historialestacionamiento>>
        > GetHistorialEstacionamiento()
        {
            return await _context
                .Historialestacionamientos.Include(h => h.IdClienteNavigation)
                .Include(h => h.IdEspacioNavigation)
                .ToListAsync();
        }

        // ✅ Obtener historial de estacionamiento de un cliente específico (Solo Administradores y Empleados)
        [HttpGet("cliente/{idCliente}")]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<
            ActionResult<IEnumerable<Historialestacionamiento>>
        > GetHistorialByCliente(int idCliente)
        {
            var historial = await _context
                .Historialestacionamientos.Where(h => h.IdCliente == idCliente)
                .Include(h => h.IdEspacioNavigation)
                .ToListAsync();

            if (!historial.Any())
            {
                return NotFound("No hay historial para este cliente.");
            }

            return historial;
        }

        // ✅ Obtener historial del cliente autenticado
        [HttpGet("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<Historialestacionamiento>>> GetMyHistorial()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var historial = await _context
                .Historialestacionamientos.Where(h => h.IdCliente == cliente.IdCliente)
                .Include(h => h.IdEspacioNavigation)
                .ToListAsync();

            return historial;
        }

        // ✅ Registrar entrada de un vehículo (Solo Empleados)
        [HttpPost]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<Historialestacionamiento>> PostHistorialEstacionamiento(
            Historialestacionamiento historial
        )
        {
            // Verificar que el espacio está disponible
            var espacio = await _context.Espaciosestacionamientos.FirstOrDefaultAsync(e =>
                e.IdEspacio == historial.IdEspacio && e.Estado == "Disponible"
            );
            if (espacio == null)
            {
                return BadRequest("El espacio no está disponible.");
            }

            // Registrar la entrada
            historial.FechaEntrada = DateTime.Now;
            historial.Estado = "Activo";

            _context.Historialestacionamientos.Add(historial);
            espacio.Estado = "Ocupado"; // Cambiar el estado del espacio
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetHistorialEstacionamiento),
                new { id = historial.IdHistorial },
                historial
            );
        }

        // ✅ Registrar salida de un vehículo (Solo Empleados)
        [HttpPut("salida/{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> RegistrarSalida(int id)
        {
            var historial = await _context
                .Historialestacionamientos.Include(h => h.IdEspacioNavigation)
                .FirstOrDefaultAsync(h => h.IdHistorial == id && h.Estado == "Activo");

            if (historial == null)
            {
                return NotFound("El registro de entrada no fue encontrado o ya fue completado.");
            }

            // Registrar la salida
            historial.FechaSalida = DateTime.Now;
            historial.Estado = "Completado";

            // Liberar el espacio de estacionamiento
            var espacio = historial.IdEspacioNavigation;
            if (espacio != null)
            {
                espacio.Estado = "Disponible";
            }

            _context.Entry(historial).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Eliminar un historial de estacionamiento (Solo Administradores)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteHistorialEstacionamiento(int id)
        {
            var historial = await _context.Historialestacionamientos.FindAsync(id);
            if (historial == null)
            {
                return NotFound("El historial no fue encontrado.");
            }

            _context.Historialestacionamientos.Remove(historial);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
