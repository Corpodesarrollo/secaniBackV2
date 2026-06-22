namespace Core.DTOs
{
    public class GestionarAlertasDto
    {
        // Bug 2026-06-17: antes IdAlerta=TPEstadoAlerta.Id (1..5) -> columna "ID Alerta" repetia
        // valores en la grilla y la apertura de respuesta fallaba (RespuestasAlerta.IdAlerta es
        // AlertaSeguimiento.Id). Ahora IdAlerta=AlertaSeguimiento.Id (PK unica por fila) y se
        // separa IdEstadoAlerta para preservar el switch de TextoEstado/ColorEstado.
        public long IdAlerta { get; set; }
        public long AlertaIdBase { get; set; }
        public long IdEstadoAlerta { get; set; }
        public string? Alerta { get; set; }
        public long IdAlertaSeguimiento { get; set; }
        public long IdSeguimiento { get; set; }
        public string? TextoEstado { get; set; }
        public string? ColorEstado { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? NombreNNA { get; set; }
        public string? NombreEAPB { get; set; }
        public string? DocumentoNNA { get; set; }
        public string? Categoria { get; set; }
        public string? Subcategoria { get; set; }
        public string? Estado { get; set; }
        // BUG-LZ 2026-06-20: HU SECANI-RQ07-HU04 = una respuesta por alerta cierra la alerta.
        // El front oculta el boton "Enviar respuesta" cuando esta bandera es true.
        public bool TieneRespuesta { get; set; }

    }
}
