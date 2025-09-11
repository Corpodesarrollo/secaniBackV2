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
            bool isCoordinadorAdmin = false;
            bool isAgenteSeguimiento = false;
            bool isCuidador = false;
            bool isET = false;
            bool isEAPB = false;
            string rolSecani = "";
            var user = await db.ApplicationUser.FirstOrDefaultAsync(x => x.Alias == data.Alias);
            if (data.UserGroups.Any(x => x.Code == "SECANI-CoordinadorAdmin"))
            {
                isCoordinadorAdmin = true;
                rolSecani = "CoordinadorAdmin";
            } 
            else if (data.UserGroups.Any(x => x.Code == "SECANI-AgenteSeguimiento"))
            {
                isAgenteSeguimiento = true;
                rolSecani = "AgentesDeSeguimiento";
            } 
            else if (data.Enterprise.Identification.TypeCode == "CC")
            {
                isCuidador = true;
                rolSecani = "Cuidador";
            } 
            else if (data.Enterprise.Identification.TypeCode == "MU" || data.Enterprise.Identification.TypeCode == "DE" || data.Enterprise.Identification.TypeCode == "DI")
            {
                isET = true;
                rolSecani = "Externos";
            }
            else if (data.Enterprise.Identification.TypeCode == "NI" || data.Enterprise.Identification.TypeCode == "EPS" || data.Enterprise.Identification.TypeCode == "IPS")
            {
                isEAPB = true;
                rolSecani = "Externos";
            }
            var rol = db.Roles.FirstOrDefault(x => x.NormalizedName == rolSecani);
            if (rol != null)
            {
                rolSecani = rol.Id;
            }
            return new UserDto
            {
                Id = (user != null)? user.Id:"",
                IdRol = rolSecani,
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
                IsCoordinadorAdmin = isCoordinadorAdmin,
                IsAgenteSeguimiento = isAgenteSeguimiento,
                IsCuidador = isCuidador,
                IsET = isET,
                IsEAPB = isEAPB
            };
        }
    }
}
