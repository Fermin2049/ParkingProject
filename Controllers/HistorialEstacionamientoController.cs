using System.Collections.Generic;
using System.Linq;
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
    public class HistorialEstacionamientoController : ControllerBase
    {
        private readonly MyDbContext _context;

        public HistorialEstacionamientoController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/HistorialEstacionamiento
        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<Historialestacionamiento>>
        > GetHistorialEstacionamiento()
        {
            return await _context
                .Historialestacionamientos.Include(h => h.IdClienteNavigation)
                .Include(h => h.IdEspacioNavigation)
                .ToListAsync();
        }

        // GET: api/HistorialEstacionamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Historialestacionamiento>> GetHistorialEstacionamiento(
            int id
        )
        {
            var historial = await _context
                .Historialestacionamientos.Include(h => h.IdClienteNavigation)
                .Include(h => h.IdEspacioNavigation)
                .FirstOrDefaultAsync(h => h.IdHistorial == id);

            if (historial == null)
            {
                return NotFound("El historial no fue encontrado.");
            }

            return historial;
        }

        // POST: api/HistorialEstacionamiento
        [HttpPost]
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

        // PUT: api/HistorialEstacionamiento/Salida/5
        [HttpPut("Salida/{id}")]
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

        // DELETE: api/HistorialEstacionamiento/5
        [HttpDelete("{id}")]
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
