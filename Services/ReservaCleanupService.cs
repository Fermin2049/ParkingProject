using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalMarzo.net.Data;
using FinalMarzo.net.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinalMarzo.net.Services
{
    public class ReservaCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservaCleanupService> _logger;
        private readonly TimeSpan _intervaloEjecucion = TimeSpan.FromMinutes(5); // 🔹 Ejecuta cada 5 minutos

        public ReservaCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReservaCleanupService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await LimpiarReservasExpiradas();
                await Task.Delay(_intervaloEjecucion, stoppingToken); // 🔹 Esperar 5 minutos antes de la próxima ejecución
            }
        }

        private async Task LimpiarReservasExpiradas()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                DateTime limite = DateTime.UtcNow.AddMinutes(-10); // 🔹 Expira en 10 minutos
                DateTime haceUnDia = DateTime.UtcNow.AddDays(-1); // 🔹 Solo revisa las últimas 24 horas

                var reservasExpiradas = await _context
                    .Reservas.Where(r =>
                        r.Estado == "EnProceso"
                        && r.FechaReserva < limite
                        && r.FechaReserva > haceUnDia
                    )
                    .ToListAsync();

                if (reservasExpiradas.Any())
                {
                    foreach (var reserva in reservasExpiradas)
                    {
                        reserva.Estado = "Cancelada";
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation(
                        $"Se cancelaron {reservasExpiradas.Count} reservas por tiempo expirado."
                    );
                }
            }
        }
    }
}
