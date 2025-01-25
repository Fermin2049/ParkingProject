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
    public class EspaciosEstacionamientoController : ControllerBase
    {
        private readonly MyDbContext _context;

        public EspaciosEstacionamientoController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/EspaciosEstacionamiento
        [HttpGet]
        public async Task<
            ActionResult<IEnumerable<Espaciosestacionamiento>>
        > GetEspaciosEstacionamiento()
        {
            return await _context.Espaciosestacionamientos.ToListAsync();
        }

        // GET: api/EspaciosEstacionamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Espaciosestacionamiento>> GetEspacioEstacionamiento(int id)
        {
            var espacio = await _context.Espaciosestacionamientos.FindAsync(id);

            if (espacio == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }

            return espacio;
        }

        // POST: api/EspaciosEstacionamiento
        [HttpPost]
        public async Task<ActionResult<Espaciosestacionamiento>> PostEspacioEstacionamiento(
            Espaciosestacionamiento espacio
        )
        {
            // Validar si el número de espacio ya existe
            var espacioExistente = await _context.Espaciosestacionamientos.FirstOrDefaultAsync(e =>
                e.NumeroEspacio == espacio.NumeroEspacio
            );

            if (espacioExistente != null)
            {
                return BadRequest("El número de espacio ya está en uso.");
            }

            _context.Espaciosestacionamientos.Add(espacio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEspacioEstacionamiento),
                new { id = espacio.IdEspacio },
                espacio
            );
        }

        // PUT: api/EspaciosEstacionamiento/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspacioEstacionamiento(
            int id,
            Espaciosestacionamiento espacio
        )
        {
            if (id != espacio.IdEspacio)
            {
                return BadRequest("El ID del espacio no coincide.");
            }

            // Validar si el espacio existe
            var espacioExistente = await _context.Espaciosestacionamientos.FindAsync(id);
            if (espacioExistente == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }

            // Actualizar los campos del espacio existente
            espacioExistente.NumeroEspacio = espacio.NumeroEspacio;
            espacioExistente.Estado = espacio.Estado;
            espacioExistente.TipoEspacio = espacio.TipoEspacio;
            espacioExistente.Sector = espacio.Sector;
            espacioExistente.FechaActualizacion = DateTime.Now;

            _context.Entry(espacioExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Espaciosestacionamientos.Any(e => e.IdEspacio == id))
                {
                    return NotFound("El espacio de estacionamiento no existe.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/EspaciosEstacionamiento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspacioEstacionamiento(int id)
        {
            var espacio = await _context.Espaciosestacionamientos.FindAsync(id);
            if (espacio == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }

            // Verificar que el espacio no esté reservado u ocupado
            if (espacio.Estado == "Reservado" || espacio.Estado == "Ocupado")
            {
                return BadRequest("No se puede eliminar un espacio reservado u ocupado.");
            }

            _context.Espaciosestacionamientos.Remove(espacio);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
