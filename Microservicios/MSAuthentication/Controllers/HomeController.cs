using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SISPRO.TRV.Entity;
using SISPRO.TRV.Web.MVCCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MSAuthentication.Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class HomeController : Controller
    {
        // GET: api/<HomeController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            User user = this.GetUser();

            bool IsMinSalud = user.Enterprise.Identification.IsEP2 || user.Enterprise.Identification.IsNITMinSalud;
            string TipoIdEntidadUsuarioSesion = user.Enterprise.Identification.TypeCode;
            var NroIdEntidadUsuarioSesion = user.Enterprise.Identification.NumberAsLong;
            string LoginUser = user.Alias;
            List<BasicReference> Perfiles = user.UserGroups;

            return new string[] { "value1", "value2" };
        }
    }
}
