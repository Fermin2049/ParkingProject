using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Data.Models;
using FinalMarzo.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalMarzo.net.Controllers
{
    [Authorize] // Todos los endpoints requieren autenticación
    [Route("api/[controller]")]
    [ApiController]
    public class EspaciosEstacionamientoController : ControllerBase
    {
        private readonly MyDbContext _context;

        public EspaciosEstacionamientoController(MyDbContext context)
        {
            _context = context;
        }

        // Obtener todos los espacios (Administradores y Empleados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<
            ActionResult<IEnumerable<Espaciosestacionamiento>>
        > GetEspaciosEstacionamiento()
        {
            return await _context.Espaciosestacionamientos.ToListAsync();
        }

        // Obtener un espacio específico (Administradores y Empleados)
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<Espaciosestacionamiento>> GetEspacioEstacionamiento(int id)
        {
            var espacio = await _context.Espaciosestacionamientos.FindAsync(id);
            if (espacio == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }
            return espacio;
        }

        // Crear un nuevo espacio (Solo Administradores)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<Espaciosestacionamiento>> PostEspacioEstacionamiento(
            Espaciosestacionamiento espacio
        )
        {
            // Validar si el número de espacio ya existe
            if (
                await _context.Espaciosestacionamientos.AnyAsync(e =>
                    e.NumeroEspacio == espacio.NumeroEspacio
                )
            )
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

        // Actualizar un espacio (Solo Administradores)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutEspacioEstacionamiento(
            int id,
            Espaciosestacionamiento espacio
        )
        {
            if (id != espacio.IdEspacio)
            {
                return BadRequest("El ID del espacio no coincide.");
            }

            var espacioExistente = await _context.Espaciosestacionamientos.FindAsync(id);
            if (espacioExistente == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }

            // Actualizar los campos del espacio existente (sin el campo Estado)
            espacioExistente.NumeroEspacio = espacio.NumeroEspacio;
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
                if (!await _context.Espaciosestacionamientos.AnyAsync(e => e.IdEspacio == id))
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

        // Eliminar un espacio (Solo Administradores)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteEspacioEstacionamiento(int id)
        {
            var espacio = await _context.Espaciosestacionamientos.FindAsync(id);
            if (espacio == null)
            {
                return NotFound("El espacio de estacionamiento no fue encontrado.");
            }

            // Verificar que el espacio no tenga una reserva activa
            bool tieneReservaActiva = await _context.Reservas.AnyAsync(r =>
                r.IdEspacio == id && r.Estado == "Activa"
            );
            if (tieneReservaActiva)
            {
                return BadRequest("No se puede eliminar un espacio con una reserva activa.");
            }

            _context.Espaciosestacionamientos.Remove(espacio);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Obtener solo los espacios disponibles (sin reservas activas)
        [HttpGet("disponibles")]
        [Authorize(Roles = "Administrador,Empleado,Cliente")]
        public async Task<
            ActionResult<IEnumerable<Espaciosestacionamiento>>
        > GetEspaciosDisponibles()
        {
            var espaciosDisponibles = await _context
                .Espaciosestacionamientos.Where(e =>
                    !_context.Reservas.Any(r => r.IdEspacio == e.IdEspacio && r.Estado == "Activa")
                )
                .ToListAsync();
            return Ok(espaciosDisponibles);
        }
    }
}
