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
    //[Authorize] // Descomentar si quieres que todos los endpoints requieran autenticación
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public ClientesController(MyDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener todos los clientes (solo si es necesario)
        [HttpGet]
        [Authorize(Roles = "Administrador")] // Solo admin puede ver todos los clientes
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return await _context.Clientes.ToListAsync();
        }

        // ✅ Obtener los datos del cliente autenticado
        [HttpGet("me")]
        [Authorize] // Solo clientes autenticados pueden acceder
        public async Task<ActionResult<Cliente>> GetMyData()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Email == email);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            return cliente;
        }

        // ✅ Registrar un nuevo cliente
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Validar que el teléfono y la placa no estén duplicados
            if (await _context.Clientes.AnyAsync(c => c.Telefono == cliente.Telefono))
                return BadRequest("El teléfono ya está registrado.");

            if (await _context.Clientes.AnyAsync(c => c.VehiculoPlaca == cliente.VehiculoPlaca))
                return BadRequest("La placa del vehículo ya está registrada.");

            cliente.FechaRegistro = DateTime.Now;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyData), new { id = cliente.IdCliente }, cliente);
        }

        // ✅ Actualizar los datos del cliente autenticado
        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> PutMyData(Cliente cliente)
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var clienteExistente = await _context.Clientes.FirstOrDefaultAsync(c =>
                c.Email == email
            );
            if (clienteExistente == null)
                return NotFound("Cliente no encontrado.");

            // Validar duplicados de teléfono y placa
            if (
                await _context.Clientes.AnyAsync(c =>
                    c.Telefono == cliente.Telefono && c.IdCliente != clienteExistente.IdCliente
                )
            )
                return BadRequest("El teléfono ya está registrado por otro cliente.");

            if (
                await _context.Clientes.AnyAsync(c =>
                    c.VehiculoPlaca == cliente.VehiculoPlaca
                    && c.IdCliente != clienteExistente.IdCliente
                )
            )
                return BadRequest("La placa del vehículo ya está registrada por otro cliente.");

            // Actualizar los datos permitidos
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Telefono = cliente.Telefono;
            clienteExistente.Email = cliente.Email;
            clienteExistente.VehiculoPlaca = cliente.VehiculoPlaca;

            _context.Entry(clienteExistente).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ Eliminar la cuenta del cliente autenticado
        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized();

            var cliente = await _context
                .Clientes.Include(c => c.Reservas)
                .Include(c => c.Pagos)
                .Include(c => c.Historialestacionamientos)
                .FirstOrDefaultAsync(c => c.Email == email);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            if (cliente.Reservas.Any(r => r.Estado == "Activa"))
            {
                return BadRequest("No puedes eliminar tu cuenta si tienes reservas activas.");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
