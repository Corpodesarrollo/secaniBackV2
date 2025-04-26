using Core.Interfaces.Repositorios;

namespace Infra.Repositorios.Procesos
{
    public class ProcesoRevisarEnviarNotificaciones(INotificacionRepo notificacionRepo) : ProcesoAutomaticoBase
    {
        public override string Nombre => "ProcesoRevisarEnviarNotificaciones";

        protected override async Task EjecutarProcesoAsync(DateTime lastRun, CancellationToken cancellationToken)
        {
            await notificacionRepo.RevisarYEnviarNotificaciones();
        }
    }
}
