namespace Core.DTOs
{
    public class GestionarAlertasDto
    {
        public long IdAlerta { get; set; }
        public string? Alerta { get; set; }
        public long IdAlertaSeguimiento { get; set; }

        public string? TextoEstado { get; set; }
        public string? ColorEstado { get; set; }
        public DateTime? FechaNotificacion { get; set; }
        public string? NombreNNA { get; set; }
        public string? NombreEAPB { get; set; }
        public string? DocumentoNNA { get; set; }
        public string? Categoria { get; set; }
        public string? Subcategoria { get; set; }
        public string? Estado { get; set; }

    }
}
