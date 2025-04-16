namespace Infra.Repositorios.Procesos
{
    public class ProcesoEnviarCorreos : ProcesoAutomaticoBase
    {
        public override string Nombre => "ProcesoEnviarCorreos";

        protected override async Task EjecutarProcesoAsync(CancellationToken cancellationToken)
        {
            // Simula envío de correos
            await Task.Delay(3000, cancellationToken);
        }
    }
}
