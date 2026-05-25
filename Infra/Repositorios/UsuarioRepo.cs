using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repositorios
{
    public class UsuarioRepo(ApplicationDbContext context) : IUsurioRepo
    {
        private readonly ApplicationDbContext _context = context;
        // Rol Agente de seguimiento (constante usada en SeguimientoRepo)
        private const string ROLE_AGENTE_SEGUIMIENTO = "14CDDEA5-FA06-4331-8359-036E101C5046";

        public UltimoRol UltimoRolPorIdUsuario(string IdUsuario)
        {
            var urol = _context.UserRoles
                .Where(ur => ur.UserId == IdUsuario)
                .FirstOrDefault();

            if (urol != null)
            {
                UltimoRol ultimoRol = new UltimoRol();
                ultimoRol.Id = urol.RoleId ?? "";
                var rol = _context.Roles
                    .Where(r => r.Id == urol.RoleId)
                    .FirstOrDefault();
                if (rol != null)
                {
                    ultimoRol.Name = rol.Name;
                }
                return ultimoRol;
            }
            return null;
        }

        // BUG-LZ-015: usuarios con rol Agente de seguimiento (uso del Coordinador)
        public async Task<List<AgenteSeguimientoResponse>> GetAgentesSeguimientoAsync(CancellationToken cancellationToken = default)
        {
            return await (from ur in _context.UserRoles
                          join u in _context.Users on ur.UserId equals u.Id
                          where ur.RoleId == ROLE_AGENTE_SEGUIMIENTO
                          orderby u.FullName
                          select new AgenteSeguimientoResponse
                          {
                              Id = u.Id,
                              FullName = u.FullName,
                              Email = u.Email,
                              Alias = u.Alias,
                              Telefonos = u.Telefonos,
                              EntidadId = u.EntidadId,
                              Cargo = u.Cargo,
                              Activo = u.Activo
                          }).ToListAsync(cancellationToken);
        }
    }
}
