using Microsoft.AspNetCore.Mvc;
using Core.Interfaces.Repositorios;
using Core.Request;
using Core.response;

namespace MSSeguimiento.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IntentoController : ControllerBase
    {
        private readonly IIntentoRepo _intentoRepo;

        public IntentoController(IIntentoRepo intentoRepo)
        {
            _intentoRepo = intentoRepo;
        }

        [HttpGet("GetIntentoUsuario")]
        public List<GetIntentoResponse> GetIntentoUsuario(int UsuarioId, DateTime FechaInicial, DateTime FechaFinal)
        {
            List<GetIntentoResponse> response = _intentoRepo.RepoIntentoUsuario(UsuarioId, FechaInicial, FechaFinal);
            return response;
        }

        [HttpPost("PostIntento")]
        public ActionResult<int> PostIntento(PostIntentoRequest request)
        {
            var result = _intentoRepo.RepoInsertarIntento(request);
            return Ok(result);
        }

        [HttpPut("PutIntentoActualizacionFecha")]
        public int PutIntentoActualizacionFecha(PutIntentoActualizacionFechaRequest request)
        {
            return _intentoRepo.RepoIntentoActualizacionFecha(request);
        }

        [HttpPut("PutIntentoUsuarioActualizacionUsuario")]
        public int PutIntentoUsuarioActualizacionUsuario(PutIntentoActualizacionUsuarioRequest request)
        {
            return _intentoRepo.RepoIntentoActualizacionUsuario(request);
        }

        [HttpGet("GetIntentoContactoAgrupado")]
        public List<GetIntentoContactoAgrupadoResponse> GetIntentoContactoAgrupado(int NNAId)
        {
            List<GetIntentoContactoAgrupadoResponse> response = _intentoRepo.RepoIntentoContactoAgrupado(NNAId);
            return response;
        }

        [HttpGet("GetIntentosContactoNNA")]
        public List<GetContactoNNAIntentoResponse> GetIntentosContactoNNA(int NNAId)
        {
            List<GetContactoNNAIntentoResponse> response = _intentoRepo.RepoIntentosContactoNNA(NNAId);
            return response;
        }
    }
}
