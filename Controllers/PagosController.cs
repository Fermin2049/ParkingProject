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
    public class PagosController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PagosController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Pagos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagos()
        {
            return await _context.Pagos.Include(p => p.IdClienteNavigation).ToListAsync();
        }

        // GET: api/Pagos/Cliente/5
        [HttpGet("Cliente/{idCliente}")]
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

        // GET: api/Pagos/Estado/Exitoso
        [HttpGet("Estado/{estado}")]
        public async Task<ActionResult<IEnumerable<Pago>>> GetPagosByEstado(string estado)
        {
            var pagos = await _context
                .Pagos.Where(p => p.Estado == estado)
                .Include(p => p.IdClienteNavigation)
                .ToListAsync();

            if (!pagos.Any())
            {
                return NotFound("No se encontraron pagos con el estado especificado.");
            }

            return pagos;
        }

        // GET: api/Pagos/5
        [HttpGet("{id}")]
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

        // POST: api/Pagos
        [HttpPost]
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

        // PUT: api/Pagos/5
        [HttpPut("{id}")]
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

        // DELETE: api/Pagos/5
        [HttpDelete("{id}")]
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
