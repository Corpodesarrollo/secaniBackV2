using Core.DTOs;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class CuidadorRepo(ApplicationDbContext db) : ICuidadorRepo
    {
        public async Task<bool> SetUserCuidador(string id)
        {
            var user = await db.ApplicationUser.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            user.EsCuidador = true;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CuidadorDto>> GetAllCuidadores()
        {
            var result = await db.ApplicationUser.Where(x => x.EsCuidador).Select(x => new CuidadorDto
            {
                Id = x.Id,
                Identificacion = x.Alias,
                NombreCompleto = x.FullName,
                Telefono = x.Telefonos,
                Correo = x.Email
            }).ToArrayAsync();

            return result;
        }
    }
}
