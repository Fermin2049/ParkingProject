using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Data.Models;
using FinalMarzo.net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalMarzo.net.Controllers
{
    [Authorize] // Todos los endpoints requieren autenticación
    [Route("api/[controller]")]
    [ApiController]
    public class ReservasController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<ReservasController> _logger;

        public ReservasController(MyDbContext context, ILogger<ReservasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Obtener todas las reservas (Solo Administradores y Empleados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context
                .Reservas.Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEspacioNavigation)
                .ToListAsync();
        }

        // Obtener reservas de un cliente específico (Solo Administradores y Empleados)
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

        // Obtener reservas del cliente autenticado
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

        // Obtener una reserva específica (Solo Administradores, Empleados y Cliente Propietario)
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

        // Crear una nueva reserva (Solo Clientes)
        [HttpPost]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            // Imprimir en consola y loguear el objeto recibido
            _logger.LogInformation("PostReserva: Reserva recibida: {@Reserva}", reserva);
            Console.WriteLine(
                "PostReserva: Reserva recibida: "
                    + Newtonsoft.Json.JsonConvert.SerializeObject(reserva)
            );

            // Validar ModelState y registrar errores en consola
            if (!ModelState.IsValid)
            {
                var errores = string.Join(
                    "; ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                );
                _logger.LogWarning("PostReserva: ModelState inválido: {Errores}", errores);
                Console.WriteLine("PostReserva: ModelState inválido: " + errores);
                return BadRequest(ModelState);
            }

            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("PostReserva: No se encontró el email en el token.");
                Console.WriteLine("PostReserva: No se encontró el email en el token.");
                return Unauthorized();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
            {
                _logger.LogWarning("PostReserva: Cliente no encontrado para email {Email}", email);
                Console.WriteLine("PostReserva: Cliente no encontrado para email: " + email);
                return NotFound("Cliente no encontrado.");
            }

            var espacio = await _context.EspaciosEstacionamiento.FindAsync(reserva.IdEspacio);
            if (espacio == null)
            {
                _logger.LogWarning(
                    "PostReserva: Espacio no encontrado para IdEspacio {IdEspacio}",
                    reserva.IdEspacio
                );
                Console.WriteLine(
                    "PostReserva: Espacio no encontrado para IdEspacio: " + reserva.IdEspacio
                );
                return NotFound("Espacio no encontrado.");
            }

            // Verificar que el espacio no esté ocupado ni reservado
            bool tieneReservaActiva = await _context.Reservas.AnyAsync(r =>
                r.IdEspacio == reserva.IdEspacio
                && (r.Estado == "Activa" || r.Estado == "EnProceso")
            );
            if (tieneReservaActiva)
            {
                _logger.LogWarning(
                    "PostReserva: El espacio {IdEspacio} ya tiene una reserva activa o en proceso.",
                    reserva.IdEspacio
                );
                Console.WriteLine(
                    "PostReserva: El espacio "
                        + reserva.IdEspacio
                        + " ya tiene una reserva activa o en proceso."
                );
                return BadRequest(
                    "El espacio ya tiene una reserva activa o está en proceso de pago."
                );
            }

            reserva.IdCliente = cliente.IdCliente;
            reserva.Estado = "EnProceso"; // Se marca como "EnProceso" mientras se paga

            // Asignar valores por defecto para fechas si no se han proporcionado
            if (reserva.FechaReserva == default)
            {
                reserva.FechaReserva = System.DateTime.UtcNow;
                _logger.LogInformation(
                    "PostReserva: FechaReserva no proporcionada, se asigna UTC now: {FechaReserva}",
                    reserva.FechaReserva
                );
                Console.WriteLine(
                    "PostReserva: FechaReserva no proporcionada, se asigna UTC now: "
                        + reserva.FechaReserva
                );
            }
            if (reserva.FechaExpiracion == default)
            {
                reserva.FechaExpiracion = reserva.FechaReserva.AddMinutes(10);
                _logger.LogInformation(
                    "PostReserva: FechaExpiracion no proporcionada, se asigna 10 minutos después: {FechaExpiracion}",
                    reserva.FechaExpiracion
                );
                Console.WriteLine(
                    "PostReserva: FechaExpiracion no proporcionada, se asigna 10 minutos después: "
                        + reserva.FechaExpiracion
                );
            }

            try
            {
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "PostReserva: Reserva creada con IdReserva {IdReserva}",
                    reserva.IdReserva
                );
                Console.WriteLine("PostReserva: Reserva creada con IdReserva " + reserva.IdReserva);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "PostReserva: Exception al guardar la reserva");
                Console.WriteLine("PostReserva: Exception al guardar la reserva: " + ex.ToString());
                return BadRequest("Error al registrar la reserva.");
            }

            return CreatedAtAction(nameof(GetReserva), new { id = reserva.IdReserva }, reserva);
        }

        // Modificar una reserva (Solo Clientes dueños de la reserva)
        [HttpPut("{id}")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> PutReserva(int id, Reserva reserva)
        {
            if (id != reserva.IdReserva)
                return BadRequest("El ID de la reserva no coincide.");

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

            // Solo actualizamos las fechas si se enviaron valores distintos al valor por defecto.
            if (reserva.FechaReserva != default(DateTime))
            {
                existingReserva.FechaReserva = reserva.FechaReserva;
            }

            if (reserva.FechaExpiracion != default(DateTime))
            {
                existingReserva.FechaExpiracion = reserva.FechaExpiracion;
            }

            // Actualizamos el estado siempre
            existingReserva.Estado = reserva.Estado;

            _context.Entry(existingReserva).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Cancelar o eliminar una reserva (Clientes pueden cancelar su reserva, Administradores pueden eliminar cualquier reserva)
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

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Endpoint para obtener los espacios disponibles en un rango de fechas y opcionalmente filtrados por tipo.
        [HttpGet("disponibles")]
        [Authorize]
        public async Task<
            ActionResult<IEnumerable<Espaciosestacionamiento>>
        > GetEspaciosDisponibles(
            [FromQuery] string fechaInicio,
            [FromQuery] string fechaFin,
            [FromQuery] string? tipo
        )
        {
            if (
                !System.DateTime.TryParseExact(
                    fechaInicio,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out System.DateTime fi
                )
            )
            {
                return BadRequest("Formato incorrecto para fecha de inicio.");
            }
            if (
                !System.DateTime.TryParseExact(
                    fechaFin,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out System.DateTime ff
                )
            )
            {
                return BadRequest("Formato incorrecto para fecha de fin.");
            }

            if (fi < System.DateTime.Now)
            {
                return BadRequest("La fecha de inicio debe ser en el futuro.");
            }
            if (ff <= fi)
            {
                return BadRequest("La fecha de fin debe ser posterior a la de inicio.");
            }

            var query = _context.EspaciosEstacionamiento.AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(e =>
                    e.TipoEspacio != null && e.TipoEspacio.ToLower() == tipo.ToLower()
                );
            }

            // Aquí se modifica la condición:
            // Se excluyen los espacios que tienen una reserva con estado "EnProceso"
            // O que tienen una reserva con estado "Activa" cuyo intervalo [FechaReserva, FechaExpiracion]
            // se solape con el intervalo solicitado [fi, ff]
            var espaciosDisponibles = await query
                .Where(e =>
                    !_context.Reservas.Any(r =>
                        r.IdEspacio == e.IdEspacio
                        && (
                            r.Estado == "EnProceso"
                            || (
                                r.Estado == "Activa"
                                && r.FechaReserva <= ff
                                && r.FechaExpiracion >= fi
                            )
                        )
                    )
                )
                .ToListAsync();

            return Ok(espaciosDisponibles);
        }
    }
}
