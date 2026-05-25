using Core.Interfaces;

namespace Infra.Repositorios
{
    public abstract class ProcesoAutomaticoBase : IProcesoAutomatico
    {
        public abstract string Nombre { get; }

        public async Task EjecutarAsync(DateTime lastRun, CancellationToken cancellationToken)
        {
            try
            {
                await EjecutarProcesoAsync(lastRun, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected abstract Task EjecutarProcesoAsync(DateTime lastRun, CancellationToken cancellationToken);
    }
}
