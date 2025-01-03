using Core.DTOs;
using Core.Modelos;
using Core.Request;

namespace Core.Interfaces.Repositorios
{
    public interface IAlertaRepo
    {
        public string CrearAlertaSeguimiento(CrearAlertaSeguimientoRequest request);
        public string GestionarAlerta(GestionarAlertaRequest request);
        Task<AlertaSeguimientoDto[]> ConsultarAlertasUltimoSeguimiento(int idNNA);
        List<AlertaSeguimiento> ConsultarAlertaSeguimiento(ConsultarAlertasRequest request);
        List<AlertaSeguimiento> ConsultarAlertaEstados(ConsultarAlertasEstadosRequest request);
    }
}
