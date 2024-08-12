using Core.DTOs;
using Core.Request;
using Core.response;
using Core.Modelos;
using Microsoft.AspNetCore.Mvc;
using Core.Response;


namespace Core.Interfaces.Repositorios
{
    public interface ISeguimientoRepo
    {
        public Seguimiento GetById(long id);
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
        public void SetAdherenciaProceso (AdherenciaProcesoRequest request);
        public GetNNaParcialResponse GetNNaById(long id);
        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA);
        Task<List<SeguimientoDto>> GetAllByIdUser(string id, int filtro);
        Task<SeguimientoCntFiltrosDto> GetCntSeguimiento(string id);
        public string SetSeguimiento(SetSeguimientoRequest request);
    }
}
