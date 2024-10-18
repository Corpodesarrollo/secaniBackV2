using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;

namespace Core.Interfaces.Repositorios
{
    public interface INNARepo
    {
        Task<NNADto?> GetById(long id);
        Task<(bool, NNAs)> AddAsync(NNAs dto);
        Task<(bool, NNAs)> UpdateAsync(NNAs entity);
        public RespuestaResponse<FiltroNNADto> ConsultarNNAFiltro(FiltroNNARequest entrada);
        public List<VwAgentesAsignados> VwAgentesAsignados();
        public void ActualizarNNASeguimiento(NNASeguimientoRequest request);
        public NNAResponse ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion);
        public NNAResponse ConsultarNNAsById(long NNAId);
        public Task<DatosBasicosNNAResponse> ConsultarDatosBasicosNNAById(long NNAId, TablaParametricaService tablaParametricaService);
        public Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService);
        public DepuracionProtocoloResponse DepuracionProtocolo(List<DepuracionProtocoloRequest> request);
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request);
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request);
        public void SetDificultadesProceso(DificultadesProcesoRequest request);
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request);
        public List<ConsultaCasosAbiertosResponse> ConsultaCasosAbiertos(CasosAbiertosRequest request);
        public void AsignacionManual(AsignacionManualRequest request);



    }
}
