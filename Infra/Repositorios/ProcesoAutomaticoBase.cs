using Core.Interfaces;

namespace Infra.Repositorios
{
    public abstract class ProcesoAutomaticoBase : IProcesoAutomatico
    {
        public abstract string Nombre { get; }

        public async Task EjecutarAsync(CancellationToken cancellationToken)
        {
            try
            {
                await EjecutarProcesoAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        protected abstract Task EjecutarProcesoAsync(CancellationToken cancellationToken);
    }
}
