using Microsoft.AspNetCore.Mvc;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
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


        [HttpGet("GetSeguimientoUsuario")]
        public List<GetSeguimientoResponse> GetSeguimientoUsuario(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        [HttpPost("SetEstadoDiagnosticoTratamiento")]
        public void SetEstadoDiagnosticoTratamiento(EstadoDiagnosticoTratamientoRequest request)
        {
           
            List<GetSeguimientoResponse>  response = seguimientoRepo.RepoSeguimientoUsuario(UsuarioId, FechaInicial, FechaFinal);
            return response;
            seguimientoRepo.SetEstadoDiagnosticoTratamiento(request);
        }

        [HttpPut("PutSeguimientotActualizacionFecha")]
        public int PutSeguimientotActualizacionFecha(PutSeguimientoActualizacionFechaRequest request)
        [HttpPost("SetDiagnosticoTratamiento")]
        public void SetDiagnosticoTratamiento(DiagnosticoTratamientoRequest request)
        {
            return seguimientoRepo.RepoSeguimientoActualizacionFecha(request);
        }

        [HttpPut("PutSeguimientoActualizacionUsuario")]
        public int PutSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            return seguimientoRepo.RepoSeguimientoActualizacionUsuario(request);
            seguimientoRepo.SetDiagnosticoTratamiento(request);
        }

        [HttpGet("GetSeguimientoFestivos")]
        public List<GetSeguimientoFestivoResponse> GetSeguimientoFestivos(DateTime FechaInicial, DateTime FechaFinal)
        [HttpPost("SetResidenciaDiagnosticoTratamiento")]
        public void SetResidenciaDiagnosticoTratamiento(ResidenciaDiagnosticoTratamientoRequest request)
        {

            List<GetSeguimientoFestivoResponse> response = seguimientoRepo.RepoSeguimientoFestivo( FechaInicial, FechaFinal);
            return response;
            seguimientoRepo.SetResidenciaDiagnosticoTratamiento(request);
        }

        [HttpGet("GetSeguimientoHorarioAgente")]
        public List<GetSeguimientoHorarioAgenteResponse> GetSeguimientoHorarioAgente(string UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        [HttpPost("SetDificultadesProceso")]
        public void SetDificultadesProceso(DificultadesProcesoRequest request)
        {

            List<GetSeguimientoHorarioAgenteResponse> response = seguimientoRepo.RepoSeguimientoHorarioAgente(UsuarioId, FechaInicial, FechaFinal);
            return response;
            seguimientoRepo.SetDificultadesProceso(request);
        }

        [HttpGet("GetSeguimientoAgentes")]
        public List<GetSeguimientoAgentesResponse> GetSeguimientoAgentes(string UsuarioId)
        [HttpPost("SetAdherenciaProceso")]
        public void SetAdherenciaProceso(AdherenciaProcesoRequest request)
        {

            List<GetSeguimientoAgentesResponse> response = seguimientoRepo.RepoSeguimientoAgentes(UsuarioId);
            return response;
            seguimientoRepo.SetAdherenciaProceso(request);
        }


    }
}


