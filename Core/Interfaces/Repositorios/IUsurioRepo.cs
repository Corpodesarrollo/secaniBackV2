using Core.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Repositorios
{
    public interface IUsurioRepo
    {
        public UltimoRol UltimoRolPorIdUsuario(string IdUsuario);

        // BUG-LZ-015: lista usuarios con rol Agente de seguimiento
        Task<List<AgenteSeguimientoResponse>> GetAgentesSeguimientoAsync(CancellationToken cancellationToken = default);
    }

    public class AgenteSeguimientoResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Alias { get; set; }
        public string? Telefonos { get; set; }
        public string? EntidadId { get; set; }
        public string? Cargo { get; set; }
        public bool? Activo { get; set; }
    }
}
