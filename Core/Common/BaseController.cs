using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Common
{

    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class BaseController() : Controller { }
}
