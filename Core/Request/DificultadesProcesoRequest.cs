namespace Core.Request
{
    public class DificultadesProcesoRequest
    {
        public int IdSeguimiento { get; set; }
        public bool AutorizacionMedicamento { get; set; }
        public bool EntregaMedicamentoLAP { get; set; }
        public bool EntregaMedicamentoNoLAP { get; set; }
        public bool AsignacionCitas { get; set; }
        public bool CobradoCopagos { get; set; }
        public bool AutorizacionProcedimientos { get; set; }
        public bool RemisionEspecialistas { get; set; }
        public bool MalaAtencionIPS { get; set; }
        public int IdMalaIPS { get; set; }
        public bool FallasMIPRES { get; set; }
        public bool FallasConvenio { get; set; }
        public int IdCategoriaAlerta { get; set; }
        public int IdSubcategoriaAlerta { get; set; }
        public bool HaSidoTrasladado { get; set; }
        public int NumeroTraslados { get; set; }
        public int[] IdIpsTraslado { get; set; }
        public bool AccionLegal { get; set; }
        public string MotivoAccionLegal { get; set; }
        public string? IdTipoRecurso { get; set; }

    }
}
