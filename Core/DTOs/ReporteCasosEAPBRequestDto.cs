namespace Core.DTOs
{
    public class ReporteCasosEAPBRequestDto
    {
        public DateTime FechaInicial { get; set; }
        public DateTime FechaFinal { get; set; }
        public string? Buscar { get; set; }
        public string? EAPB { get; set; }
        public int IdCicloVida { get; set; }

        public bool CategoriaAlerta { get; set; }
        public bool FechaEnvioRespuesta { get; set; }
        public bool DepartamentoProcedencia { get; set; }
        public bool DireccionProcedencia { get; set; }
        public bool Nacionalidad { get; set; }
        public bool SubcategoriaAlerta { get; set; }
        public bool Respuesta { get; set; }
        public bool MunicipioProcedencia { get; set; }
        public bool DepartamentoActual { get; set; }
        public bool Etnia { get; set; }
        public bool Observaciones { get; set; }
        public bool TipoIdentificacion { get; set; }
        public bool BarrioProcedencia { get; set; }
        public bool EstadoNNA { get; set; }
        public bool NumeroIdentificacion { get; set; }
        public bool AreaProcedencia { get; set; }
        public bool RegimenAfiliacion { get; set; }
        public bool DiagnosticoNNA { get; set; }
        public bool IpsPrimaria { get; set; }
    }
}
