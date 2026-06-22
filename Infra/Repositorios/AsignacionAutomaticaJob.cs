using Core.Interfaces.Repositorios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

namespace Infra.Repositorios
{
    public class AsignacionAutomaticaJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;

        public AsignacionAutomaticaJob(IServiceProvider serviceProvider, IOptions<Core.DTOs.Quartz> optQuartz)
        {
            _serviceProvider = serviceProvider;
        }

        // BUG-LZ 2026-06-20: antes el job hacia un HTTP GET a la raiz del API que no
        // disparaba nada. Ahora invoca directamente la logica de asignacion en el repo,
        // que es lo que la HU SECANI-RQ10-HU10 describe: el sistema reparte los
        // seguimientos pendientes entre los agentes disponibles.
        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var repo = scope.ServiceProvider.GetRequiredService<ISeguimientoRepo>();
                var asignados = await repo.AsignacionAutomatica();
                Console.WriteLine($"[AsignacionAutomaticaJob] Asignados {asignados?.Count ?? 0} seguimientos.");

                var reagendados = await repo.AsignacionAutomaticaReagendar();
                Console.WriteLine($"[AsignacionAutomaticaJob] Reagendados {reagendados?.Count ?? 0} seguimientos.");

                var reasignados = await repo.AsignacionAutomaticaReasignacion();
                Console.WriteLine($"[AsignacionAutomaticaJob] Reasignados {reasignados?.Count ?? 0} seguimientos.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[AsignacionAutomaticaJob] Error: {ex.GetType().Name}: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}
