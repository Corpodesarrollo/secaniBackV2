using Core.DTOs;
using Core.Request;
using Core.response;
using Core.Response;


namespace Core.Interfaces.Repositorios
{
    public interface ISeguimientoRepo
    {
        public List<GetSeguimientoResponse> RepoSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        public int RepoSeguimientoActualizacionFecha(PutSeguimientoActualizacionFechaRequest request);
        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request);
        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal);
        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId);
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request);
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request);
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request);
        public void SetDificultadesProceso(DificultadesProcesoRequest request);
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request);
        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA);
        Task<SeguimientoCntFiltrosDto> SeguimientoCntFiltros(string id);
        Task<List<SeguimientoDto>> GetAllByIdUser(string id, int filtro);
    }
}
