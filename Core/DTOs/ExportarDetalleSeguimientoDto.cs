using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTOs
{
    public class ExportarDetalleSeguimientoDto
    {
        public string Nombre { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string? Diagnostico { get; set; }
        public DateTime? FechaSeguimiento { get; set; }
        public long Id { get; set; }
        public string IdSexo { get; set; }
        public string Sexo { get; set; }
        public string TipoIdentificacion { get; set; }
        public string NumeroIdentificacion { get; set; }
        public string PaisNacimiento { get; set; }
        public string Etnia { get; set; }
        public string DepartamentoNacimiento { get; set; }
        public string CiudadNacimiento { get; set; }
        public int? OrigenReporte { get; set; }
        public string DepartamentoTratamiento { get; set; }
        public int? EstadoIngreso { get; set; }
        public DateTime? FechaIngreso { get; set; }
        public string? GrupoPoblacional { get; set; }
        public int SemanasGestacion { get; set; }
        public string? RegimenAfiliacion { get; set; }
        public int? Asegurador { get; set; }
        public int? Ips { get; set; }
        public int CantidadSegumientos { get; set; }
        public long IdNNA { get; set; }
        public string razonesNoTratamiento { get; set; }
        public DateTime? fechaConsulta { get; set; }
        public DateTime? fechaDiagnostico { get; set; }
        public DateTime? fechaInicioTratamiento { get; set; }
        public string IpsTratamiento { get; set; }
        public bool? tieneRecaidas{ get; set; }
        public int? numeroRecaidas {  get; set; }
        public DateTime? fechaUltimaRecaida {  get; set; }
        public string departamentoResidencia {  get; set; }
        public string municipioResidencia { get; set; }
        public string barrioResidencia { get; set; }
        public string areaResidencia { get; set; }
        public string direccionResidencia { get; set; }
        public string estratoResidencia { get; set; }
        public string telefonoResidencia { get; set; }
        public bool requirioTrasladarse {  get; set; }
        public string departamentoResidenciaActual {  get; set; }
        public string municipioResidenciaActual { get; set; }
        public string barrioResidenciaActual { get; set; }
        public string areaResidenciaActual { get;set; }
        public string direccionResidenciaActual { get; set; }
        public string estratoResidenciaActual { get; set; }
        public string telefonoResidenciaActual { get; set; }
        public bool? capacidadEconomicaTraslado {  get; set; }
        public bool? apoyoTraslado { get; set; }
        public bool? apoyoOportuno {  get; set; }
        public bool? coberturaServicioSocial {  get; set; }
        public string? nombreFundacion {  get; set; }
        public string? apoyoFundacion { get; set; }
        public string? tipoResidenciaActual { get; set; }
        public string? asumioCostosTraslado { get; set; }
        public string? asumioCostosVivienda {  get; set; }
        public bool? dificultadAutorizacionMedicamentos {  get; set; }
        public bool? dificultadEntregaMedicamentosLAP { get; set; }
        public bool? dificultadEntregaMedicamentosNoLAP {  get; set; }
        public bool? dificultadAsignacionCitas {  get; set; }
        public bool? HanCobradoCopago {  get; set; }
        public bool? AutorizacionProcedimiento {  get; set; }
        public bool? remisionEspecialista {  get; set; }
        public bool? MalaAtencionIps {  get; set; }
        public int? cualIps {  get; set; }
        public bool? FallaMipres {  get; set; }
        public bool? fallaConvenio {  get; set; }
        public bool? HaTrasladado {  get; set; }
        public int? ips {  get; set; }
        public bool? haRecurridoAccionLegal {  get; set; }
        public string? Motivo {  get; set; }
        public string? tipoRecurso {  get; set; }
        public bool? haDejadoTratamiento {  get; set; }
        public int? tiempoInasistenciaTratamiento { get; set; }
        public string? causaInasistencia {  get; set; }
        public string? CualOtraCausaInasistencia { get; set; }
        public bool? estudiaActualmente {  get; set; }
        public bool? haDejadoColegio {  get; set; }
        public int? tiempoInasistenciaColegio { get; set; }
        public bool? ipsClara {  get; set; }
        public List<ExportarDetalleSeguimientoContactoDto> Contactos { get; set; }
        public List<ExportarDetalleSeguimientoSeguimientoAnteriorDto> Anteriores { get; set; }
        public List<ExportarDetalleSeguimientoAlertaDto> Alerta { get; set; }
    }
}
