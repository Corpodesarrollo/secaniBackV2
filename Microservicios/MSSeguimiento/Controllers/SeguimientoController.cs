using Core.DTOs;
using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Request;
using Core.response;
using Core.Response;
using Microsoft.AspNetCore.Mvc;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("[controller]")]
    public class SeguimientoController : ControllerBase
    {
        private readonly ISeguimientoRepo seguimientoRepo;
        private readonly IWebHostEnvironment _env;

        public SeguimientoController(ISeguimientoRepo seguimiento, IWebHostEnvironment env)
        {
            seguimientoRepo = seguimiento;
            _env = env;
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
        public int PutSeguimientoActualizacionUsuario(PutSeguimientoActualizacionUsuarioRequest request)
        {
            return seguimientoRepo.RepoSeguimientoActualizacionUsuario(request);
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
        public string SetSeguimiento(SetSeguimientoRequest request)
        {
            string response = seguimientoRepo.SetSeguimiento(request);

            return response;
        }

        [HttpPut("PutSeguimientoRechazo")]
        public int PutSeguimientoRechazo(PutSeguimientoRechazoRequest request)
        {
            return seguimientoRepo.RepoSeguimientoRechazo(request);
        }

        [HttpGet("AsignacionAutomatica")]
        public void AsignacionAutomatica()
        {
            seguimientoRepo.AsignacionAutomatica();
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

        [HttpGet("HistoricoPlantillaCorreo/{id}")]
        public IActionResult HistoricoPlantillaCorreo(string id)
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
    }
}


