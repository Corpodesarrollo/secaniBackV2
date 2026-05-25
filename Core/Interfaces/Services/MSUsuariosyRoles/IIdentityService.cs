namespace Core.Interfaces.Services.MSUsuariosyRoles
{
    public interface IIdentityService
    {
        // User section
        Task<(bool isSucceed, string userId)> CreateUserAsync(string userName, string password, string email, string fullName, List<string> roles, string alias, string Telefonos = "", string EntidadId = "", string Cargo = "");
        Task<bool> SigninUserAsync(string userName, string password);
        Task<string> GetUserIdAsync(string userName);
        Task<string> GetUserNameAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> IsUniqueUserName(string userName);
        Task<bool> UpdateUserProfile(string id, string fullName, string email, string Telefonos = "", string EntidadId = "", string Cargo = "", bool? Activo = false);
        Task<bool> IsUnicoAgenteActivo(string userId);

        // Role Section
        Task<bool> CreateRoleAsync(string roleName);
        Task<bool> DeleteRoleAsync(string roleId);
        Task<List<(string id, string roleName)>> GetRolesAsync();
        Task<(string id, string roleName)> GetRoleByIdAsync(string id);
        Task<bool> UpdateRole(string id, string roleName);

        // User's Role section
        Task<bool> IsInRoleAsync(string userId, string role);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> AssignUserToRole(string userName, IList<string> roles);
        Task<bool> UpdateUsersRole(string userName, IList<string> usersRole);
        Task<(string userId, string fullName, string UserName, string email, string telefonos, string entidadId, string cargo, bool Estado, IList<string> roles)> GetUserDetailsAsync(string userId);
        Task<List<(string id, string fullName, string userName, string email, string telefonos, string entidadId, string cargo, bool? activo)>> GetAllUsersAsync();
        Task<(string userId, string fullName, string UserName, string email, string telefonos, string entidadId, string cargo, bool? Estado, IList<string> roles)> GetUserDetailsByUserNameAsync(string userName);
    }
}
