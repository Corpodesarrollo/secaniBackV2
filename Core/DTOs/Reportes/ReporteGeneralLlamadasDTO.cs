namespace Core.DTOs.Reportes
{
    public class ReporteGeneralLlamadasDTO
    {
        public string AgenteDeSeguimientoId { get; set; }
        public string AgenteDeSeguimiento { get; set; }
        public DateTime FechaIntento { get; set; }
        public int LlamadasRealizadas { get; set; }
        public int Exitosas { get; set; }
        public int Fallidas { get; set; }
        public int NoContestan { get; set; }
        public int TelErrado { get; set; }
        public int TelOcupado { get; set; }
        public int BuzonDeVoz { get; set; }
        public int SinSenal { get; set; }
        public int Otro { get; set; }
    }
}
