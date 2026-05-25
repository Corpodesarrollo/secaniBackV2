using Core.Common;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    public class AlertaController : BaseController
    {
        private readonly IAlertaRepo alertaRepo;

        public AlertaController(IAlertaRepo alerta)
        {
            alertaRepo = alerta;
        }

        [HttpPost("CrearAlertaSeguimiento")]
        public string CrearAlerta([FromBody] CrearAlertaSeguimientoRequest request)
        {
            return alertaRepo.CrearAlertaSeguimiento(request);
        }

        [HttpPost("GestionarAlerta")]
        public string GestionarAlerta(GestionarAlertaRequest request)
        {
            return alertaRepo.GestionarAlerta(request);
        }

        [HttpPost("ConsultarAlertasSeguimiento")]
        public List<AlertaSeguimiento> ConsultarAlertaSeguimiento(ConsultarAlertasRequest request)
        {
            return alertaRepo.ConsultarAlertaSeguimiento(request);
        }

        [HttpPost("ConsultarAlertasEstados")]
        public List<AlertaSeguimiento> ConsultarAlertaEstados(ConsultarAlertasEstadosRequest request)
        {
            return alertaRepo.ConsultarAlertaEstados(request);
        }

        [HttpGet("ConsultarAlertasUltimoSeguimiento/{idNNA}")]
        public async Task<AlertaSeguimientoDto[]> ConsultarAlertasUltimoSeguimiento(int idNNA)
        {
            return await alertaRepo.ConsultarAlertasUltimoSeguimiento(idNNA);
        }

        [HttpGet("Exportar/{idAlerta}")]
        public async Task<IActionResult> Exportar(int idAlerta)
        {
            var result = await alertaRepo.Exportar(idAlerta);

            if (result.Item1 == null)
                return NotFound("El archivo no existe o está vacío.");

            return File(result.Item1,
                        "application/zip",
                        $"{Path.GetFileNameWithoutExtension(result.Item2)}.zip");
        }
    }
}
