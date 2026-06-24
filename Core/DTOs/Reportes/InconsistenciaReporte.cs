namespace Core.DTOs.Reportes
{
    public class InconsistenciaReporte
    {
        // KPI 1
        public int TotalInconsistencias { get; set; }

        // KPI 2 — distribución por tipo de campo (22 campos HU)
        public Dictionary<string, int> InconsistenciasPorCampo { get; set; } = new();

        // KPI 3 — % por fuente de datos
        public List<InconsistenciaFuente> InconsistenciasPorFuente { get; set; } = new();

        // KPI 4 — por entidad territorial
        public List<InconsistenciaDepartamento> InconsistenciasPorDepartamento { get; set; } = new();
        public List<InconsistenciaMunicipio> InconsistenciasPorMunicipio { get; set; } = new();

        // KPI 5 — por tipo de cáncer
        public List<InconsistenciaTipoCancer> InconsistenciasPorTipoCancer { get; set; } = new();
        // Mantenido (diagnóstico CIE10)
        public List<InconsistenciaDiagnostico> InconsistenciasPorDiagnostico { get; set; } = new();

        // KPI 6 — tiempo promedio resolución (días)
        public double TiempoPromedioResolucionDias { get; set; }
        public int TotalResueltos { get; set; }
        public int TotalPendientes { get; set; }

        // KPI 7 — validados auto vs manual
        public int ValidadasAutomaticamente { get; set; }
        public int ValidadasManualmente { get; set; }

        // KPI 8 — tasa reincidencia
        public double TasaReincidencia { get; set; }
        public int TotalReincidentes { get; set; }

        // KPI 9 — impacto tiempo notif / tratamiento
        public double ImpactoNotificacionDias { get; set; }
        public double ImpactoTratamientoDias { get; set; }

        // KPI 10 — campos críticos trazabilidad
        public List<CampoCriticoTrazabilidad> CamposCriticosTrazabilidad { get; set; } = new();
    }

    public class InconsistenciaDepartamento
    {
        public string DepartamentoId { get; set; }
        public string Departamento { get; set; }
        public int TotalInconsistencias { get; set; }
        public double Porcentaje { get; set; }
    }

    public class InconsistenciaMunicipio
    {
        public string MunicipioId { get; set; }
        public string Municipio { get; set; }
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

    public class InconsistenciaTipoCancer
    {
        public string TipoCancerId { get; set; }
        public string TipoCancer { get; set; }
        public int TotalInconsistencias { get; set; }
        public double Porcentaje { get; set; }
    }

    public class InconsistenciaFuente
    {
        public string Fuente { get; set; }
        public int TotalInconsistencias { get; set; }
        public double Porcentaje { get; set; }
    }

    public class CampoCriticoTrazabilidad
    {
        public string Campo { get; set; }
        public int TotalNNAs { get; set; }
        public int NNAsConFalta { get; set; }
        public double PorcentajeInconsistencia { get; set; }
    }
}
