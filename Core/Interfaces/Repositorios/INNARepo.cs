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

       
        public NNAs ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion);
        public NNAs ConsultarNNAsById(long NNAId);
        public Task<DatosBasicosNNAResponse> ConsultarDatosBasicosNNAById(long NNAId, TablaParametricaService tablaParametricaService);
        public Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService);
        
        public DepuracionProtocoloResponse DepuracionProtocolo(List<DepuracionProtocoloRequest> request);
    }
}
