using System;
using System.Globalization;
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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PagosManagementController : ControllerBase
    {
        private readonly MyDbContext _context;

        public PagosManagementController(MyDbContext context)
        {
            _context = context;
        }

        // POST /api/PagosManagement/confirmar
        [HttpPost("confirmar")]
        public async Task<IActionResult> ConfirmarPago([FromBody] PaymentConfirmationDto dto)
        {
            // 1. Verifica que exista la reserva
            var reserva = await _context.Reservas.FindAsync(dto.ReservaId);
            if (reserva == null)
            {
                return NotFound($"No se encontró la reserva con ID {dto.ReservaId}");
            }

            // 2. Actualiza la FechaExpiracion y Estado de la reserva
            //    Conviertes la fechaExpiracion (string) a DateTime
            DateTime fechaExpiracion;
            if (
                !DateTime.TryParseExact(
                    dto.FechaExpiracion,
                    "yyyy-MM-dd'T'HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out fechaExpiracion
                )
            )
            {
                // Si falla el parseo, puedes manejarlo:
                return BadRequest(
                    "Formato de fechaExpiracion inválido. Se requiere yyyy-MM-dd'T'HH:mm:ss"
                );
            }

            // Marca la reserva como "Activa" y ajusta la fecha
            reserva.FechaExpiracion = fechaExpiracion;
            reserva.Estado = "Activa";
            _context.Entry(reserva).State = EntityState.Modified;

            // 3. Registrar el pago en la base de datos (si quieres guardar el Pago)
            //    Primero, obtenemos el cliente
            var cliente = await _context.Clientes.FindAsync(reserva.IdCliente);
            if (cliente == null)
            {
                return BadRequest("No se encontró el cliente asociado a la reserva.");
            }

            var nuevoPago = new Pago
            {
                IdCliente = cliente.IdCliente,
                IdReserva = reserva.IdReserva,
                Monto = (decimal)dto.Monto,
                MetodoPago = dto.MetodoPago ?? "Simulado",
                FechaPago = DateTime.Now,
                Estado = "Exitoso",
            };

            _context.Pagos.Add(nuevoPago);

            // 4. Guarda los cambios
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Error al confirmar el pago: " + ex.Message);
            }

            // 5. Devuelve un resultado satisfactorio
            return Ok(
                new
                {
                    message = "Pago confirmado y reserva actualizada",
                    reservaId = reserva.IdReserva,
                    fechaExpiracion = reserva.FechaExpiracion,
                    estado = reserva.Estado,
                }
            );
        }
    }
}
