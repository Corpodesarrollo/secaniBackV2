using static Core.Common.Estructuras;

namespace Core.response
{
    public class GetNotificacionResponse
    {
        public long IdNotificacion { get; set; }
        public TipoNotificacion TipoNotificacion { get; set; }
        public long IdSeguimiento { get; set; }
        public string? AgenteDestino { get; set; }
        public string? IdAgenteDestino { get; set; }
        public string? RolAgenteDestino { get; set; }
        public string? IdAgenteOrigen { get; set; }
        public string? AgenteOrigen { get; set; }
        public string? RolAgenteOrigen { get; set; }
        public string? TextoNotificacion { get; set; }
        public bool Administrador { get; set; }
        public string? Url { get; set; }
        public DateTime FechaNotificacion { get; set; }
        public bool Leida { get; set; }
    }
}
