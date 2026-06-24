using Core.Common;
using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Mvc;
using static Core.Common.Estructuras;

namespace MSSeguimiento.Api.Controllers
{
    public class SeguimientoController : BaseController
    {
        private readonly ISeguimientoRepo seguimientoRepo;
        private readonly IWebHostEnvironment _env;
        private readonly INotificacionRepo notificacionRepo;

        public SeguimientoController(ISeguimientoRepo seguimiento, IWebHostEnvironment env, INotificacionRepo notificacion)
        {
            seguimientoRepo = seguimiento;
            _env = env;
            notificacionRepo = notificacion;
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

        [HttpGet("GetCntSeguimientoByNNA/{idNNA}")]
        public async Task<long> GetCntSeguimientoByNNA(long idNNA)
        {

            var response = await seguimientoRepo.GetCntSeguimientoByNNA(idNNA);
            return response;
        }

        [HttpGet("SeguimientoNNA/{IdNNA}")]
        public async Task<SeguimientoDatosNNADto?> SeguimientoNNA(long IdNNA)
        {

            var response = await seguimientoRepo.SeguimientoNNA(IdNNA);
            return response;
        }

        [HttpGet("{id}")]
        public ActionResult<Seguimiento> GetById(long id)
        {
            var seguimiento = seguimientoRepo.GetById(id);
            if (seguimiento == null)
            {
                return NotFound(); // Retorna 404 si no se encuentra el registro
            }
            return Ok(seguimiento); // Retorna 200 con el registro encontrado
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
        public async Task<int> PutSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            var result = seguimientoRepo.RepoSeguimientoActualizacionUsuario(request);

            // BUG-LZ-090: la reasignacion no notificaba al nuevo agente (por eso qa-agente-005
            // tenia casos reasignados pero 0 notificaciones). Notificar al destino si reasigno OK.
            if (result == 1)
            {
                try
                {
                    await notificacionRepo.SetNotificacion(new()
                    {
                        TipoNotificacion = TipoNotificacion.AsignacionReasignacion,
                        IdAgenteDestino = request.UsuarioId,
                        IdSeguimiento = request.Id
                    });
                }
                catch { /* la notificacion no debe afectar la reasignacion */ }
            }

            return result;
        }

        [HttpGet("GetSeguimientoFestivos")]
        public List<GetSeguimientoFestivoResponse> GetSeguimientoFestivos(DateTime FechaInicial, DateTime FechaFinal, string UsuarioId)
        {

            List<GetSeguimientoFestivoResponse> response = seguimientoRepo.RepoSeguimientoFestivo(FechaInicial, FechaFinal, UsuarioId);
            return response;
        }

        [HttpGet("GetSeguimientoHorarioAgente")]
        public List<GetSeguimientoHorarioAgenteResponse> GetSeguimientoHorarioAgente(string UsuarioId)
        {

            List<GetSeguimientoHorarioAgenteResponse> response = seguimientoRepo.RepoSeguimientoHorarioAgente(UsuarioId);
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

        [HttpGet("GetSeguimientosByNNA/{idNNA}")]
        public async Task<ActionResult> GetSeguimientosByNNA(int idNNA)
        {

            var response = await seguimientoRepo.GetSeguimientosByNNA(idNNA);
            return Ok(response);
        }

        // HU SECANI-RQ06-HU01: historico de asignaciones/reasignaciones del NNA para
        // mostrar en pestaña Trazabilidad/Seguimiento del detalle.
        [HttpGet("HistorialAsignacionesNNA/{nnaId}")]
        public async Task<ActionResult> HistorialAsignacionesNNA(long nnaId)
        {
            var response = await seguimientoRepo.GetHistorialAsignacionesNNA(nnaId);
            return Ok(response);
        }

        [HttpGet("GetSeguimientosNNA/{idNNA}")]
        public List<SeguimientoNNAResponse> GetSeguimientosNNA(int idNNA)
        {

            List<SeguimientoNNAResponse> response = seguimientoRepo.GetSeguimientosNNA(idNNA);
            return response;
        }

        [HttpGet("NNa/{id}")]
        public GetNNaParcialResponse GetNNaById(long id)
        {
            var seguimiento = seguimientoRepo.GetNNaById(id);
            return seguimiento;
        }

        [HttpPost("SetSeguimiento")]
        public async Task<ActionResult> SetSeguimiento(SetSeguimientoRequest request)
        {
            try
            {
                var response = await seguimientoRepo.SetSeguimiento(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("PutSeguimientoRechazo")]
        public int PutSeguimientoRechazo(PutSeguimientoRechazoRequest request)
        {
            return seguimientoRepo.RepoSeguimientoRechazo(request);
        }

        [HttpGet("AsignacionAutomatica")]
        public async Task AsignacionAutomatica()
        {
            await seguimientoRepo.AsignacionAutomatica();
        }

        [HttpPost("CrearPlantillaCorreo")]
        public IActionResult CrearPlantillaCorreo(CrearPlantillaCorreoRequest request)
        {
            var response = seguimientoRepo.CrearPlantillaCorreo(request);

            return Ok(response);
        }

        [HttpPost("EliminarPlantillaCorreo")]
        public IActionResult EliminarPlantillaCorreo(EliminarPlantillaCorreoRequest request)
        {
            var response = seguimientoRepo.EliminarPlantillaCorreo(request);

            return Ok(response);
        }

        [HttpGet("ConsultarPlantillaCorreo")]
        public IActionResult ConsultarPlantillaCorreo()
        {
            var response = seguimientoRepo.ConsultarPlantillasCorreo();

            return Ok(response);
        }

        [HttpGet("ConsultarPlantillaCorreo/{id}")]
        public IActionResult ConsultarUnaPlantillaCorreo(long id)
        {
            var response = seguimientoRepo.ConsultarUnaPlantillasCorreo(id);

            return Ok(response);
        }

        [HttpGet("HistoricoPlantillaCorreo/{id}")]
        public IActionResult HistoricoPlantillaCorreo(long id)
        {
            var response = seguimientoRepo.HistoricoPlantillaCorreo(id);

            return Ok(response);
        }

        [HttpGet("ExportarDetalleSeguimiento/{id}")]
        public IActionResult ExportarDetalleSeguimiento(long id)
        {
            var response = seguimientoRepo.ExportarDetalleSeguimiento(id);
            return Ok(response);
        }

        [HttpGet("GetSeguimientosEstados/{id}")]
        public async Task<ActionResult> GetSeguimientosEstados(string id)
        {
            try
            {
                var response = await seguimientoRepo.GetSeguimientosEstados(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetSeguimientosCuidador/{id}")]
        public async Task<ActionResult> GetSeguimientosCuidador(string id)
        {
            try
            {
                var response = await seguimientoRepo.GetSeguimientosCuidador(id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}


