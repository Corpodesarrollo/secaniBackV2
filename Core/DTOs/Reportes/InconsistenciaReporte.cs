namespace Core.DTOs.Reportes
{
    public class InconsistenciaReporte
    {
        public int TotalInconsistencias { get; set; }
        public Dictionary<string, int> InconsistenciasPorCampo { get; set; }
        public List<InconsistenciaDepartamento> InconsistenciasPorDepartamento { get; set; }
        public List<InconsistenciaDiagnostico> InconsistenciasPorDiagnostico { get; set; }
    }

    public class InconsistenciaDepartamento
    {
        public string DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public int TotalInconsistencias { get; set; }
        public double Porcentaje { get; set; }
    }

    public class InconsistenciaDiagnostico
    {
        public int? DiagnosticoId { get; set; }
        public string Diagnostico { get; set; }
        public int TotalInconsistencias { get; set; }
        public double Porcentaje { get; set; }
    }
}
