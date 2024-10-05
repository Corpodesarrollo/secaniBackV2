using Microsoft.AspNetCore.Mvc;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;
using Core.Modelos;
using Infra.Repositorios;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepo _dashboardRepo;

        public DashboardController(IDashboardRepo dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }

        
        [HttpGet("GetTotalCasos")]
        public GetTotalDashboardResponse GetTotalCasos(DateTime FechaInicial, DateTime FechaFinal )
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardTotalCasos(FechaInicial, FechaFinal);
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
        public List<GetDashboardCasosCriticosResponse> GetCasosCriticos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID)
        {

            List<GetDashboardCasosCriticosResponse> response = _dashboardRepo.RepoDashboardCasosCriticos(FechaInicial, FechaFinal, UsuarioID);
            return response;
        }

        [HttpGet("GetTotalSeguimientosCuidador")]
        public GetTotalDashboardResponse GetTotalSeguimientosCuidador(DateTime FechaInicial, DateTime FechaFinal)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardTotalSeguimientosCuidador(FechaInicial, FechaFinal);
            return response;
        }

        [HttpGet("GetRegistrosPropios")]
        public GetTotalDashboardResponse GetRegistrosPropios(DateTime FechaInicial, DateTime FechaFinal, int EntidadId)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardRegistrosPropios(FechaInicial, FechaFinal, EntidadId);
            return response;
        }

        [HttpGet("GetTotalAlertasEAPB")]
        public GetTotalDashboardResponse GetTotalAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int EntidadId)
        {

            GetTotalDashboardResponse response = _dashboardRepo.RepoDashboardTotalAlertasEAPB(FechaInicial, FechaFinal, EntidadId);
            return response;
        }

        [HttpGet("GetEstadosAlertasEAPB")]
        public List<GetDashboardEstadoResponse> GetEstadosAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int EntidadId)
        {

            List<GetDashboardEstadoResponse> response = _dashboardRepo.RepoDashboardAlertasEAPB(FechaInicial, FechaFinal, EntidadId!);
            return response;
        }


        [HttpGet("GetTiposCasos")]
        public GetDashboardTipoCasosResponse GetTiposCasos(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId)
        {

            GetDashboardTipoCasosResponse response = _dashboardRepo.RepoDashboardTipoCasos(FechaInicial, FechaFinal, EntidadId!);
            return response;
        }

        [HttpGet("GetCasosCriticosEAPB")]
        public List<GetDashboardCasosCriticosEapbResponse> GetCasosCriticosEAPB( int EntidadId)
        {

            List<GetDashboardCasosCriticosEapbResponse> response = _dashboardRepo.RepoDashboardCasosCriticosEAPB( EntidadId!);
            return response;
        }

    }

}