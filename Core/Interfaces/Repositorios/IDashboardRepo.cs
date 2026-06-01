using Core.response;

namespace Core.Interfaces.Repositorios
{
    public interface IDashboardRepo
    {
        GetTotalDashboardResponse RepoDashboardTotalCasos(DateTime FechaInicial, DateTime FechaFinal);
        GetTotalDashboardResponse RepoDashboardTotalRegistros(DateTime FechaInicial, DateTime FechaFinal, string? EntidadId);
        GetTotalDashboardResponse RepoDashboardMisCasos(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID);
        GetTotalDashboardResponse RepoDashboardAlertas(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID);

        //Graficas Dashboard 1
        List<GetDashboardEstadoResponse> RepoDashboardEstados(DateTime FechaInicial, DateTime FechaFinal, string? UsuarioID);
        List<GetDashboardEstadoResponse> RepoDashboardEstadoNNA(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID);
        List<GetDashboardEstadoResponse> RepoDashboardAlerta(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID);
        List<GetDashboardEstadoResponse> RepoDashboardIntentos(DateTime FechaInicial, DateTime FechaFinal, string UsuarioID);
        List<GetDashboardFechaTotalResponse> RepoDashboardAsignadosPorFecha(DateTime fechaInicial, DateTime fechaFinal, string UsuarioID);
        List<GetEntidadCantidadResponse> RepoDashboardEntidadCantidad(DateTime fechaInicial, DateTime fechaFinal);
        List<GetEntidadCantidadResponse> RepoDashboardAgenteCantidad(DateTime FechaInicial, DateTime FechaFinal);

        List<GetDashboardCasosCriticosResponse> RepoDashboardCasosCriticos(DateTime fechaInicio, DateTime fechaFin, string? UsuarioID);

        GetTotalDashboardResponse RepoDashboardTotalSeguimientosCuidador(DateTime FechaInicial, DateTime FechaFinal);

        GetTotalDashboardResponse RepoDashboardRegistrosPropios(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId);

        GetTotalDashboardResponse RepoDashboardTotalAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId);

        List<GetDashboardEstadoResponse> RepoDashboardAlertasEAPB(DateTime FechaInicial, DateTime FechaFinal, int EAPBId);
        GetDashboardTipoCasosResponse RepoDashboardTipoCasos(DateTime FechaInicial, DateTime FechaFinal, int? EntidadId);
        List<GetDashboardCasosCriticosEapbResponse> RepoDashboardCasosCriticosEAPB(string EntidadId, DateTime FechaInicial, DateTime FechaFinal);

        // BUG-LZ-087: resolver TPEAPB.Id a partir del NIT del usuario logueado (enterpriseIdentification)
        // para que el dashboard EAPB deje de usar el id hardcoded "2" del frontend.
        int? GetEAPBIdByNit(long nit);
    }
}
