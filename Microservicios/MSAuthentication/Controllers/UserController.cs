using Core.Common;
using Core.CQRS.MSUsuariosyRoles.Commands.User;
using Core.CQRS.MSUsuariosyRoles.Queries.User;
using Core.DTOs.MSUsuariosyRoles;
using Core.Interfaces.Repositorios;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;


namespace MSAuthentication.Api.Controllers
{
    public class UserController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IUsurioRepo _usurioRepo;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserController(IMediator mediator, IUsurioRepo usurioRepo, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _mediator = mediator;
            _usurioRepo = usurioRepo;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // BUG-003: Consultar usuarios SISPRO con fallback a BD local
        [HttpGet("GetAllFromSispro")]
        public async Task<IActionResult> GetAllFromSispro([FromQuery] string? role = null)
        {
            var baseUrl = _configuration["SisproApi:BaseUrl"];
            var apiKey = _configuration["SisproApi:ApiKey"];

            if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(apiKey))
            {
                try
                {
                    var endpoint = string.IsNullOrEmpty(role)
                        ? "api/UsuarioInstitucional/GetAllByApp"
                        : $"api/UsuarioInstitucional/GetAllByRole?role={Uri.EscapeDataString(role)}";

                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("ApiKey", apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var response = await client.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrWhiteSpace(json))
                        {
                            using var doc = JsonDocument.Parse(json);
                            return Ok(doc.RootElement.Clone());
                        }
                    }
                    Console.WriteLine($"SISPRO API returned {(int)response.StatusCode}. Fallback to local DB.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SISPRO API error: {ex.Message}. Fallback to local DB.");
                }
            }

            // Fallback: retornar usuarios locales
            var localUsers = await _mediator.Send(new GetAllUsersDetailsQuery());
            return Ok(localUsers);
        }

        [HttpPost("Create")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> CreateUser(CreateUserCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        [HttpGet("GetAll")]
        [ProducesDefaultResponseType(typeof(List<UserResponseDTO>))]
        public async Task<IActionResult> GetAllUserAsync()
        {
            return Ok(await _mediator.Send(new GetUserQuery()));
        }

        [HttpDelete("Delete/{userId}")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _mediator.Send(new DeleteUserCommand() { Id = userId });
            return Ok(result);
        }

        [HttpGet("GetUserDetails/{userId}")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetails(string userId)
        {
            try
            {
                var result = await _mediator.Send(new GetUserDetailsQuery() { UserId = userId });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserDetailsByUserName/{userName}")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetUserDetailsByUserName(string userName)
        {
            var result = await _mediator.Send(new GetUserDetailsByUserNameQuery() { UserName = userName });
            return Ok(result);
        }

        [HttpPost("AssignRoles")]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> AssignRoles(AssignUsersRoleCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("EditUserRoles")]
        [ProducesDefaultResponseType(typeof(int))]

        public async Task<ActionResult> EditUserRoles(UpdateUserRolesCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("GetAllUserDetails")]
        [ProducesDefaultResponseType(typeof(UserDetailsResponseDTO))]
        public async Task<IActionResult> GetAllUserDetails()
        {
            var result = await _mediator.Send(new GetAllUsersDetailsQuery());
            return Ok(result);
        }


        [HttpPut("EditUserProfile/{id}")]
        [ProducesDefaultResponseType(typeof(int))]
        public async Task<ActionResult> EditUserProfile(string id, [FromBody] EditUserProfileCommand command)
        {
            if (id == command.Id)
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("GetUserRole/{userId}")]
        public IActionResult GetUserRole(string userId)
        {
            var result = _usurioRepo.UltimoRolPorIdUsuario(userId);
            return Ok(result);
        }

    }
}
