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
    public class PagosController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PagosController(MyDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener TODOS los pagos (Solo Administradores y Empleados)
        [HttpGet]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
        {
            return await _context.Pagos.Include(p => p.IdClienteNavigation).ToListAsync();
        }

        // ✅ Obtener pagos de un cliente específico (Solo Administradores y Empleados)
        [HttpGet("cliente/{idCliente}")]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagosByCliente(int idCliente)
        {
            var pagos = await _context
                .Pagos.Where(p => p.IdCliente == idCliente)
                .Include(p => p.IdClienteNavigation)
                .ToListAsync();

            if (!pagos.Any())
            {
                return NotFound("No se encontraron pagos para el cliente especificado.");
            }

            return pagos;
        }

        // ✅ Obtener los pagos del cliente autenticado
        [HttpGet("me")]
        [Authorize(Roles = "Cliente")]
        public async Task<ActionResult<IEnumerable<Pago>>> GetMyPagos()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var pagos = await _context
                .Pagos.Where(p => p.IdCliente == cliente.IdCliente)
                .Include(p => p.IdClienteNavigation)
                .ToListAsync();

            return pagos;
        }

        // ✅ Obtener un pago específico (Solo Administradores y Empleados)
        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador,Empleado")]
        public async Task<ActionResult<Pago>> GetPago(int id)
        {
            var pago = await _context
                .Pagos.Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
            {
                return NotFound("El pago no fue encontrado.");
            }

            return pago;
        }

        // ✅ Registrar un nuevo pago (Solo Empleados)
        [HttpPost]
        [Authorize(Roles = "Empleado")]
        public async Task<ActionResult<Pago>> PostPago(Pago pago)
        {
            var cliente = await _context.Clientes.FindAsync(pago.IdCliente);
            if (cliente == null)
            {
                return BadRequest("El cliente especificado no existe.");
            }

            pago.FechaPago = DateTime.Now;
            pago.Estado = "Exitoso";

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPago), new { id = pago.IdPago }, pago);
        }

        // ✅ Modificar un pago (Solo Administradores)
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> PutPago(int id, Pago pago)
        {
            if (id != pago.IdPago)
            {
                return BadRequest("El ID del pago no coincide.");
            }

            var existingPago = await _context.Pagos.FindAsync(id);
            if (existingPago == null)
            {
                return NotFound("El pago no fue encontrado.");
            }

            existingPago.Monto = pago.Monto;
            existingPago.MetodoPago = pago.MetodoPago;
            existingPago.Estado = pago.Estado;

            _context.Entry(existingPago).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pagos.Any(e => e.IdPago == id))
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

        // ✅ Eliminar un pago (Solo Administradores)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeletePago(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
            {
                return NotFound("El pago no fue encontrado.");
            }

            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
