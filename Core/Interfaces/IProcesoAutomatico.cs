namespace Core.Interfaces
{
    public interface IProcesoAutomatico
    {
        string Nombre { get; }

        Task EjecutarAsync(CancellationToken cancellationToken);
    }
}
