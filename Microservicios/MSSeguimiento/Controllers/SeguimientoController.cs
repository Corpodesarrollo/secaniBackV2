using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeguimientoController : ControllerBase
    {
        private ISeguimientoRepo seguimientoRepo;

        public SeguimientoController(ISeguimientoRepo seguimiento)
        {
            seguimientoRepo = seguimiento;
        }

        [HttpGet("GetAllByIdUser/{UsuarioId}/{filtro}")]
        public async Task<List<SeguimientoDto>> GetAllByIdUser(string UsuarioId, int filtro)
        {

            var response = await seguimientoRepo.GetAllByIdUser(UsuarioId, filtro);
            return response;
        }

        [HttpGet("GetCntSeguimiento/{UsuarioId}")]
        public async Task<SeguimientoCntFiltrosDto> GetCntSeguimiento(string UsuarioId)
        {

            var response = await seguimientoRepo.GetCntSeguimiento(UsuarioId);
            return response;
        }

        [HttpGet("GetSeguimientoUsuario")]
        public List<GetSeguimientoResponse> GetSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetSeguimientoResponse> response = seguimientoRepo.RepoSeguimientoUsuario(UsuarioId, FechaInicial, FechaFinal);
            return response;
        }

        [HttpPut("PutSeguimientotActualizacionFecha")]
        public int PutSeguimientotActualizacionFecha(PutSeguimientoActualizacionFechaRequest request)
        {
            return seguimientoRepo.RepoSeguimientoActualizacionFecha(request);
        }

        [HttpPut("PutSeguimientoActualizacionUsuario")]
        public int PutSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            return seguimientoRepo.RepoSeguimientoActualizacionUsuario(request);
        }

        [HttpGet("GetSeguimientoFestivos")]
        public List<GetSeguimientoFestivoResponse> GetSeguimientoFestivos(DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetSeguimientoFestivoResponse> response = seguimientoRepo.RepoSeguimientoFestivo(FechaInicial, FechaFinal);
            return response;
        }

        [HttpGet("GetSeguimientoHorarioAgente")]
        public List<GetSeguimientoHorarioAgenteResponse> GetSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetSeguimientoHorarioAgenteResponse> response = seguimientoRepo.RepoSeguimientoHorarioAgente(UsuarioId, FechaInicial, FechaFinal);
            return response;
        }

        [HttpGet("GetSeguimientoAgentes")]
        public List<GetSeguimientoAgentesResponse> GetSeguimientoAgentes(string UsuarioId)
        {

            List<GetSeguimientoAgentesResponse> response = seguimientoRepo.RepoSeguimientoAgentes(UsuarioId);
            return response;
        }


        [HttpPost("SetEstadoDiagnosticoTratamiento")]
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetEstadoDiagnosticoTratamiento(request);
        }

        [HttpPost("SetDiagnosticoTratamiento")]
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetDiagnosticoTratamiento(request);
        }

        [HttpPost("SetResidenciaDiagnosticoTratamiento")]
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {
            seguimientoRepo.SetResidenciaDiagnosticoTratamiento(request);
        }

        [HttpPost("SetDificultadesProceso")]
        public void SetDificultadesProceso(DificultadesProcesoRequest request)
        {
            seguimientoRepo.SetDificultadesProceso(request);
        }

        [HttpPost("SetAdherenciaProceso")]
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request)
        {
            seguimientoRepo.SetAdherenciaProceso(request);
        }

        [HttpGet("GetSeguimientosNNA")]
        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA)
        {

            List<SeguimientoNNAResponse> response = seguimientoRepo.GetSeguimientosNNA(idNNA);
            return response;
        }

    }



}


