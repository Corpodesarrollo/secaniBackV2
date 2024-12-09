using Core.DTOs;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;


namespace Core.Interfaces.Repositorios
{
    public interface ISeguimientoRepo
    {
        public Seguimiento GetById(long id);
        public List<GetSeguimientoResponse> RepoSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal);
        public int RepoSeguimientoActualizacionFecha(PutSeguimientoActualizacionFechaRequest request);
        public int RepoSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request);
        public List<GetSeguimientoFestivoResponse> RepoSeguimientoFestivo(DateTime FechaInicial, DateTime FechaFinal, string UsuarioId);
        public List<GetSeguimientoHorarioAgenteResponse> RepoSeguimientoHorarioAgente(string UsuarioId);
        public List<GetSeguimientoAgentesResponse> RepoSeguimientoAgentes(string UsuarioId);
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request);
        public GetNNaParcialResponse GetNNaById(long id);
        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA);
        Task<List<SeguimientoDto>> GetAllByIdUser(string id, int filtro);
        Task<SeguimientoCntFiltrosDto> GetCntSeguimiento(string id);
        Task<SeguimientoDatosNNADto?> SeguimientoNNA(long id);
        public int RepoSeguimientoRechazo(PutSeguimientoRechazoRequest request);
        public void AsignacionAutomatica();
        public string CrearPlantillaCorreo(CrearPlantillaCorreoRequest request);
        public string EliminarPlantillaCorreo(EliminarPlantillaCorreoRequest id);
        public List<ConsultarPlantillaResponse> ConsultarPlantillasCorreo();
        public List<HistoricoPlantillaCorreoResponse> HistoricoPlantillaCorreo(string id);
        public Task<ExportarDetalleSeguimientoResponse> ExportarDetalleSeguimiento(long id);
        Task<long> GetCntSeguimientoByNNA(long id);
        Task<string> SetSeguimiento(SetSeguimientoRequest request);
        Task<SeguimientoDto[]> GetSeguimientosByNNA(int idNNA);
    }
}
