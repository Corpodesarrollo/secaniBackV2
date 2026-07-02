using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.Response;
using Core.Services.MSTablasParametricas;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;
using SISPRO.TRV.Entity;

namespace Core.Interfaces.Repositorios
{
    public interface INNARepo
    {
        Task<NNADto?> GetById(long id);
        Task<(bool, NNAs)> AddAsync(NNAs dto, User user);
        Task<(bool, NNAs)> UpdateAsync(NNAs entity, User user);
        public RespuestaResponse<List<FiltroNNADto>> ConsultarNNAFiltro(FiltroNNARequest entrada);
        public List<VwAgentesAsignados> VwAgentesAsignados();
        public void ActualizarNNASeguimiento(NNASeguimientoRequest request);
        public NNADto? ConsultarNNAsByTipoIdNumeroId(string tipoIdentificacionId, string numeroIdentificacion);
        public NNAResponse ConsultarNNAsById(long NNAId);
        public Task<DatosBasicosNNAResponse> ConsultarDatosBasicosNNAById(long NNAId, TablaParametricaService tablaParametricaService);
        public Task<SolicitudSeguimientoCuidadorResponse> SolicitudSeguimientoCuidador(long NNAId, TablaParametricaService tablaParametricaService);
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request);
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request);
        public void SetDificultadesProceso(DificultadesProcesoRequest request);
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request);
        Task<List<ConsultaCasosAbiertosResponse>> ConsultaCasosAbiertos(CasosAbiertosRequest request);
        Task<DepuracionProtocoloResponse> DepuracionProtocolo(List<DepuracionProtocoloRequest> request);
        Task<DepuracionProtocoloResponse> CargarArchivoNNA(IFormFile file);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction transaction);
        Task RollbackTransactionAsync(IDbContextTransaction transaction);
        Task ActualizarFallecido(NNADto data);
        Task AsignacionManual(AsignacionManualRequest request);
        Task CrearSeguimientoInicialNNA(long nnaId, long? contactoNNAId, string? telefono, string? userId);
        Task<List<NNAPendienteSivigilaDto>> GetPendientesSivigila(int? eapbId, string? municipioId, string? departamentoId);
    }
}
