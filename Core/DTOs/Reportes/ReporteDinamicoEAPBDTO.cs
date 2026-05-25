namespace Core.DTOs.Reportes
{
    public class ReporteDinamicoEAPBDTO
    {
        public int EAPBId { get; set; }
        public string? EAPB { get; set; }
        public int CasosAsociados { get; set; }
        public int CasosConAlertasSinResolver { get; set; }
        public int TotalDeAlertasSinResolver { get; set; }
        public double PromedioTiempoRespuestaAlertas { get; set; }
        public int CasosRegimenContributivo { get; set; }
        public int CasosRegimenSubsidiado { get; set; }
        public int CasosRegimenEspecial { get; set; }
        public int CasosRegimenExcepcion { get; set; }
        public int CasosRegimenNoAfiliado { get; set; }
        public int TotalAlertasResueltas { get; set; }
        public int CasosSeguimientoPorIniciar { get; set; }
        public int CasosSeguimientoEnProceso { get; set; }
        public int CasosSeguimientoCulminado { get; set; }
    }
}
