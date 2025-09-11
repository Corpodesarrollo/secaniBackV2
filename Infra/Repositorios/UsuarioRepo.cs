using Core.Interfaces.Repositorios;
using Core.Modelos;
using Core.Response;
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
    }
}
