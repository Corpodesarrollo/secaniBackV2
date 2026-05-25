namespace Core.DTOs.MSTablasParametricas
{
    public class HistoricoTransaccionDTO
    {
        public string NombreTabla { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Transaccion { get; set; } // Puede repetir el valor de TipoId o tener más detalle
        public string UsuarioId { get; set; }
        public string RegistroAnterior { get; set; }
        public string RegistroNuevo { get; set; }
        public string Comentario { get; set; } 
    }
}
