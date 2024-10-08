using Core.DTOs;
using Core.Interfaces.Repositorios;
using Microsoft.EntityFrameworkCore;
using SISPRO.TRV.Entity;

namespace Infra.Repositorios
{
    public class AuthRepo(ApplicationDbContext db) : IAuthRepo
    {
        public async Task<UserDto?> GetUser(User data)
        {
            var user = await db.AspNetUsers.FirstOrDefaultAsync(x => x.Alias == data.Alias);
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Alias = data.Alias,
                Email = data.Email,
                Name = data.calFullName,
                State = true,
                RolCode = data.UserGroups.Select(x => x.Code).ToArray(),
                EnterpriseCode = data.Enterprise.Code,
                EnterpriseDeptoCode = data.Enterprise.DeptoCode,
                EnterpriseEmail = data.Enterprise.Email,
                EnterpriseName = data.Enterprise.Name,
                EnterpriseIdentification = data.Enterprise.Identification.Number,
                IsMinSalud = data.Enterprise.Identification.IsEP2 || data.Enterprise.Identification.IsNITMinSalud,
                IsAuth = true
            };
        }
    }
}
