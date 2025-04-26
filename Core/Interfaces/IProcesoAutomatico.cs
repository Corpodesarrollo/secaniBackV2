namespace Core.Interfaces
{
    public interface IProcesoAutomatico
    {
        string Nombre { get; }

        Task EjecutarAsync(DateTime lastRun, CancellationToken cancellationToken);
    }
}
