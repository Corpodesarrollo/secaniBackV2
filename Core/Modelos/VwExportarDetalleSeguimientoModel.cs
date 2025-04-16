using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Modelos
{
    [Table("VwExportarDetalleSeguimiento")]
    public class VwExportarDetalleSeguimientoModel
    {
        [Key]
        public long SeguimientoId { get; set; }
        public long NNAId { get; set; }
        public int SeguimientosRealizados { get; set; }
        public int SeguimientosEnProceso { get; set; }
        public string? Nombres { get; set; }
        public string? Edad { get; set; }
        public string? Diagnostico { get; set; }
        public string? FechaSeguimiento { get; set; }
        public string? FechaSivigila { get; set; }
        public string? Sexo { get; set; }
        public string? TipoIdentificacion { get; set; }
        public string? NumeroIdentificacion { get; set; }
        public string? Pais { get; set; }
        public string? FechaNacimiento { get; set; }
        public string? Etnia { get; set; }
        public string? DepartamentoNacimiento { get; set; }
        public string? CiudadNacimiento { get; set; }
        public string? OrigenReporte { get; set; }
        public string? DepartamentoTratamiento { get; set; }
        public string? EstadoIngresoEstrategia { get; set; }
        public string? FechaIngresoEstrategia { get; set; }
        public string? GrupoPoblacional { get; set; }
        public string? RegimenAfiliacion { get; set; }
        public string? Asegurador { get; set; }
        public string? Ips { get; set; }
        public string? RazonesNoDiagnostico { get; set; }
        public string? FechaConsulta { get; set; }
        public string? FechaDiagnostico { get; set; }
        public string? FechaInicioTratamiento { get; set; }
        public string? IpsTratamiento { get; set; }
        public string? Recaida { get; set; }
        public int CantidadRecaidas { get; set; }
        public string? FechaUltimaRecaida { get; set; }
        public string? ProcedenciaDepartamento { get; set; }
        public string? ProcedenciaMunicipio { get; set; }
        public string? ProcedenciaBarrio { get; set; }
        public string? ProcedenciaArea { get; set; }
        public string? ProcedenciaDireccion { get; set; }
        public string? ProcedenciaEstrato { get; set; }
        public string? ProcedenciaTelefono { get; set; }
        public string? ActualDepartamento { get; set; }
        public string? ActualMunicipio { get; set; }
        public string? ActualBarrio { get; set; }
        public string? ActualArea { get; set; }
        public string? ActualDireccion { get; set; }
        public string? ActualEstrato { get; set; }
        public string? ActualTelefono { get; set; }
        public string? RequirioTraslado { get; set; }
        public string? CapacidadEconomica { get; set; }
        public string? ServiciosSocialesA { get; set; }
        public string? OportunidadSSA { get; set; }
        public string? CoberturaTrasladoSSA { get; set; }
        public string? NombreFundacion { get; set; }
        public string? ApoyoFundacion { get; set; }
        public string? SitioResidencia { get; set; }
        public string? AsumioCostoTraslado { get; set; }
        public string? AsumioCostoVivienda { get; set; }
        public string? AutorizacionMed { get; set; }
        public string? EntregaMedLAP { get; set; }
        public string? EntregaMedNoLAP { get; set; }
        public string? AsignacionCitas { get; set; }
        public string? CobroCopagos { get; set; }
        public string? AutorizacionProc { get; set; }
        public string? RemisionIExp { get; set; }
        public string? MalaAtencionIPS { get; set; }
        public string? FallaMipres { get; set; }
        public string? FallaEapbIps { get; set; }
        public string? TransladoInstitucion { get; set; }
        public int NumeroTraslado { get; set; }
        public string? IpsTraslado { get; set; }
        public string? AccionLegal { get; set; }
        public string? MotivoAccionLegal { get; set; }
        public string? TipoRecursoAccionLegal { get; set; }
        public string? DejoAsistirTratamiento { get; set; }
        public string? CuantoTiempoTratamiento { get; set; }
        public string? CausaInasistencia { get; set; }
        public string? OtaCausaCual { get; set; }
        public string? Estudiando { get; set; }
        public string? DejoAsistirColegio { get; set; }
        public string? CuantoTiempoColegio { get; set; }
        public string? InformeClaroDiagTrat { get; set; }
    }
}
