using Core.Interfaces.Repositorios;
using Core.response;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepo _dashboardRepo;

        public DashboardController(IDashboardRepo dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }


        [HttpGet("GetTotalCasos")]
        public GetTotalDashboardResponse GetTotalCasos(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardTotalCasos(FechaInicial, FechaFinal, EntidadId!);
            return response;
        }

        [HttpGet("GetTotalRegistros")]
        public GetTotalDashboardResponse GetTotalRegistros(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardTotalRegistros(FechaInicial, FechaFinal, EntidadId!);
            return response;
        }

        [HttpGet("GetTotalMisCasos")]
        public GetTotalDashboardResponse GetTotalMisCasos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardMisCasos(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }

        [HttpGet("GetTotalAlertas")]
        public GetTotalDashboardResponse GetTotalAlertas(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardAlertas(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }

        [HttpGet("GetEstadosSeguimientos")]
        public List<GetDashboardEstadoResponse> GetEstadosSeguimientos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardEstadoResponse> response = _dashboardRepo.RepoDashboardEstados(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }

        [HttpGet("GetEstadosNNas")]
        public List<GetDashboardEstadoResponse> GetEstadosNNas(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardEstadoResponse> response = _dashboardRepo.RepoDashboardEstadoNNA(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }

        [HttpGet("GetEstadosAlertas")]
        public List<GetDashboardEstadoResponse> GetEstadosAlertas(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardEstadoResponse> response = _dashboardRepo.RepoDashboardAlerta(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }

        [HttpGet("GetEstadosIntentos")]
        public List<GetDashboardEstadoResponse> GetEstadosIntentos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardEstadoResponse> response = _dashboardRepo.RepoDashboardIntentos(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }


        [HttpGet("GetFechasAsignacion")]
        public List<GetDashboardFechaTotalResponse> GetFechasAsignacion(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardFechaTotalResponse> response = _dashboardRepo.RepoDashboardAsignadosPorFecha(FechaInicial, FechaFinal, UsuarioID!);
            return response;
        }


        // Segundo dashboard

        [HttpGet("GetEntidadCantidad")]
        public List<GetEntidadCantidadResponse> GetEntidadCantidad(DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetEntidadCantidadResponse> response = _dashboardRepo.RepoDashboardEntidadCantidad(FechaInicial, FechaFinal);
            return response;
        }

        [HttpGet("GetAgenteCantidad")]
        public List<GetEntidadCantidadResponse> GetAgenteCantidad(DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetEntidadCantidadResponse> response = _dashboardRepo.RepoDashboardAgenteCantidad(FechaInicial, FechaFinal);
            return response;
        }

        [HttpGet("GetCasosCriticos")]
        public List<GetDashboardCasosCriticosResponse> GetCasosCriticos(DateTime FechaInicial, DateTime FechaFinal)
        {

            List<GetDashboardCasosCriticosResponse> response = _dashboardRepo.RepoDashboardCasosCriticos(FechaInicial, FechaFinal);
            return response;
        }

    }

}