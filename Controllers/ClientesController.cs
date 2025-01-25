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
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ClientesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context
                .Clientes.Include(c => c.Reservas)
                .Include(c => c.Pagos)
                .Include(c => c.Historialestacionamientos)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            return cliente;
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Validar que el teléfono y la placa no estén duplicados
            var telefonoDuplicado = await _context.Clientes.AnyAsync(c =>
                c.Telefono == cliente.Telefono
            );
            if (telefonoDuplicado)
            {
                return BadRequest("El teléfono ya está registrado.");
            }

            var placaDuplicada = await _context.Clientes.AnyAsync(c =>
                c.VehiculoPlaca == cliente.VehiculoPlaca
            );
            if (placaDuplicada)
            {
                return BadRequest("La placa del vehículo ya está registrada.");
            }

            cliente.FechaRegistro = DateTime.Now;

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.IdCliente }, cliente);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return BadRequest("El ID del cliente no coincide.");
            }

            var clienteExistente = await _context.Clientes.FindAsync(id);
            if (clienteExistente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            // Validar duplicados de teléfono y placa
            var telefonoDuplicado = await _context.Clientes.AnyAsync(c =>
                c.Telefono == cliente.Telefono && c.IdCliente != id
            );
            if (telefonoDuplicado)
            {
                return BadRequest("El teléfono ya está registrado por otro cliente.");
            }

            var placaDuplicada = await _context.Clientes.AnyAsync(c =>
                c.VehiculoPlaca == cliente.VehiculoPlaca && c.IdCliente != id
            );
            if (placaDuplicada)
            {
                return BadRequest("La placa del vehículo ya está registrada por otro cliente.");
            }

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Telefono = cliente.Telefono;
            clienteExistente.Email = cliente.Email;
            clienteExistente.VehiculoPlaca = cliente.VehiculoPlaca;

            _context.Entry(clienteExistente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Clientes.Any(c => c.IdCliente == id))
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

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context
                .Clientes.Include(c => c.Reservas)
                .Include(c => c.Pagos)
                .Include(c => c.Historialestacionamientos)
                .FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                return NotFound("Cliente no encontrado.");
            }

            if (cliente.Reservas.Any(r => r.Estado == "Activa"))
            {
                return BadRequest("El cliente tiene reservas activas y no puede ser eliminado.");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
