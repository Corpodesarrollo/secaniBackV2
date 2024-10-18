using Core.Modelos;
using Core.Request;

namespace Core.Interfaces.Repositorios
{
    public interface IAlertaRepo
    {
        public string CrearAlertaSeguimiento(CrearAlertaSeguimientoRequest request);
        public string GestionarAlerta(GestionarAlertaRequest request);
        public List<AlertaSeguimiento> ConsultarAlertaSeguimiento(ConsultarAlertasRequest request);

        public List<AlertaSeguimiento> ConsultarAlertaEstados(ConsultarAlertasEstadosRequest request);
    }
}
