using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infra.Repositorios.Procesos
{
    public class ProcesadorTareasAutomaticas : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan _frecuenciaConsulta;

        public ProcesadorTareasAutomaticas(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var intervaloHoras = configuration.GetValue<int>("TareaSegundoPlano:IntervaloMinutos");
            _frecuenciaConsulta = TimeSpan.FromSeconds(5);
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var procesos = await db.ControlProcesosAutomaticos
                    .Where(p => p.EstaActivo && p.ProximaEjecucion <= DateTime.Now && !p.EnEjecucion)
                    .ToListAsync(stoppingToken);

                foreach (var proceso in procesos)
                {
                    proceso.EnEjecucion = true;
                    db.Update(proceso);
                    await db.SaveChangesAsync();

                    try
                    {
                        var procesoImpl = scope.ServiceProvider.GetServices<IProcesoAutomatico>()
                            .FirstOrDefault(p => p.Nombre == proceso.NombreProceso);

                        if (procesoImpl != null)
                        {
                            await procesoImpl.EjecutarAsync(stoppingToken);

                            proceso.UltimaEjecucion = DateTime.Now;
                            proceso.ProximaEjecucion = DateTime.Now.AddSeconds(proceso.IntervaloSegundos);
                            proceso.Estado = "Exitoso";
                            proceso.MensajeError = null;
                        }
                        else
                        {
                            proceso.Estado = "No implementado";
                            proceso.MensajeError = $"No se encontró implementación para {proceso.NombreProceso}";
                        }
                    }
                    catch (Exception ex)
                    {
                        proceso.Estado = "Error";
                        proceso.MensajeError = ex.Message;
                    }

                    proceso.EnEjecucion = false;
                    proceso.FechaActualizacion = DateTime.Now;
                    db.Update(proceso);
                    await db.SaveChangesAsync();
                }

                await Task.Delay(_frecuenciaConsulta, stoppingToken);
            }
        }
    }
}
