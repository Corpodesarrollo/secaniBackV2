using Core.DTOs;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositorios
{
    public class EAPBRepo(ApplicationDbContext db) : IEAPBRepo
    {
        IQueryable<TPEAPBDto> GetSelect()
        {
            var query = from eapb in db.TPEAPB
                        select new TPEAPBDto
                        {
                            Id = eapb.Id,
                            Codigo = eapb.Codigo,
                            Nombre = eapb.Nombre,
                            Descripcion = eapb.Descripcion,
                            NIT = eapb.NIT,
                            DV = eapb.DV,
                            Creation = eapb.Creation,
                            LastUpdate = eapb.LastUpdate,
                            Tipo = eapb.Tipo
                        };
            return query;
        }

        public async Task<List<TPEAPBDto>> GetEAPB()
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Tipo == 2).ToListAsync();
            return result;
        }

        public async Task<List<TPEAPBDto>> GetET()
        {
            var query = GetSelect();
            var result = await query.Where(x => x.Tipo == 1).ToListAsync();
            return result;
        }

        public async Task<List<TPEAPBDto>> Entidates()
        {
            var query = GetSelect();
            var result = await query.ToListAsync();
            return result;
        }
    }
}
