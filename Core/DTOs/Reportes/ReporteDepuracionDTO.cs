namespace Core.DTOs.Reportes
{
    public class ReporteDepuracionDTO
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public int RegistrosIngresados { get; set; }
        public int RegistrosNuevos { get; set; }
        public int RegistrosDuplicados { get; set; }
        public int SegundasNeoplasias { get; set; }
        public int Recaidas { get; set; }
        public string Estado { get; set; }
    }
}
