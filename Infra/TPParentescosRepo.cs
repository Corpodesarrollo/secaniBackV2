using Core.DTOs;
using Core.Interfaces;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Infra
{
    public class TPParentescosRepo(ApplicationDbContext _context) : ITPParentescos
    {
        public async Task<IEnumerable<TPParentescosDto>> GetAllAsync()
        {
            var response = await _context.TPParentescos.ToListAsync();
            return response.Adapt<IEnumerable<TPParentescosDto>>();
        }
    }
}
